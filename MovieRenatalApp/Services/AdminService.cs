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

        
        public async Task<DashboardStatsDto> GetDashboardStats()
        {
            
            var users = await _userRepository.GetAllAsync();
            var movies = await _movieRepository.GetAllAsync();
            var rentals = await _rentalRepository.GetAllAsync();
            var payments = await _paymentRepository.GetAllAsync();

           
            var totalRevenue = payments.Any()
                ? payments.Sum(p => p.Amount) : 0;
            var activeRentals = rentals
                .Where(r => r.Status == "Active").Count();

            
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

        
        public async Task<IEnumerable<UserRentalSummaryDto>> GetAllUsersWithRentals()
        {
            
            var users = await _userRepository.GetAllAsync();
            if (!users.Any())
                throw new EntityNotFoundException("No users found.");

            
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie);

            
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

            
            return summary;
        }

       
        public async Task<PaymentSummaryDto> GetAllPayments()
        {
           
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            
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

        
        public async Task<IEnumerable<PaymentDetailDto>> GetPaymentsByUser(
            int userId)
        {
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            
            var userPayments = payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            
            if (!userPayments.Any())
                throw new EntityNotFoundException(
                    $"No payments found for user ID {userId}.");

            
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

       
        public async Task<IEnumerable<AuditLogResponseDto>> GetAllLogs()
        {
           
            var logs = await _auditLogRepository
                .GetAllWithIncludeAsync(a => a.User);
            if (!logs.Any())
                throw new EntityNotFoundException("No logs found.");

            
            return logs
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToLogDto);
        }

        
        public async Task<IEnumerable<AuditLogResponseDto>> GetLogsByUser(
            int userId)
        {
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            
            var logs = await _auditLogRepository
                .FindAsync(a => a.UserId == userId);
            if (!logs.Any())
                throw new EntityNotFoundException(
                    $"No logs found for user ID {userId}.");

            
            return logs
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToLogDto);
        }

        
        public async Task<AuditLogResponseDto> CreateLog(
            int userId, string message, string errorNumber)
        {
            
            if (userId <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid user ID.");
            if (string.IsNullOrWhiteSpace(message))
                throw new BusinessRuleViolationException(
                    "Message cannot be empty.");
            if (string.IsNullOrWhiteSpace(errorNumber))
                throw new BusinessRuleViolationException(
                    "Error number cannot be empty.");

            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            
            var log = new AuditLog
            {
                UserId = userId,
                UserName = user.UserName,
                Role = user.Role.ToString(),
                Message = message,
                ErrorNumber = errorNumber,
                CreatedAt = DateTime.UtcNow
            };

            
            var created = await _auditLogRepository.AddAsync(log);

            // Step 5 - Return
            return MapToLogDto(created);
        }

        
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