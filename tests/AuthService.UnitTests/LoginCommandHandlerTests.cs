using AuthService.DTOs;
using AuthService.Features.Login;
using AuthService.Models;
using AuthService.Services;
using AuthService.UnitTests.Utils;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using SharedLib.Exceptions;

namespace AuthService.UnitTests
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<TokenService> _tokenServiceMock;
        private readonly UserService _userService;

        public LoginCommandHandlerTests()
        {
            _userManagerMock = UserManagerHelper.MockUserManager<AppUser>();
            _tokenServiceMock = new Mock<TokenService>(null!);
            _userService = new UserService();
        }

        [Fact]
        public async Task Handle_WithNonExistentUsername_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var loginDto = fixture.Create<LoginDto>();
            var loginCommand = new LoginCommand(loginDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = fixture.CreateMany<AppUser>().ToArray();

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new LoginCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(loginCommand, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithCreatePasswordFailure_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var loginDto = fixture.Build<LoginDto>().With(p => p.Username, "tom").Create();
            var loginCommand = new LoginCommand(loginDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.UserName, "tom").Create()
            };

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());
            _userManagerMock.Setup(config => config.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(false);

            var registerCommandHandler = new LoginCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(loginCommand, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithCreatePasswordSuccess_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var loginDto = fixture.Build<LoginDto>().With(p => p.Username, "tom").Create();
            var loginCommand = new LoginCommand(loginDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.UserName, "tom").Create()
            };

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());
            _userManagerMock.Setup(config => config.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);

            _tokenServiceMock.Setup(config => config.CreateToken(It.IsAny<AppUser>())).Returns("token");

            var registerCommandHandler = new LoginCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            var userDto = await registerCommandHandler.Handle(loginCommand, cancellationToken);

            Assert.NotNull(userDto);
            Assert.NotEmpty(userDto.Username);
            Assert.NotEmpty(userDto.Token);
        }
    }
}
