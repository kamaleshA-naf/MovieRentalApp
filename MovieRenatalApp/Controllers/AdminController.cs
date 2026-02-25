using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]        // ← Only Admin can access
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ── Dashboard ─────────────────────────────────────────────
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

        // ── Users + Their Rentals ─────────────────────────────────
        [HttpGet("users/rentals")]
        public async Task<IActionResult> GetAllUsersWithRentals()
        {
            try
            {
                var result = await _adminService.GetAllUsersWithRentals();
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── All Payments ──────────────────────────────────────────
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var result = await _adminService.GetAllPayments();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Payments By User ──────────────────────────────────────
        [HttpGet("payments/user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUser(int userId)
        {
            try
            {
                var result = await _adminService.GetPaymentsByUser(userId);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── All Audit Logs ────────────────────────────────────────
        [HttpGet("logs")]
        public async Task<IActionResult> GetAllLogs()
        {
            try
            {
                var result = await _adminService.GetAllLogs();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Logs By User ──────────────────────────────────────────
        [HttpGet("logs/user/{userId}")]
        public async Task<IActionResult> GetLogsByUser(int userId)
        {
            try
            {
                var result = await _adminService.GetLogsByUser(userId);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Create Audit Log ──────────────────────────────────────
        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(
            [FromQuery] int userId,
            [FromQuery] string message,
            [FromQuery] string errorNumber)
        {
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