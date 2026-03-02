using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        // ── Public ────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllGenres()
        {
            try
            {
                var result = await _genreService.GetAllGenres();
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Admin + ContentManager ────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> AddGenre([FromBody] GenreCreateDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _genreService.AddGenre(dto);
                return CreatedAtAction(nameof(GetAllGenres), result);
            }
            catch (DuplicateEntityException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Admin Only ────────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid genre ID." });

            try
            {
                var result = await _genreService.DeleteGenre(id);
                return Ok(new { message = "Genre deleted.", data = result });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}