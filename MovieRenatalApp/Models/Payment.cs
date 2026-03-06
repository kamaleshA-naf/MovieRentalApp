namespace MovieRentalApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // ✅ Links payment to specific rental
        public int RentalId { get; set; }

        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public DateTime PaymentDate { get; set; }
    }
}