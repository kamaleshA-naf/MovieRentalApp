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
        private readonly IWebHostEnvironment _env;

        public MovieController(
            IMovieService movieService,
            IWebHostEnvironment env)
        {
            _movieService = movieService;
            _env = env;
        }

        
        [HttpPost("AddMovie")]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> AddMovie(
            [FromBody] MovieCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _movieService.AddMovie(dto);
                return CreatedAtAction(
                    nameof(GetMovie),
                    new { id = result.Id }, result);
            }
            catch (BusinessRuleViolationException ex)
            { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── Upload Movie Video - Admin + ContentManager only ──────
        // Route  : POST /api/Movie/upload-video
        // Body   : multipart/form-data → File + MovieId
        // Returns: videoUrl saved to movie record
        [HttpPost("upload-video")]
        [Authorize(Roles = "Admin,ContentManager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMovieVideo(
            [FromForm] VideoUploadRequest request)
        {
            //  Validate file exists
            if (request.File == null || request.File.Length == 0)
                return BadRequest(
                    new { message = "No file uploaded." });

            //  Validate file type (video only)
            var allowedTypes = new[]
            {
                "video/mp4", "video/avi",
                "video/x-matroska", "video/webm"
            };
            if (!allowedTypes.Contains(
                    request.File.ContentType.ToLower()))
                return BadRequest(new
                {
                    message = "Invalid file type. " +
                              "Allowed: mp4, avi, mkv, webm."
                });

            //  Validate file size (max 500MB)
            const long maxSize = 500L * 1024 * 1024;
            if (request.File.Length > maxSize)
                return BadRequest(new
                {
                    message = "File too large. Maximum size is 500MB."
                });

            //  Validate movieId
            if (request.MovieId <= 0)
                return BadRequest(
                    new { message = "Invalid movie ID." });

            try
            {
                // Check movie exists first
                var movie = await _movieService
                    .GetMovie(request.MovieId);

                //  Create upload folder if not exists
                var uploadFolder = Path.Combine(
                    _env.WebRootPath ?? "wwwroot",
                    "uploads", "movies");
                Directory.CreateDirectory(uploadFolder);

                //  Generate unique filename
                var ext = Path.GetExtension(
                    request.File.FileName).ToLower();
                var fileName = $"movie_{request.MovieId}_" +
                               $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Save video file to disk
                using (var stream = new FileStream(
                    filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                //  Build public URL
                var videoUrl = $"/uploads/movies/{fileName}";

                //  Update movie with new VideoUrl
                var updateDto = new MovieUpdateDto
                {
                    Title = movie.Title,
                    Description = movie.Description,
                    RentalPrice = movie.RentalPrice,
                    Director = movie.Director,
                    ReleaseYear = movie.ReleaseYear,
                    Rating = movie.Rating,
                    VideoUrl = videoUrl
                };
                await _movieService.UpdateMovie(
                    request.MovieId, updateDto);

                //  Return success with URL
                return Ok(new
                {
                    message = "Video uploaded successfully.",
                    movieId = request.MovieId,
                    videoUrl = videoUrl
                });
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMovie(int id)
        {
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid movie ID." });

            try
            {
                var result = await _movieService.GetMovie(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMovies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagination = new PaginationDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result = await _movieService
                    .GetAllMovies(pagination);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchMovies(
            [FromQuery] string? keyword,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(
                    new { message = "Keyword cannot be empty." });

            var pagination = new PaginationDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result = await _movieService
                    .SearchMovies(keyword, pagination);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex)
            { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpGet("genre/{genreId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMoviesByGenre(
            int genreId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (genreId <= 0)
                return BadRequest(
                    new { message = "Invalid genre ID." });

            var pagination = new PaginationDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result = await _movieService
                    .GetMoviesByGenre(genreId, pagination);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex)
            { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ContentManager")]
        public async Task<IActionResult> UpdateMovie(
            int id, [FromBody] MovieUpdateDto dto)
        {
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid movie ID." });

            try
            {
                var result = await _movieService
                    .UpdateMovie(id, dto);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (BusinessRuleViolationException ex)
            { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            if (id <= 0)
                return BadRequest(
                    new { message = "Invalid movie ID." });

            try
            {
                var result = await _movieService.DeleteMovie(id);
                return Ok(new
                {
                    message = "Movie deleted.",
                    data = result
                });
            }
            catch (EntityNotFoundException ex)
            { return NotFound(new { message = ex.Message }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }
    }

    
    public class VideoUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public int MovieId { get; set; }
    }
}