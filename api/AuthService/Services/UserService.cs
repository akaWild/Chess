using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public class UserService
    {
        public UserDto CreateUserDto(AppUser user, string token)
        {
            return new UserDto(user.DisplayName, user.UserName, user.Email, token);
        }
    }
}
