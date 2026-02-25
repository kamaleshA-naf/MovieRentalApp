using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class UserRepository : Repository<int, User>
    {
        public UserRepository(MovieContext context) : base(context) { }

        protected override async Task<User?> GetByKey(int key)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == key);  // ← UserId
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserEmail == email); // ← UserEmail
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.UserEmail == email); // ← UserEmail
        }
    }
}