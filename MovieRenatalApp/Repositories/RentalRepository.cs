using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class RentalRepository : Repository<int, Rental>
    {
        public RentalRepository(MovieContext context) : base(context) { }

        protected override async Task<Rental?> GetByKey(int key)
        {
            return await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.Id == key);
        }

        public async Task<IEnumerable<Rental>> GetRentalsByUser(int userId)
        {
            return await _context.Rentals
                .Include(r => r.Movie)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RentalDate) // ← RentalDate
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetActiveRentals()
        {
            return await _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Movie)
                .Where(r => r.Status == "Active")   // ← Status field
                .ToListAsync();
        }

        public async Task<bool> IsMovieAlreadyRentedByUser(int userId, int movieId)
        {
            return await _context.Rentals
                .AnyAsync(r => r.UserId == userId
                            && r.MovieId == movieId
                            && r.Status == "Active"); // ← Status field
        }
    }
}