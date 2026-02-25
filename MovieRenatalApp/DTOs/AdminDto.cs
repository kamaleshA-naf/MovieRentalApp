namespace MovieRentalApp.Models.DTOs
{
    // ── User Rental Summary ───────────────────────────────────────
    public class UserRentalSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalRentals { get; set; }
        public List<RentalResponseDto> Rentals { get; set; } = new();
    }

    // ── Payment Summary ───────────────────────────────────────────
    public class PaymentSummaryDto
    {
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<PaymentDetailDto> Payments { get; set; } = new();
    }

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

    // ── Admin Dashboard Stats ─────────────────────────────────────
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int ActiveRentals { get; set; }
        public int TotalRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPayments { get; set; }
    }
}