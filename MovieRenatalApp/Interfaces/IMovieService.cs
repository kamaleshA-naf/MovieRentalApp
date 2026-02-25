using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Interfaces
{
    public interface IMovieService
    {
        Task<MovieResponseDto> AddMovie(MovieCreateDto dto);
        Task<MovieResponseDto> GetMovie(int id);
        Task<IEnumerable<MovieResponseDto>> GetAllMovies();
        Task<MovieResponseDto> UpdateMovie(int id, MovieUpdateDto dto);
        Task<MovieResponseDto> DeleteMovie(int id);
        Task<IEnumerable<MovieResponseDto>> SearchMovies(string keyword);
        Task<IEnumerable<MovieResponseDto>> GetMoviesByGenre(int genreId);
    }
}