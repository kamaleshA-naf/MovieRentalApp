using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]                               // ← All rental endpoints need login
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        public async Task<IActionResult> RentMovie([FromBody] RentalCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
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
        public async Task<IActionResult> ReturnMovie(int id)
        {
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
        public async Task<IActionResult> GetRental(int id)
        {
            try
            {
                var result = await _rentalService.GetRental(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRentalsByUser(int userId)
        {
            try
            {
                var result = await _rentalService.GetRentalsByUser(userId);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]          // ← Only Admin sees all active rentals
        public async Task<IActionResult> GetActiveRentals()
        {
            try
            {
                var result = await _rentalService.GetActiveRentals();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}