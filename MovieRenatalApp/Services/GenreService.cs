using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<int, Genre> _genreRepository;

        public GenreService(IRepository<int, Genre> genreRepository)
        {
            _genreRepository = genreRepository;
        }

        // ── Add Genre ─────────────────────────────────────────────
        public async Task<GenreResponseDto> AddGenre(GenreCreateDto dto)
        {
            // Step 1 - Check duplicate
            var existing = await _genreRepository
                .FindAsync(g => g.Name.ToLower() == dto.Name.ToLower());
            if (existing.Any())
                throw new DuplicateEntityException(
                    $"Genre '{dto.Name}' already exists.");

            // Step 2 - Create genre
            var genre = new Genre
            {
                Name = dto.Name
            };

            // Step 3 - Save
            var created = await _genreRepository.AddAsync(genre);

            // Step 4 - Return response
            return MapToDto(created);
        }

        // ── Get All Genres ────────────────────────────────────────
        public async Task<IEnumerable<GenreResponseDto>> GetAllGenres()
        {
            // Step 1 - Get all genres
            var genres = await _genreRepository.GetAllAsync();

            // Step 2 - Check if empty
            if (!genres.Any())
                throw new EntityNotFoundException("No genres found.");

            // Step 3 - Return ordered
            return genres
                .OrderBy(g => g.Name)
                .Select(MapToDto);
        }

        // ── Delete Genre ──────────────────────────────────────────
        public async Task<GenreResponseDto> DeleteGenre(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid genre ID.");

            // Step 2 - Find genre
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null)
                throw new EntityNotFoundException("Genre", id);

            // Step 3 - Save dto before delete
            var dto = MapToDto(genre);

            // Step 4 - Delete
            await _genreRepository.DeleteAsync(id);

            // Step 5 - Return deleted info
            return dto;
        }

        // ── Mapper ────────────────────────────────────────────────
        private static GenreResponseDto MapToDto(Genre g) => new()
        {
            Id = g.Id,
            Name = g.Name
        };
    }
}