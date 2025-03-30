using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
    }
}
