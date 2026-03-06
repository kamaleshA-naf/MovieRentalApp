using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> RentMovie([FromBody] RentalCreateDto dto)
        {
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           
            try
            {
                var result = await _rentalService.RentMovie(dto);
                return CreatedAtAction(nameof(GetRental), new { id = result.Id }, result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnableToCreateEntityException ex) { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}/return")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> ReturnMovie(int id)
        {
            
            if (id <= 0)
                return BadRequest(new { message = "Invalid rental ID." });

            try
            {
                var result = await _rentalService.ReturnMovie(id);
                return Ok(new { message = "Movie returned successfully.", data = result });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> GetRental(int id)
        {
            
            if (id <= 0)
                return BadRequest(new { message = "Invalid rental ID." });

            try
            {
                var result = await _rentalService.GetRental(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> GetRentalsByUser(int userId)
        {
            
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var result = await _rentalService.GetRentalsByUser(userId);

               
                if (result == null || !result.Any())
                    return NotFound(new { message = "No rentals found for this user." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveRentals()
        {
            try
            {
                var result = await _rentalService.GetActiveRentals();

                if (result == null || !result.Any())
                    return NotFound(new { message = "No active rentals found." });

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}