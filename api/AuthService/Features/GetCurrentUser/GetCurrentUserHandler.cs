using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLib.CQRS;
using SharedLib.Exceptions;
using System.Security.Claims;

namespace AuthService.Features.GetCurrentUser
{
    public record GetCurrentUserQuery : IQuery<UserDto>;

    public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _context;

        public GetCurrentUserQueryHandler(UserManager<AppUser> userManager, UserService userService, IHttpContextAccessor context)
        {
            _userManager = userManager;
            _userService = userService;
            _context = context;
        }

        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var context = _context.HttpContext;
            if (context == null)
                throw new CustomErrorResponseException("Unable to get current user info", 400);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == context.User.FindFirstValue(ClaimTypes.Name), cancellationToken: cancellationToken);

            if (user == null)
                throw new CustomErrorResponseException("Unable to get current user info", 400);

            var token = await context.GetTokenAsync("access_token");
            if (token == null)
                throw new CustomErrorResponseException("Unable to get current user info", 400);

            return _userService.CreateUserDto(user, token);
        }
    }
}
