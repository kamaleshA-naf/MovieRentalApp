using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<int, Payment> _paymentRepository;
        private readonly IRepository<int, User> _userRepository;

        public PaymentService(
            IRepository<int, Payment> paymentRepository,
            IRepository<int, User> userRepository)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
        }

        // ── Get Payment ───────────────────────────────────────────
        public async Task<PaymentResponseDto> GetPayment(int id)
        {
            // Step 1 - Validate id
            if (id <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid payment ID.");

            // Step 2 - Find payment with user
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);
            var payment = payments.FirstOrDefault(p => p.Id == id);

            // Step 3 - Check if found
            if (payment == null)
                throw new EntityNotFoundException("Payment", id);

            // Step 4 - Return response
            return MapToDto(payment);
        }

        // ── Get Payments By User ──────────────────────────────────
        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByUser(
            int userId)
        {
            // Step 1 - Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            // Step 2 - Get payments with user
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            // Step 3 - Filter by user
            var userPayments = payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            // Step 4 - Check if empty
            if (!userPayments.Any())
                throw new EntityNotFoundException(
                    $"No payments found for user ID {userId}.");

            // Step 5 - Return response
            return userPayments.Select(MapToDto);
        }

        // ── Get All Payments ──────────────────────────────────────
        public async Task<IEnumerable<PaymentResponseDto>> GetAllPayments()
        {
            // Step 1 - Get all with user
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);

            // Step 2 - Check if empty
            if (!payments.Any())
                throw new EntityNotFoundException("No payments found.");

            // Step 3 - Return ordered
            return payments
                .OrderByDescending(p => p.PaymentDate)
                .Select(MapToDto);
        }

        // ── Update Payment Status ─────────────────────────────────
        public async Task<PaymentResponseDto> UpdatePaymentStatus(
            int id, string status)
        {
            // Step 1 - Validate id
            if (id <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid payment ID.");

            // Step 2 - Validate status
            var validStatuses = new[]
            {
                "Completed", "Failed", "Refunded", "Pending"
            };
            if (!validStatuses.Contains(status))
                throw new BusinessRuleViolationException(
                    $"Invalid status. Valid: " +
                    $"{string.Join(", ", validStatuses)}");

            // Step 3 - Find payment
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                throw new EntityNotFoundException("Payment", id);

            // Step 4 - Check if already refunded
            if (payment.Status == "Refunded")
                throw new BusinessRuleViolationException(
                    "Cannot update a refunded payment.");

            // Step 5 - Update status
            payment.Status = status;
            await _paymentRepository.UpdateAsync(id, payment);

            // Step 6 - Return updated with user
            var payments = await _paymentRepository
                .GetAllWithIncludeAsync(p => p.User);
            var updated = payments.FirstOrDefault(p => p.Id == id);
            return MapToDto(updated!);
        }

        // ── Mapper ────────────────────────────────────────────────
        private static PaymentResponseDto MapToDto(Payment p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            UserName = p.User?.UserName ?? "",
            Amount = p.Amount,
            Method = p.Method,
            Status = p.Status,
            PaymentDate = p.PaymentDate
        };
    }
}