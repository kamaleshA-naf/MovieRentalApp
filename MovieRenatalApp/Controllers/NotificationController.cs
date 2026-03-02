using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ── Customer + Admin ──────────────────────────────────────
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            // Step 1 - Validate userId
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _notificationService
                    .GetUserNotifications(userId);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}/read")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid notification ID." });

            try
            {
                var result = await _notificationService.MarkAsRead(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("user/{userId}/read-all")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            // Step 1 - Validate userId
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _notificationService.MarkAllAsRead(userId);
                return Ok(new { message = "All notifications marked as read.", success = result });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Admin Only ────────────────────────────────────────────
        [HttpPost("send-expiry-reminders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendExpiryReminders()
        {
            try
            {
                await _notificationService.SendRentalExpiryReminders();
                return Ok(new { message = "Expiry reminders sent successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("new-release/{movieId}")]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> SendNewReleaseNotification(int movieId)
        {
            // Step 1 - Validate movieId
            if (movieId <= 0)
                return BadRequest(new { message = "Invalid movie ID." });

            try
            {
                await _notificationService
                    .SendNewReleaseNotification(movieId);
                return Ok(new { message = "New release notification sent to all customers." });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("return-reminder/{rentalId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendReturnReminder(int rentalId)
        {
            // Step 1 - Validate rentalId
            if (rentalId <= 0)
                return BadRequest(new { message = "Invalid rental ID." });

            try
            {
                await _notificationService
                    .SendReturnReminderNotification(rentalId);
                return Ok(new { message = "Return reminder sent successfully." });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}