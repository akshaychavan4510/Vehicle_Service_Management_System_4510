using Microsoft.AspNetCore.Identity;

namespace Vehicle_Service_Management_System.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}