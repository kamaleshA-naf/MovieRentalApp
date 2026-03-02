using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // ── Admin + ContentManager ────────────────────────────────
        [HttpPost("AddMovie")]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> AddMovie([FromBody] MovieCreateDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2 - Try to add movie
            try
            {
                var result = await _movieService.AddMovie(dto);

                // Step 3 - Return success
                return CreatedAtAction(nameof(GetMovie), new { id = result.Id }, result);
            }
            catch (UnableToCreateEntityException ex) { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] MovieUpdateDto dto)
        {
            // Step 1 - Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Step 2 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid movie ID." });

            // Step 3 - Try to update
            try
            {
                var result = await _movieService.UpdateMovie(id, dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnableToCreateEntityException ex) { return StatusCode(500, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid movie ID." });

            // Step 2 - Try to delete
            try
            {
                var result = await _movieService.DeleteMovie(id);
                return Ok(new { message = "Movie deleted successfully.", data = result });
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Public ────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMovies()
        {
            try
            {
                var result = await _movieService.GetAllMovies();

                // Step 1 - Check if empty
                if (result == null || !result.Any())
                    return NotFound(new { message = "No movies found." });

                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMovie(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                return BadRequest(new { message = "Invalid movie ID." });

            try
            {
                var result = await _movieService.GetMovie(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchMovies([FromQuery] string keyword)
        {
            // Step 1 - Validate keyword
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Search keyword cannot be empty." });

            try
            {
                var result = await _movieService.SearchMovies(keyword);

                if (result == null || !result.Any())
                    return NotFound(new { message = $"No movies found for '{keyword}'." });

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("genre/{genreId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMoviesByGenre(int genreId)
        {
            // Step 1 - Validate genreId
            if (genreId <= 0)
                return BadRequest(new { message = "Invalid genre ID." });

            try
            {
                var result = await _movieService.GetMoviesByGenre(genreId);

                if (result == null || !result.Any())
                    return NotFound(new { message = "No movies found for this genre." });

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}