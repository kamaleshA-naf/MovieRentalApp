using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;
using System.Security.Claims;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        
        private int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

       
        private string GetLoggedInRole()
        {
            var claim = User.FindFirst(ClaimTypes.Role);
            return claim?.Value ?? "";
        }

        
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] UserCreateDto dto)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.Register(dto);
                return CreatedAtAction(
                    nameof(GetUser),
                    new { id = result.Id },
                    result);
            }
            catch (DuplicateEntityException ex)
            { return Conflict(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.Login(dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex)
            { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _userService.GetAllUsers();
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(int id)
        {
          
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid user ID." });

            
            var loggedInUserId = GetLoggedInUserId();
            var loggedInRole = GetLoggedInRole();

            
            if (loggedInRole == "Customer" &&
                loggedInUserId != id)
                return StatusCode(403, new
                {
                    message = "Access denied. " +
                              "You can only view your own profile."
                });

            try
            {
                var result = await _userService.GetUser(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(
            int id, [FromBody] UserUpdateDto dto)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid user ID." });

            
            var loggedInUserId = GetLoggedInUserId();
            var loggedInRole = GetLoggedInRole();

           //customer they update owm profilee
            if (loggedInRole == "Customer" &&
                loggedInUserId != id)
                return StatusCode(403, new
                {
                    message = "Access denied. " +
                              "You can only update your own profile."
                });

           
            if (loggedInRole == "ContentManager")
                return StatusCode(403, new
                {
                    message = "Access denied. " +
                              "ContentManagers cannot update " +
                              "user profiles."
                });

            try
            {
                var result = await _userService.UpdateUser(id, dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (DuplicateEntityException ex)
            { return Conflict(new { message = ex.Message }); }
            catch (UnableToCreateEntityException ex)
            { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid user ID." });

            try
            {
                var result = await _userService.DeleteUser(id);
                return Ok(new
                {
                    message = "User deleted.",
                    data = result
                });
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

       
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            
            if (dto.UserId <= 0)
                return BadRequest(
                    new { message = "Invalid user ID." });

            
            var loggedInUserId = GetLoggedInUserId();
            var loggedInRole = GetLoggedInRole();

            
            if (loggedInRole == "Customer" &&
                loggedInUserId != dto.UserId)
                return StatusCode(403, new
                {
                    message = "Access denied. " +
                              "You can only change your own password."
                });

           
            try
            {
                var result = await _userService.ChangePassword(dto);
                return Ok(new { message = result });
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex)
            { return Unauthorized(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex)
            { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}