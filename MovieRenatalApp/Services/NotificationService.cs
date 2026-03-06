using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<int, Notification> _notificationRepository;
        private readonly IRepository<int, User> _userRepository;
        private readonly IRepository<int, Rental> _rentalRepository;
        private readonly IRepository<int, Movie> _movieRepository;

        public NotificationService(
            IRepository<int, Notification> notificationRepository,
            IRepository<int, User> userRepository,
            IRepository<int, Rental> rentalRepository,
            IRepository<int, Movie> movieRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _rentalRepository = rentalRepository;
            _movieRepository = movieRepository;
        }

        
        public async Task<IEnumerable<NotificationResponseDto>>
            GetUserNotifications(int userId)
        {
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            
            var notifications = await _notificationRepository
                .FindAsync(n => n.UserId == userId);

            
            if (!notifications.Any())
                throw new EntityNotFoundException(
                    $"No notifications found for user ID {userId}.");

            
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(MapToDto);
        }

        
        public async Task<NotificationResponseDto> MarkAsRead(
            int notificationId)
        {
            
            if (notificationId <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid notification ID.");

            
            var notification = await _notificationRepository
                .GetByIdAsync(notificationId);
            if (notification == null)
                throw new EntityNotFoundException(
                    "Notification", notificationId);

            
            if (notification.IsRead)
                throw new BusinessRuleViolationException(
                    "Notification is already marked as read.");

           
            notification.IsRead = true;
            var updated = await _notificationRepository
                .UpdateAsync(notificationId, notification);

            
            return MapToDto(updated!);
        }

        
        public async Task<bool> MarkAllAsRead(int userId)
        {
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            
            var unread = await _notificationRepository
                .FindAsync(n => n.UserId == userId && !n.IsRead);
            if (!unread.Any())
                throw new BusinessRuleViolationException(
                    "No unread notifications found.");

            
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                await _notificationRepository
                    .UpdateAsync(notification.Id, notification);
            }

           
            return true;
        }

        
        public async Task SendRentalExpiryReminders()
        {
           
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie, r => r.User);

            var expiringSoon = rentals.Where(r =>
                r.Status == "Active" &&
                r.ExpiryDate <= DateTime.UtcNow.AddDays(2) &&
                r.ExpiryDate > DateTime.UtcNow).ToList();

            
            foreach (var rental in expiringSoon)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = rental.UserId,
                    Title = "Rental Expiring Soon!",
                    Message = $"Your rental for '{rental.Movie?.Title}' " +
                                $"expires on {rental.ExpiryDate:dd MMM yyyy}. " +
                                $"Please return it on time.",
                    Type = "RentalExpiry",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

       
        public async Task SendNewReleaseNotification(int movieId)
        {
            
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", movieId);

          
            var users = await _userRepository
                .FindAsync(u => u.IsActive &&
                                u.Role == UserRole.Customer);

            
            foreach (var user in users)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = user.UserId,
                    Title = "New Movie Available!",
                    Message = $"'{movie.Title}' is now available to rent " +
                                $"for just ₹{movie.RentalPrice}/day. " +
                                $"Don't miss it!",
                    Type = "NewRelease",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        
        public async Task SendPaymentSuccessNotification(
            int userId, decimal amount)
        {
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

           
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = userId,
                Title = "Payment Successful!",
                Message = $"Your payment of ₹{amount} was successful. " +
                            $"Enjoy your movie!",
                Type = "PaymentSuccess",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        
        public async Task SendReturnReminderNotification(int rentalId)
        {
            
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie);
            var rental = rentals.FirstOrDefault(r => r.Id == rentalId);
            if (rental == null)
                throw new EntityNotFoundException("Rental", rentalId);

            
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = rental.UserId,
                Title = "Return Reminder",
                Message = $"Please return '{rental.Movie?.Title}' " +
                            $"by {rental.ExpiryDate:dd MMM yyyy} " +
                            $"to avoid late charges.",
                Type = "ReturnReminder",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

       
        private static NotificationResponseDto MapToDto(
            Notification n) => new()
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
    }
}