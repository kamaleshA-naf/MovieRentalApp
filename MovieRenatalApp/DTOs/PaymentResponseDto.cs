namespace MovieRentalApp.Models.DTOs
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RentalId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidOn { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}