namespace MovieRentalApp.Models.DTOs
{
    public class PaymentDetailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }
}