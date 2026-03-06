using System.ComponentModel.DataAnnotations;

namespace MovieRentalApp.Models.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage =
            "Password must be at least 6 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}