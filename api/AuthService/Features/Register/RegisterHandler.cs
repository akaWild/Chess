using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLib.CQRS;
using SharedLib.Exceptions;

namespace AuthService.Features.Register
{
    public record RegisterCommand(RegisterDto RegisterDto)
        : ICommand<UserDto>;

    public class RegisterCommandValidator
        : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.RegisterDto).NotNull().WithMessage("Register data object can't be null");
            RuleFor(x => x.RegisterDto.DisplayName).NotEmpty().WithMessage("Display name is required");
            RuleFor(x => x.RegisterDto.Username).NotEmpty().WithMessage("Username name is required");
            RuleFor(x => x.RegisterDto.Email).NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.RegisterDto.Email).EmailAddress().WithMessage("A valid email address is required");
            RuleFor(p => p.RegisterDto.Password).NotEmpty().WithMessage("Your password cannot be empty")
                .MinimumLength(8).WithMessage("Your password length must be at least 8 chars")
                .MaximumLength(16).WithMessage("Your password length must not exceed 16 chars")
                .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter")
                .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number");
        }
    }

    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, UserDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public RegisterCommandHandler(UserManager<AppUser> userManager, UserService userService, TokenService tokenService)
        {
            _userManager = userManager;
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == request.RegisterDto.Username, cancellationToken: cancellationToken))
                throw new CustomErrorResponseException("Username is taken. Try another one", 400);

            if (await _userManager.Users.AnyAsync(x => x.Email == request.RegisterDto.Email, cancellationToken: cancellationToken))
                throw new CustomErrorResponseException("Email is taken. Try another one", 400);

            var user = new AppUser
            {
                DisplayName = request.RegisterDto.DisplayName,
                Email = request.RegisterDto.Email,
                UserName = request.RegisterDto.Username
            };

            var result = await _userManager.CreateAsync(user, request.RegisterDto.Password);

            if (result.Succeeded)
                return _userService.CreateUserDto(user, _tokenService.CreateToken(user));

            string errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));

            throw new CustomErrorResponseException(errors, 400);
        }
    }
}
