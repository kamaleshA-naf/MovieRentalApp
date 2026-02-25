using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class AuditLogRepository : Repository<int, AuditLog>
    {
        public AuditLogRepository(MovieContext context) : base(context) { }

        protected override async Task<AuditLog?> GetByKey(int key)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.LogId == key);
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogs()
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByUser(int userId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}