//using System.ComponentModel.DataAnnotations;

//namespace MovieRentalApp.Models
//{
//    public class PasswordResetToken
//    {
//        public int Id { get; set; }

//        [Required]
//        public int UserId { get; set; }
//        public User User { get; set; } = null!;

//        [Required]
//        public string Token { get; set; } = string.Empty;

//        public DateTime ExpiryTime { get; set; }
//        public bool IsUsed { get; set; } = false;
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    }
//}