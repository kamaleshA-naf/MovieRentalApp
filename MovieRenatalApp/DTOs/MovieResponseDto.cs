namespace MovieRentalApp.Models.DTOs
{
    public class MovieResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public string Director { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double Rating { get; set; }

        
        public string? VideoUrl { get; set; }

        public List<GenreResponseDto> Genres { get; set; }
            = new List<GenreResponseDto>();
    }
}