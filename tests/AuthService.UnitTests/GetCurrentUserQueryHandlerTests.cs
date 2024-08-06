using AuthService.Features.GetCurrentUser;
using AuthService.Models;
using AuthService.Services;
using AuthService.UnitTests.Utils;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using SharedLib.Exceptions;

namespace AuthService.UnitTests
{
    public class GetCurrentUserQueryHandlerTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IHttpContextAccessor> _contextMock;
        private readonly UserService _userService;

        public GetCurrentUserQueryHandlerTests()
        {
            _userManagerMock = UserManagerHelper.MockUserManager<AppUser>();
            _contextMock = new Mock<IHttpContextAccessor>();
            _userService = new UserService();
        }

        [Fact]
        public async Task Handle_WithNullContext_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var userQuery = new GetCurrentUserQuery();
            var cancellationToken = fixture.Create<CancellationToken>();

            _contextMock.Setup(config => config.HttpContext).Returns((HttpContext?)null);

            var registerCommandHandler = new GetCurrentUserQueryHandler(_userManagerMock.Object, _userService, _contextMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(userQuery, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithNonExistentUsername_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var userQuery = new GetCurrentUserQuery();
            var cancellationToken = fixture.Create<CancellationToken>();

            var httpContext = new DefaultHttpContext()
            {
                User = IdentityHelper.GetClaimsPrincipal()
            };

            var users = fixture.CreateMany<AppUser>().ToArray();

            _contextMock.Setup(config => config.HttpContext).Returns(httpContext);
            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new GetCurrentUserQueryHandler(_userManagerMock.Object, _userService, _contextMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(userQuery, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithNoAccessToken_ThrowsCustomErrorResponseException()
        {
            var fixture = new Fixture();

            var userQuery = new GetCurrentUserQuery();
            var cancellationToken = fixture.Create<CancellationToken>();

            var httpContext = new DefaultHttpContext()
            {
                User = IdentityHelper.GetClaimsPrincipal()
            };

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.UserName, "test").Create()
            };

            _contextMock.Setup(config => config.HttpContext).Returns(httpContext);
            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new GetCurrentUserQueryHandler(_userManagerMock.Object, _userService, _contextMock.Object);

            await Assert.ThrowsAsync<CustomErrorResponseException>(async () => await registerCommandHandler.Handle(userQuery, cancellationToken));
        }

        [Fact]
        public async Task Handle_WithAccessToken_ReturnValidUserDto()
        {
            var fixture = new Fixture();

            var userQuery = new GetCurrentUserQuery();
            var cancellationToken = fixture.Create<CancellationToken>();

            var httpContext = new DefaultHttpContext()
            {
                User = IdentityHelper.GetClaimsPrincipal(),
                Request = { Headers = { Authorization = "test_token" } }
            };

            var users = new[]
            {
                fixture.Create<AppUser>(),
                fixture.Create<AppUser>(),
                fixture.Build<AppUser>().With(p => p.UserName, "test").Create()
            };

            _contextMock.Setup(config => config.HttpContext).Returns(httpContext);
            _userManagerMock.Setup(config => config.Users).Returns(users.AsQueryable().BuildMock());

            var registerCommandHandler = new GetCurrentUserQueryHandler(_userManagerMock.Object, _userService, _contextMock.Object);

            var userDto = await registerCommandHandler.Handle(userQuery, cancellationToken);

            Assert.NotNull(userDto);
            Assert.NotEmpty(userDto.Username);
            Assert.NotEmpty(userDto.Token);
        }
    }
}
