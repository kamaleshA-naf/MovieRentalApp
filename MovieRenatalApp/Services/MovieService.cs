using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<int, Movie> _movieRepository;
        private readonly IRepository<int, MovieGenre> _movieGenreRepository;
        private readonly IRepository<int, Genre> _genreRepository;

        public MovieService(
            IRepository<int, Movie> movieRepository,
            IRepository<int, MovieGenre> movieGenreRepository,
            IRepository<int, Genre> genreRepository)
        {
            _movieRepository = movieRepository;
            _movieGenreRepository = movieGenreRepository;
            _genreRepository = genreRepository;
        }

        public async Task<MovieResponseDto> AddMovie(MovieCreateDto dto)
        {
            // Step 1 - Create movie
            var movie = new Movie
            {
                Title = dto.Title,
                Description = dto.Description,
                RentalPrice = dto.RentalPrice,
                AvailableCopies = dto.AvailableCopies,
                Director = dto.Director,
                ReleaseYear = dto.ReleaseYear
            };

            // Step 2 - Save movie
            var created = await _movieRepository.AddAsync(movie);

            // Step 3 - Add genres
            if (dto.GenreIds != null && dto.GenreIds.Any())
            {
                foreach (var genreId in dto.GenreIds)
                {
                    await _movieGenreRepository.AddAsync(new MovieGenre
                    {
                        MovieId = created.Id,
                        GenreId = genreId
                    });
                }
            }

            return await BuildMovieDto(created.Id);
        }

        public async Task<MovieResponseDto> GetMovie(int id)
        {
            // Step 1 - Check exists
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            return await BuildMovieDto(id);
        }

        public async Task<IEnumerable<MovieResponseDto>> GetAllMovies()
        {
            // Step 1 - Get all with genres
            var movies = await _movieRepository.GetAllWithIncludeAsync(
                m => m.MovieGenres);

            if (!movies.Any())
                throw new EntityNotFoundException("No movies found.");

            return movies.Select(MapMovieToDto);
        }

        public async Task<MovieResponseDto> UpdateMovie(int id, MovieUpdateDto dto)
        {
            // Step 1 - Find movie
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            // Step 2 - Update fields
            if (dto.Title != null) movie.Title = dto.Title;
            if (dto.Description != null) movie.Description = dto.Description;
            if (dto.RentalPrice != null) movie.RentalPrice = dto.RentalPrice.Value;
            if (dto.AvailableCopies != null) movie.AvailableCopies = dto.AvailableCopies.Value;
            if (dto.Director != null) movie.Director = dto.Director;
            if (dto.ReleaseYear != null) movie.ReleaseYear = dto.ReleaseYear;

            // Step 3 - Save
            await _movieRepository.UpdateAsync(id, movie);
            return await BuildMovieDto(id);
        }

        public async Task<MovieResponseDto> DeleteMovie(int id)
        {
            // Step 1 - Find movie
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            var dto = MapMovieToDto(movie);

            // Step 2 - Delete
            await _movieRepository.DeleteAsync(id);
            return dto;
        }

        public async Task<IEnumerable<MovieResponseDto>> SearchMovies(string keyword)
        {
            // Step 1 - Search by title or description
            var movies = await _movieRepository
                .FindAsync(m => m.Title.Contains(keyword) ||
                                m.Description.Contains(keyword));
            return movies.Select(MapMovieToDto);
        }

        public async Task<IEnumerable<MovieResponseDto>> GetMoviesByGenre(int genreId)
        {
            // Step 1 - Find movie genres
            var movieGenres = await _movieGenreRepository
                .FindAsync(mg => mg.GenreId == genreId);

            var movieIds = movieGenres.Select(mg => mg.MovieId).ToList();

            // Step 2 - Get movies
            var movies = await _movieRepository
                .FindAsync(m => movieIds.Contains(m.Id));

            return movies.Select(MapMovieToDto);
        }

        // ── Helpers ───────────────────────────────────────────────
        private async Task<MovieResponseDto> BuildMovieDto(int movieId)
        {
            var movies = await _movieRepository
                .GetAllWithIncludeAsync(m => m.MovieGenres);
            var movie = movies.FirstOrDefault(m => m.Id == movieId);
            return MapMovieToDto(movie!);
        }

        private static MovieResponseDto MapMovieToDto(Movie movie) => new()
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
                                .ToList() ?? new List<string>()
        };
    }
}