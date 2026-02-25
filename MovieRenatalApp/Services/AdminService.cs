using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;
using MovieRentalApp.Repositories;

namespace MovieRentalApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly MovieContext _context;
        private readonly UserRepository _userRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly RentalRepository _rentalRepository;
        private readonly AuditLogRepository _auditLogRepository;

        public AdminService(
            MovieContext context,
            UserRepository userRepository,
            PaymentRepository paymentRepository,
            RentalRepository rentalRepository,
            AuditLogRepository auditLogRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _rentalRepository = rentalRepository;
            _auditLogRepository = auditLogRepository;
        }

        // ── Dashboard Stats ───────────────────────────────────────
        public async Task<DashboardStatsDto> GetDashboardStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalMovies = await _context.Movies.CountAsync();
            var totalRentals = await _context.Rentals.CountAsync();
            var activeRentals = await _context.Rentals
                                    .CountAsync(r => r.Status == "Active");
            var totalRevenue = await _context.Payments
                                    .SumAsync(p => p.Amount);
            var totalPayments = await _context.Payments.CountAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalMovies = totalMovies,
                TotalRentals = totalRentals,
                ActiveRentals = activeRentals,
                TotalRevenue = totalRevenue,
                TotalPayments = totalPayments
            };
        }

        // ── All Users With Their Rentals ──────────────────────────
        public async Task<IEnumerable<UserRentalSummaryDto>> GetAllUsersWithRentals()
        {
            var users = await _context.Users
                // ❌ Remove .Include(u => u.Role) → Role is enum, not navigation
                .Include(u => u.Rentals)
                    .ThenInclude(r => r.Movie)
                .ToListAsync();

            return users.Select(u => new UserRentalSummaryDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.UserEmail,
                TotalRentals = u.Rentals.Count,
                Rentals = u.Rentals.Select(r => new RentalResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = u.UserName,
                    MovieId = r.MovieId,
                    MovieTitle = r.Movie?.Title ?? "",
                    RentalDate = r.RentalDate,
                    ExpiryDate = r.ExpiryDate,
                    Status = r.Status
                }).ToList()
            });
        }

        // ── All Payments ──────────────────────────────────────────
        public async Task<PaymentSummaryDto> GetAllPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return new PaymentSummaryDto
            {
                TotalPayments = payments.Count,
                TotalRevenue = payments.Sum(p => p.Amount),
                Payments = payments.Select(MapPaymentToDto).ToList()
            };
        }

        // ── Payments By User ──────────────────────────────────────
        public async Task<IEnumerable<PaymentDetailDto>> GetPaymentsByUser(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            var payments = await _context.Payments
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapPaymentToDto);
        }

        // ── Audit Logs ────────────────────────────────────────────
        public async Task<IEnumerable<AuditLogResponseDto>> GetAllLogs()
        {
            var logs = await _auditLogRepository.GetAllLogs();
            return logs.Select(MapLogToDto);
        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetLogsByUser(int userId)
        {
            var logs = await _auditLogRepository.GetLogsByUser(userId);
            return logs.Select(MapLogToDto);
        }

        public async Task<AuditLogResponseDto> CreateLog(
            int userId, string message, string errorNumber)
        {
            var user = await _userRepository.Get(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            var log = new AuditLog
            {
                UserId = userId,
                Message = message,
                ErrorNumber = errorNumber,
                Role = user.Role.ToString(),  // ← enum.ToString() not ?.Name
                UserName = user.UserName,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _auditLogRepository.Add(log);
            return MapLogToDto(created);
        }

        // ── Mappers ───────────────────────────────────────────────
        private static PaymentDetailDto MapPaymentToDto(Payment p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            UserName = p.User?.UserName ?? "",
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status,
            PaymentDate = p.PaymentDate
        };

        private static AuditLogResponseDto MapLogToDto(AuditLog log) => new()
        {
            LogId = log.LogId,
            Message = log.Message,
            ErrorNumber = log.ErrorNumber,
            Role = log.Role,
            UserName = log.UserName,
            UserId = log.UserId,
            CreatedAt = log.CreatedAt
        };
    }
}