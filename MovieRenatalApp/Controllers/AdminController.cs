using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var result = await _adminService.GetDashboardStats();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("users/rentals")]
        public async Task<IActionResult> GetAllUsersWithRentals()
        {
            try
            {
                var result = await _adminService.GetAllUsersWithRentals();

                if (result == null || !result.Any())
                    return NotFound(new { message = "No users found." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var result = await _adminService.GetAllPayments();

                if (result.TotalPayments == 0)
                    return NotFound(new { message = "No payments found." });

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("payments/user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUser(int userId)
        {
            // Step 1 - Validate userId
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _adminService.GetPaymentsByUser(userId);

                if (result == null || !result.Any())
                    return NotFound(new { message = "No payments found for this user." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetAllLogs()
        {
            try
            {
                var result = await _adminService.GetAllLogs();

                if (result == null || !result.Any())
                    return NotFound(new { message = "No logs found." });

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("logs/user/{userId}")]
        public async Task<IActionResult> GetLogsByUser(int userId)
        {
            // Step 1 - Validate userId
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _adminService.GetLogsByUser(userId);

                if (result == null || !result.Any())
                    return NotFound(new { message = "No logs found for this user." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(
            [FromQuery] int userId,
            [FromQuery] string message,
            [FromQuery] string errorNumber)
        {
            // Step 1 - Validate inputs
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { message = "Log message cannot be empty." });

            if (string.IsNullOrWhiteSpace(errorNumber))
                return BadRequest(new { message = "Error number cannot be empty." });

            // Step 2 - Create log
            try
            {
                var result = await _adminService.CreateLog(userId, message, errorNumber);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}