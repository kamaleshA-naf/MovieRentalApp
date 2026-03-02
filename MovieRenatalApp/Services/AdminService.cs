using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<int, User> _userRepository;
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, Payment> _paymentRepository;
        private readonly IRepository<int, AuditLog> _auditLogRepository;

        public AdminService(
            IRepository<int, User> userRepository,
            IRepository<int, Movie> movieRepository,
            IRepository<int, Rental> rentalRepository,
            IRepository<int, Payment> paymentRepository,
            IRepository<int, AuditLog> auditLogRepository)
        {
            _userRepository = userRepository;
            _movieRepository = movieRepository;
            _rentalRepository = rentalRepository;
            _paymentRepository = paymentRepository;
            _auditLogRepository = auditLogRepository;
        }

        // ── Dashboard ─────────────────────────────────────────────
        public async Task<DashboardStatsDto> GetDashboardStats()
        {
            // Step 1 - Get all data
            var users = await _userRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();
            var rentals = await _rentalRepository.GetAllAsync();
            var payments = await _paymentRepository.GetAllAsync();

            // Step 2 - Calculate stats
            var totalRevenue = payments.Any()
                ? payments.Sum(p => p.Amount) : 0;
            var activeRentals = rentals
                .Where(r => r.Status == "Active").Count();

            // Step 3 - Return dashboard
            return new DashboardStatsDto
            {
                TotalUsers = users.Count(),
                TotalMovies = movies.Count(),
                TotalRentals = rentals.Count(),
                ActiveRentals = activeRentals,
                TotalRevenue = totalRevenue,
                TotalPayments = payments.Count()
            };
        }

        // ── Get All Users With Rentals ────────────────────────────
        public async Task<IEnumerable<UserRentalSummaryDto>> GetAllUsersWithRentals()
        {
            // Step 1 - Get all users
            var users = await _userRepository.GetAllAsync();
            if (!users.Any())
                throw new EntityNotFoundException("No users found.");

            // Step 2 - Get all rentals
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie);

            // Step 3 - Build summary
            var summary = users.Select(u => new UserRentalSummaryDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.UserEmail,
                Role = u.Role.ToString(),
                TotalRentals = rentals.Count(r => r.UserId == u.UserId),
                Rentals = rentals
                    .Where(r => r.UserId == u.UserId)
                    .Select(r => new RentalResponseDto
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
            }).ToList();

            // Step 4 - Return
            return summary;
        }

        // ── Get All Payments ──────────────────────────────────────
        public async Task<PaymentSummaryDto> GetAllPayments()
        {
            // Step 1 - Get all payments with user
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            // Step 2 - Build summary
            return new PaymentSummaryDto
            {
                TotalRevenue = payments.Any()
                    ? payments.Sum(p => p.Amount) : 0,
                TotalPayments = payments.Count(),
                Payments = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentDetailDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserName = p.User?.UserName ?? "",
                        Amount = p.Amount,
                        Method = p.Method,
                        Status = p.Status,
                        PaymentDate = p.PaymentDate
                    }).ToList()
            };
        }

        // ── Get Payments By User ──────────────────────────────────
        public async Task<IEnumerable<PaymentDetailDto>> GetPaymentsByUser(
            int userId)
        {
            // Step 1 - Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Get payments
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            // Step 3 - Filter by user
            var userPayments = payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            // Step 4 - Check empty
            if (!userPayments.Any())
                throw new EntityNotFoundException(
                    $"No payments found for user ID {userId}.");

            // Step 5 - Return
            return userPayments.Select(p => new PaymentDetailDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User?.UserName ?? "",
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            });
        }

        // ── Get All Logs ──────────────────────────────────────────
        public async Task<IEnumerable<AuditLogResponseDto>> GetAllLogs()
        {
            // Step 1 - Get all logs
            var logs = await _auditLogRepository
                .GetAllWithIncludeAsync(a => a.User);
            if (!logs.Any())
                throw new EntityNotFoundException("No logs found.");

            // Step 2 - Return ordered
            return logs
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToLogDto);
        }

        // ── Get Logs By User ──────────────────────────────────────
        public async Task<IEnumerable<AuditLogResponseDto>> GetLogsByUser(
            int userId)
        {
            // Step 1 - Validate user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Get logs
            var logs = await _auditLogRepository
                .FindAsync(a => a.UserId == userId);
            if (!logs.Any())
                throw new EntityNotFoundException(
                    $"No logs found for user ID {userId}.");

            // Step 3 - Return
            return logs
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToLogDto);
        }

        // ── Create Log ────────────────────────────────────────────
        public async Task<AuditLogResponseDto> CreateLog(
            int userId, string message, string errorNumber)
        {
            // Step 1 - Validate inputs
            if (userId <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid user ID.");
            if (string.IsNullOrWhiteSpace(message))
                throw new BusinessRuleViolationException(
                    "Message cannot be empty.");
            if (string.IsNullOrWhiteSpace(errorNumber))
                throw new BusinessRuleViolationException(
                    "Error number cannot be empty.");

            // Step 2 - Find user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 3 - Create log
            var log = new AuditLog
            {
                UserId = userId,
                UserName = user.UserName,
                Role = user.Role.ToString(),
                Message = message,
                ErrorNumber = errorNumber,
                CreatedAt = DateTime.UtcNow
            };

            // Step 4 - Save log
            var created = await _auditLogRepository.AddAsync(log);

            // Step 5 - Return
            return MapToLogDto(created);
        }

        // ── Mapper ────────────────────────────────────────────────
        private static AuditLogResponseDto MapToLogDto(AuditLog a) => new()
        {
            LogId = a.LogId,
            UserId = a.UserId,
            UserName = a.UserName,
            Role = a.Role,
            Message = a.Message,
            ErrorNumber = a.ErrorNumber,
            CreatedAt = a.CreatedAt
        };
    }
}