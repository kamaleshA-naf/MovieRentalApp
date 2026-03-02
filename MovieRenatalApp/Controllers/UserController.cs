using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ── Public ────────────────────────────────────────────────
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2 - Try to register
            try
            {
                var result = await _userService.Register(dto);
                return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
            }
            catch (DuplicateEntityException ex) { return Conflict(new { message = ex.Message }); }
            catch (UnableToCreateEntityException ex) { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2 - Try to login
            try
            {
                var result = await _userService.Login(dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Admin Only ────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _userService.GetAllUsers();

                // Step 1 - Check if empty
                if (result == null || !result.Any())
                    return NotFound(new { message = "No users found." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _userService.DeleteUser(id);
                return Ok(new { message = "User deleted successfully.", data = result });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Any Logged In User ────────────────────────────────────
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> GetUser(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _userService.GetUser(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _userService.UpdateUser(id, dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (DuplicateEntityException ex) { return Conflict(new { message = ex.Message }); }
            catch (UnableToCreateEntityException ex) { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}