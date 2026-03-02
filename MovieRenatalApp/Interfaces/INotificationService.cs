using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetUserNotifications(
            int userId);
        Task<NotificationResponseDto> MarkAsRead(
            int notificationId);
        Task<bool> MarkAllAsRead(int userId);
        Task SendRentalExpiryReminders();
        Task SendNewReleaseNotification(
            int movieId);
        Task SendPaymentSuccessNotification(
            int userId, decimal amount);
        Task SendReturnReminderNotification(
            int rentalId);
    }
}