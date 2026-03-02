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

        // ── Get User Notifications ────────────────────────────────
        public async Task<IEnumerable<NotificationResponseDto>>
            GetUserNotifications(int userId)
        {
            // Step 1 - Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Get notifications
            var notifications = await _notificationRepository
                .FindAsync(n => n.UserId == userId);

            // Step 3 - Check if empty
            if (!notifications.Any())
                throw new EntityNotFoundException(
                    $"No notifications found for user ID {userId}.");

            // Step 4 - Return ordered response
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(MapToDto);
        }

        // ── Mark Single As Read ───────────────────────────────────
        public async Task<NotificationResponseDto> MarkAsRead(
            int notificationId)
        {
            // Step 1 - Validate id
            if (notificationId <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid notification ID.");

            // Step 2 - Find notification
            var notification = await _notificationRepository
                .GetByIdAsync(notificationId);
            if (notification == null)
                throw new EntityNotFoundException(
                    "Notification", notificationId);

            // Step 3 - Check if already read
            if (notification.IsRead)
                throw new BusinessRuleViolationException(
                    "Notification is already marked as read.");

            // Step 4 - Mark as read
            notification.IsRead = true;
            var updated = await _notificationRepository
                .UpdateAsync(notificationId, notification);

            // Step 5 - Return response
            return MapToDto(updated!);
        }

        // ── Mark All As Read ──────────────────────────────────────
        public async Task<bool> MarkAllAsRead(int userId)
        {
            // Step 1 - Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Get unread notifications
            var unread = await _notificationRepository
                .FindAsync(n => n.UserId == userId && !n.IsRead);
            if (!unread.Any())
                throw new BusinessRuleViolationException(
                    "No unread notifications found.");

            // Step 3 - Mark all as read
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                await _notificationRepository
                    .UpdateAsync(notification.Id, notification);
            }

            // Step 4 - Return success
            return true;
        }

        // ── Send Rental Expiry Reminders ──────────────────────────
        public async Task SendRentalExpiryReminders()
        {
            // Step 1 - Get active rentals expiring in 2 days
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie, r => r.User);

            var expiringSoon = rentals.Where(r =>
                r.Status == "Active" &&
                r.ExpiryDate <= DateTime.UtcNow.AddDays(2) &&
                r.ExpiryDate > DateTime.UtcNow).ToList();

            // Step 2 - Send notification for each
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

        // ── Send New Release Notification ─────────────────────────
        public async Task SendNewReleaseNotification(int movieId)
        {
            // Step 1 - Find movie
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null)
                throw new EntityNotFoundException("Movie", movieId);

            // Step 2 - Get all active customers
            var users = await _userRepository
                .FindAsync(u => u.IsActive &&
                                u.Role == UserRole.Customer);

            // Step 3 - Send to all customers
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

        // ── Send Payment Success Notification ─────────────────────
        public async Task SendPaymentSuccessNotification(
            int userId, decimal amount)
        {
            // Step 1 - Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Send notification
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

        // ── Send Return Reminder ──────────────────────────────────
        public async Task SendReturnReminderNotification(int rentalId)
        {
            // Step 1 - Find rental
            var rentals = await _rentalRepository
                .GetAllWithIncludeAsync(r => r.Movie);
            var rental = rentals.FirstOrDefault(r => r.Id == rentalId);
            if (rental == null)
                throw new EntityNotFoundException("Rental", rentalId);

            // Step 2 - Send reminder
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

        // ── Mapper ────────────────────────────────────────────────
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