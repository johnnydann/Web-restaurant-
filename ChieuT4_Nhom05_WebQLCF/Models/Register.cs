using System.ComponentModel.DataAnnotations;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class Register
    {
        [Required(ErrorMessage = "User name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string PhoneNumber { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? ConfirmPassword { get; set; }
    }
}
