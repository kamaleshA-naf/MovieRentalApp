using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Models;

namespace MovieRentalApp.Repositories
{
    public class MovieRepository : Repository<int, Movie>
    {
        public MovieRepository(MovieContext context) : base(context) { }

        protected override async Task<Movie?> GetByKey(int key)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == key);
        }

        public async Task<IEnumerable<Movie>> SearchMovies(string keyword)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Where(m => m.Title.Contains(keyword) ||
                            m.Description.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByGenre(int genreId)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
                .ToListAsync();
        }
    }
}