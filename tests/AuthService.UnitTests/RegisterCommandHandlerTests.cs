using AuthService.DTOs;
using AuthService.Features.Register;
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
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<TokenService> _tokenServiceMock;
        private readonly UserService _userService;

        public RegisterCommandHandlerTests()
        {
            _userManagerMock = UserManagerHelper.MockUserManager<AppUser>();
            _tokenServiceMock = new Mock<TokenService>(null!);
            _userService = new UserService();
        }

        [Fact]
        public async Task Handle_WithExistentUsername_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var registerDto = fixture.Build<RegisterDto>().With(p => p.Username, "bob").Create();
            var registerCommand = new RegisterCommand(registerDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.UserName, "bob").Create()
            };

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new RegisterCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(registerCommand, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithExistentEmail_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var registerDto = fixture.Build<RegisterDto>().With(p => p.Email, "bob@test.com").Create();
            var registerCommand = new RegisterCommand(registerDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.Email, "bob@test.com").Create()
            };

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new RegisterCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(registerCommand, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithCreateUserFailure_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var registerDto = fixture.Create<RegisterDto>();
            var registerCommand = new RegisterCommand(registerDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = fixture.CreateMany<AppUser>().ToArray();

            var result = IdentityResult.Failed(fixture.CreateMany<IdentityError>().ToArray());

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());
            _userManagerMock.Setup(config => config.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(result);

            var registerCommandHandler = new RegisterCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(registerCommand, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithCreateUserSuccess_ReturnValidUserDto()
        {
            var fixture = new Fixture();

            var registerDto = fixture.Create<RegisterDto>();
            var registerCommand = new RegisterCommand(registerDto);
            var cancellationToken = fixture.Create<CancellationToken>();

            var users = fixture.CreateMany<AppUser>().ToArray();

            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());
            _userManagerMock.Setup(config => config.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock.Setup(config => config.CreateToken(It.IsAny<AppUser>())).Returns("token");

            var registerCommandHandler = new RegisterCommandHandler(_userManagerMock.Object, _userService, _tokenServiceMock.Object);

            var userDto = await registerCommandHandler.Handle(registerCommand, cancellationToken);

            Assert.NotNull(userDto);
            Assert.NotEmpty(userDto.Username);
            Assert.NotEmpty(userDto.Token);
        }
    }
}
