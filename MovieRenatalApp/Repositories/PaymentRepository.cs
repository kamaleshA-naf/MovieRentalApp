using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class PaymentRepository : Repository<int, Payment>
    {
        public PaymentRepository(MovieContext context) : base(context) { }

        protected override async Task<Payment?> GetByKey(int key)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == key);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUser(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}