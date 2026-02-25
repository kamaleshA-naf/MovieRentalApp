using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;
using MovieRentalApp.Repositories;

namespace MovieRentalApp.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly WishlistRepository _wishlistRepository;
        private readonly MovieRepository _movieRepository;
        private readonly UserRepository _userRepository;

        public WishlistService(
            WishlistRepository wishlistRepository,
            MovieRepository movieRepository,
            UserRepository userRepository)
        {
            _wishlistRepository = wishlistRepository;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
        }

        public async Task<WishlistResponseDto> AddToWishlist(WishlistCreateDto dto)
        {
            var user = await _userRepository.Get(dto.UserId);
            if (user == null)
                throw new EntityNotFoundException("User", dto.UserId);

            var movie = await _movieRepository.Get(dto.MovieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", dto.MovieId);

            if (await _wishlistRepository.ExistsAsync(dto.UserId, dto.MovieId))
                throw new DuplicateEntityException(
                    $"'{movie.Title}' is already in your wishlist.");

            var wishlist = new Wishlist
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId,
                AddedDate = DateTime.UtcNow    // ← trainer uses AddedDate
            };

            var created = await _wishlistRepository.Add(wishlist);
            if (created == null)
                throw new UnableToCreateEntityException("Wishlist item");

            return MapToDto(created, movie);
        }

        public async Task<IEnumerable<WishlistResponseDto>> GetWishlistByUser(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            var items = await _wishlistRepository.GetWishlistByUser(userId);
            return items.Select(w => MapToDto(w, w.Movie!));
        }

        public async Task<bool> RemoveFromWishlist(int wishlistId)
        {
            var item = await _wishlistRepository.Get(wishlistId);
            if (item == null)
                throw new EntityNotFoundException("Wishlist item", wishlistId);

            var deleted = await _wishlistRepository.Delete(wishlistId);
            return deleted != null;
        }

        private static WishlistResponseDto MapToDto(Wishlist wishlist, Movie movie) => new()
        {
            Id = wishlist.Id,
            UserId = wishlist.UserId,
            MovieId = wishlist.MovieId,
            MovieTitle = movie.Title,
            RentalPrice = movie.RentalPrice,
            AddedDate = wishlist.AddedDate   // ← trainer uses AddedDate
        };
    }
}