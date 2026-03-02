using System.ComponentModel.DataAnnotations;

namespace MovieRentalApp.Models.DTOs
{
    public class UserUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        //[EmailAddress]
        //public string? Email { get; set; }
    }
}