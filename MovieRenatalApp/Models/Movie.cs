namespace MovieRentalApp.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public string Director { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public double Rating { get; set; }

        // ✅ Video file path - e.g. "/uploads/movies/avengers.mp4"
        public string? VideoUrl { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; }
            = new List<MovieGenre>();
        public ICollection<Rental> Rentals { get; set; }
            = new List<Rental>();
        public ICollection<Wishlist> Wishlists { get; set; }
            = new List<Wishlist>();
    }
}