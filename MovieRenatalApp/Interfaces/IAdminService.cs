using MovieRentalApp.Models.DTOs;

public interface IAdminService
{
    // ── Dashboard ─────────────────────────────────────────────────
    Task<DashboardStatsDto> GetDashboardStats();

    // ── User Management ───────────────────────────────────────────
    Task<IEnumerable<UserRentalSummaryDto>> GetAllUsersWithRentals();

    // ── Payment Management ────────────────────────────────────────
    Task<PaymentSummaryDto> GetAllPayments();
    Task<IEnumerable<PaymentDetailDto>> GetPaymentsByUser(int userId);

    // ── Audit Logs ────────────────────────────────────────────────
    Task<IEnumerable<AuditLogResponseDto>> GetAllLogs();
    Task<IEnumerable<AuditLogResponseDto>> GetLogsByUser(int userId);
    Task<AuditLogResponseDto> CreateLog(int userId, string message, string errorNumber);
}