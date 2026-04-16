using Microsoft.AspNetCore.Identity;

namespace CarPoolingApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int DriverNumber { get; set; }

        public string? FullName { get; set; }

        public bool ShowName { get; set; } = false;
    }
}