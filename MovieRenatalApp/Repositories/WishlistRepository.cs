using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class WishlistRepository : Repository<int, Wishlist>
    {
        public WishlistRepository(MovieContext context) : base(context) { }

        protected override async Task<Wishlist?> GetByKey(int key)
        {
            return await _context.Wishlists
                .Include(w => w.Movie)
                .FirstOrDefaultAsync(w => w.Id == key);
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUser(int userId)
        {
            return await _context.Wishlists
                .Include(w => w.Movie)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int userId, int movieId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId
                            && w.MovieId == movieId);
        }
    }
}