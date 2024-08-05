using Microsoft.AspNetCore.Identity;

namespace AuthService.Models
{
    public class AppUser : IdentityUser
    {
        public required string DisplayName { get; set; }
        public new required string UserName { get; set; }
        public new required string Email { get; set; }
    }
}
