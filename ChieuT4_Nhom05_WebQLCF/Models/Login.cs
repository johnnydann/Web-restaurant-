using System.ComponentModel.DataAnnotations;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class Login
    {
        [Required(ErrorMessage = "User name is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        private string token;
    }
}
