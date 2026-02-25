using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;
using MovieRentalApp.Repositories;

namespace MovieRentalApp.Services
{
    public class RentalService : IRentalService
    {
        private readonly RentalRepository _rentalRepository;
        private readonly MovieRepository _movieRepository;
        private readonly UserRepository _userRepository;
        private readonly PaymentRepository _paymentRepository;

        public RentalService(
            RentalRepository rentalRepository,
            MovieRepository movieRepository,
            UserRepository userRepository,
            PaymentRepository paymentRepository)
        {
            _rentalRepository = rentalRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<RentalResponseDto> RentMovie(RentalCreateDto dto)
        {
            var user = await _userRepository.Get(dto.UserId);
            if (user == null)
                throw new EntityNotFoundException("User", dto.UserId);

            var movie = await _movieRepository.Get(dto.MovieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", dto.MovieId);

            if (movie.AvailableCopies <= 0)
                throw new BusinessRuleViolationException(
                    $"'{movie.Title}' is currently not available for rent.");

            if (await _rentalRepository.IsMovieAlreadyRentedByUser(dto.UserId, dto.MovieId))
                throw new BusinessRuleViolationException(
                    $"You already have '{movie.Title}' rented.");

            var rental = new Rental
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId,
                RentalDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(dto.DurationDays),
                Status = "Active"
            };

            var createdRental = await _rentalRepository.Add(rental);
            if (createdRental == null)
                throw new UnableToCreateEntityException("Rental");

            // Reduce available copies
            movie.AvailableCopies--;
            await _movieRepository.Update(movie.Id, movie);

            // Auto create payment
            var payment = new Payment
            {
                UserId = dto.UserId,
                Amount = movie.RentalPrice * dto.DurationDays,
                Method = "Online",
                Status = "Completed",
                PaymentDate = DateTime.UtcNow
            };
            await _paymentRepository.Add(payment);

            return MapToDto(createdRental, movie, user);
        }

        public async Task<RentalResponseDto> ReturnMovie(int rentalId)
        {
            var rental = await _rentalRepository.Get(rentalId);
            if (rental == null)
                throw new EntityNotFoundException("Rental", rentalId);

            if (rental.Status == "Returned")
                throw new BusinessRuleViolationException(
                    "This rental has already been returned.");

            rental.Status = "Returned";
            var updated = await _rentalRepository.Update(rentalId, rental);

            var movie = await _movieRepository.Get(rental.MovieId);
            if (movie != null)
            {
                movie.AvailableCopies++;
                await _movieRepository.Update(movie.Id, movie);
            }

            return MapToDto(updated!, rental.Movie!, rental.User!);
        }

        public async Task<RentalResponseDto> GetRental(int id)
        {
            var rental = await _rentalRepository.Get(id);
            if (rental == null)
                throw new EntityNotFoundException("Rental", id);

            return MapToDto(rental, rental.Movie!, rental.User!);
        }

        public async Task<IEnumerable<RentalResponseDto>> GetRentalsByUser(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            var rentals = await _rentalRepository.GetRentalsByUser(userId);
            return rentals.Select(r => MapToDto(r, r.Movie!, r.User!));
        }

        public async Task<IEnumerable<RentalResponseDto>> GetActiveRentals()
        {
            var rentals = await _rentalRepository.GetActiveRentals();
            return rentals.Select(r => MapToDto(r, r.Movie!, r.User!));
        }

        private static RentalResponseDto MapToDto(Rental rental, Movie movie, User user) => new()
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