using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IRepository<int, Wishlist> _wishlistRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, User> _userRepository;

        public WishlistService(
            IRepository<int, Wishlist> wishlistRepository,
            IRepository<int, Movie> movieRepository,
            IRepository<int, User> userRepository)
        {
            _wishlistRepository = wishlistRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
        }

        public async Task<WishlistResponseDto> AddToWishlist(WishlistCreateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new EntityNotFoundException("User", dto.UserId);

            var movie = await _movieRepository.GetByIdAsync(dto.MovieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", dto.MovieId);

            var existing = await _wishlistRepository.FindAsync(
                w => w.UserId == dto.UserId && w.MovieId == dto.MovieId);
            if (existing.Any())
                throw new DuplicateEntityException(
                    $"'{movie.Title}' is already in your wishlist.");

            var wishlist = new Wishlist
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId,
                AddedDate = DateTime.UtcNow
            };
            await _wishlistRepository.AddAsync(wishlist);

            return MapToDto(wishlist, movie);
        }

        public async Task<IEnumerable<WishlistResponseDto>> GetWishlistByUser(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                throw new EntityNotFoundException("User", userId);

            var items = await _wishlistRepository.GetAllWithIncludeAsync(
                w => w.Movie);

            return items
                .Where(w => w.UserId == userId)
                .Select(w => MapToDto(w, w.Movie!));
        }

        public async Task<bool> RemoveFromWishlist(int wishlistId)
        {
            
            var exists = await _wishlistRepository.ExistsAsync(wishlistId);
            if (!exists)
                throw new EntityNotFoundException("Wishlist item", wishlistId);

            return await _wishlistRepository.DeleteAsync(wishlistId);
        }

        private static WishlistResponseDto MapToDto(
            Wishlist wishlist, Movie movie) => new()
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                MovieId = wishlist.MovieId,
                MovieTitle = movie.Title,
                RentalPrice = movie.RentalPrice,
                AddedDate = wishlist.AddedDate
            };
    }
}