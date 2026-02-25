namespace MovieRentalApp.Models.DTOs
{
    public class MovieResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public int AvailableCopies { get; set; }
        public string? Director { get; set; }
        public int? ReleaseYear { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}