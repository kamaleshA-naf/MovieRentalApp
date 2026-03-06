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
            // Step 1 - Validate
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BusinessRuleViolationException(
                    "Genre name cannot be empty.");

            // Step 2 - Check duplicate
            var existing = await _genreRepository
                .FindAsync(g => g.Name.ToLower() == dto.Name.ToLower());
            if (existing.Any())
                throw new DuplicateEntityException(
                    $"Genre '{dto.Name}' already exists.");

            // Step 3 - Create
            var genre = new Genre { Name = dto.Name };
            var created = await _genreRepository.AddAsync(genre);

            // Step 4 - Return
            return new GenreResponseDto
            {
                Id = created.Id,
                Name = created.Name
            };
        }

        // ── Get All Genres ────────────────────────────────────────
        public async Task<IEnumerable<GenreResponseDto>> GetAllGenres()
        {
            var genres = await _genreRepository.GetAllAsync();
            if (!genres.Any())
                throw new EntityNotFoundException("No genres found.");

            return genres.Select(g => new GenreResponseDto
            {
                Id = g.Id,
                Name = g.Name
            });
        }

        // ── Delete Genre ──────────────────────────────────────────
        public async Task<GenreResponseDto> DeleteGenre(int id)
        {
            // Step 1 - Validate
            if (id <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid genre ID.");

            // Step 2 - Find
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null)
                throw new EntityNotFoundException("Genre", id);

            // Step 3 - Capture before delete
            var dto = new GenreResponseDto
            {
                Id = genre.Id,
                Name = genre.Name
            };

            // Step 4 - Delete
            await _genreRepository.DeleteAsync(id);
            return dto;
        }
    }
}