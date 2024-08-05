using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLib.CQRS;
using SharedLib.Exceptions;

namespace AuthService.Features.Login
{
    public record LoginCommand(LoginDto LoginDto)
        : ICommand<UserDto>;

    public class LoginCommandValidator
        : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.LoginDto).NotNull().WithMessage("Login data object can't be null");
            RuleFor(x => x.LoginDto.Username).NotEmpty().WithMessage("Username name is required");
            RuleFor(p => p.LoginDto.Password).NotEmpty().WithMessage("Your password cannot be empty");
        }
    }

    public class LoginCommandHandler : ICommandHandler<LoginCommand, UserDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public LoginCommandHandler(UserManager<AppUser> userManager, UserService userService, TokenService tokenService)
        {
            _userManager = userManager;
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<UserDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == request.LoginDto.Username, cancellationToken: cancellationToken);

            if (user == null)
                throw new CustomErrorResponseException($"User \"{request.LoginDto.Username}\" was not found", 401);

            var result = await _userManager.CheckPasswordAsync(user, request.LoginDto.Password);

            if (result)
                return _userService.CreateUserDto(user, _tokenService.CreateToken(user));

            throw new CustomErrorResponseException($"Unable to authorize user \"{request.LoginDto.Username}\"", 401);
        }
    }
}
