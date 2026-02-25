using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;
using MovieRentalApp.Repositories;

namespace MovieRentalApp.Services
{
    public class MovieService : IMovieService
    {
        private readonly MovieRepository _movieRepository;

        public MovieService(MovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<MovieResponseDto> AddMovie(MovieCreateDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                Description = dto.Description,
                RentalPrice = dto.RentalPrice,
                AvailableCopies = dto.AvailableCopies,
                Director = dto.Director,
                ReleaseYear = dto.ReleaseYear
            };

            var created = await _movieRepository.Add(movie);
            if (created == null)
                throw new UnableToCreateEntityException("Movie");

            return MapToDto(created);
        }

        public async Task<MovieResponseDto> GetMovie(int id)
        {
            var movie = await _movieRepository.Get(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            return MapToDto(movie);
        }

        public async Task<IEnumerable<MovieResponseDto>> GetAllMovies()
        {
            var movies = await _movieRepository.GetAll();
            if (movies == null || !movies.Any())
                throw new EntityNotFoundException("No movies found in the system.");

            return movies.Select(MapToDto);
        }

        public async Task<MovieResponseDto> UpdateMovie(int id, MovieUpdateDto dto)
        {
            var existing = await _movieRepository.Get(id);
            if (existing == null)
                throw new EntityNotFoundException("Movie", id);

            if (dto.Title != null) existing.Title = dto.Title;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.RentalPrice.HasValue) existing.RentalPrice = dto.RentalPrice.Value;
            if (dto.AvailableCopies.HasValue) existing.AvailableCopies = dto.AvailableCopies.Value;
            if (dto.Director != null) existing.Director = dto.Director;
            if (dto.ReleaseYear.HasValue) existing.ReleaseYear = dto.ReleaseYear;

            var updated = await _movieRepository.Update(id, existing);
            if (updated == null)
                throw new UnableToCreateEntityException("Movie", "Update failed.");

            return MapToDto(updated);
        }

        public async Task<MovieResponseDto> DeleteMovie(int id)
        {
            var movie = await _movieRepository.Get(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            var deleted = await _movieRepository.Delete(id);
            return MapToDto(deleted!);
        }

        public async Task<IEnumerable<MovieResponseDto>> SearchMovies(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new BusinessRuleViolationException("Search keyword cannot be empty.");

            var movies = await _movieRepository.SearchMovies(keyword);
            return movies.Select(MapToDto);
        }

        public async Task<IEnumerable<MovieResponseDto>> GetMoviesByGenre(int genreId)
        {
            var movies = await _movieRepository.GetMoviesByGenre(genreId);
            if (!movies.Any())
                throw new EntityNotFoundException($"No movies found for genre ID {genreId}.");

            return movies.Select(MapToDto);
        }

        private static MovieResponseDto MapToDto(Movie movie) => new()
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            RentalPrice = movie.RentalPrice,
            AvailableCopies = movie.AvailableCopies,
            Director = movie.Director,
            ReleaseYear = movie.ReleaseYear,
            Genres = movie.MovieGenres?
                                .Select(mg => mg.Genre?.Name ?? "")
                                .Where(n => n != "")
                                .ToList() ?? new()
        };
    }
}