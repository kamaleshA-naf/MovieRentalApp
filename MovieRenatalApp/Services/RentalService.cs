using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, User> _userRepository;
        private readonly IRepository<int, Payment> _paymentRepository;

        public RentalService(
            IRepository<int, Rental> rentalRepository,
            IRepository<int, Movie> movieRepository,
            IRepository<int, User> userRepository,
            IRepository<int, Payment> paymentRepository)
        {
            _rentalRepository = rentalRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<RentalResponseDto> RentMovie(RentalCreateDto dto)
        {
            // Step 1 - Validate user
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new EntityNotFoundException("User", dto.UserId);

            // Step 2 - Validate movie
            var movie = await _movieRepository.GetByIdAsync(dto.MovieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", dto.MovieId);

            // Step 3 - Check availability
            if (movie.AvailableCopies <= 0)
                throw new BusinessRuleViolationException(
                    $"'{movie.Title}' is not available for rent.");

            // Step 4 - Check duplicate active rental
            var existing = await _rentalRepository.FindAsync(
                r => r.UserId == dto.UserId &&
                     r.MovieId == dto.MovieId &&
                     r.Status == "Active");
            if (existing.Any())
                throw new BusinessRuleViolationException(
                    $"You already have '{movie.Title}' rented.");

            // Step 5 - Create rental
            var rental = new Rental
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId,
                RentalDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(dto.DurationDays),
                Status = "Active"
            };
            await _rentalRepository.AddAsync(rental);

            // Step 6 - Reduce available copies
            movie.AvailableCopies--;
            await _movieRepository.UpdateAsync(movie.Id, movie);

            // Step 7 - Auto create payment
            await _paymentRepository.AddAsync(new Payment
            {
                UserId = dto.UserId,
                Amount = movie.RentalPrice * dto.DurationDays,
                Method = "Online",
                Status = "Completed",
                PaymentDate = DateTime.UtcNow
            });

            return MapToDto(rental, movie, user);
        }

        public async Task<RentalResponseDto> ReturnMovie(int rentalId)
        {
            // Step 1 - Find rental with includes
            var rentals = await _rentalRepository.GetAllWithIncludeAsync(
                r => r.Movie, r => r.User);
            var rental = rentals.FirstOrDefault(r => r.Id == rentalId);
            if (rental == null)
                throw new EntityNotFoundException("Rental", rentalId);

            // Step 2 - Check already returned
            if (rental.Status == "Returned")
                throw new BusinessRuleViolationException(
                    "This rental has already been returned.");

            // Step 3 - Update status
            rental.Status = "Returned";
            await _rentalRepository.UpdateAsync(rentalId, rental);

            // Step 4 - Restore movie copy
            var movie = await _movieRepository.GetByIdAsync(rental.MovieId);
            if (movie != null)
            {
                movie.AvailableCopies++;
                await _movieRepository.UpdateAsync(movie.Id, movie);
            }

            return MapToDto(rental, rental.Movie!, rental.User!);
        }

        public async Task<RentalResponseDto> GetRental(int id)
        {
            // Step 1 - Find with includes
            var rentals = await _rentalRepository.GetAllWithIncludeAsync(
                r => r.Movie, r => r.User);
            var rental = rentals.FirstOrDefault(r => r.Id == id);
            if (rental == null)
                throw new EntityNotFoundException("Rental", id);

            return MapToDto(rental, rental.Movie!, rental.User!);
        }

        public async Task<IEnumerable<RentalResponseDto>> GetRentalsByUser(int userId)
        {
            // Step 1 - Get rentals with includes
            var rentals = await _rentalRepository.GetAllWithIncludeAsync(
                r => r.Movie, r => r.User);

            return rentals
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RentalDate)
                .Select(r => MapToDto(r, r.Movie!, r.User!));
        }

        public async Task<IEnumerable<RentalResponseDto>> GetActiveRentals()
        {
            // Step 1 - Get active rentals
            var rentals = await _rentalRepository.GetAllWithIncludeAsync(
                r => r.Movie, r => r.User);

            return rentals
                .Where(r => r.Status == "Active")
                .Select(r => MapToDto(r, r.Movie!, r.User!));
        }

        // ── Mapper ────────────────────────────────────────────────
        private static RentalResponseDto MapToDto(
            Rental rental, Movie movie, User user) => new()
            {
                Id = rental.Id,
                UserId = rental.UserId,
                UserName = user.UserName,
                MovieId = rental.MovieId,
                MovieTitle = movie.Title,
                RentalDate = rental.RentalDate,
                ExpiryDate = rental.ExpiryDate,
                Status = rental.Status
            };
    }
}