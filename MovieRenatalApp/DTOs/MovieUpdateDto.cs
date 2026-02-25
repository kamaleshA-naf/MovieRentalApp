using System.ComponentModel.DataAnnotations;

namespace MovieRentalApp.Models.DTOs
{
    public class MovieUpdateDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 10000)]
        public decimal? RentalPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int? AvailableCopies { get; set; }

        public string? Director { get; set; }
        public int? ReleaseYear { get; set; }
        public List<int>? GenreIds { get; set; }
    }
}