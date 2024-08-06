using AuthService.Models;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.UnitTests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly AppUser _appUser;

        public TokenServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _appUser = new AppUser
            {
                DisplayName = "Bobby",
                UserName = "bob",
                Email = "bob@test.com"
            };
        }

        [Fact]
        public void CreateToken_WithTokenKeyOfValidLength_ThrowsNoException()
        {
            var tokenKey = "MEgCQQCPbDhohZXk+x+qmz7M49VenP4YsAmNdkNeHlLaeKY3oXRZxmVHePw006+U54VubXfshn7izM4mujXE48x9py/zAgMBAAE=";

            _configMock.Setup(config => config["TokenKey"]).Returns(tokenKey);

            var tokenService = new TokenService(_configMock.Object);

            Assert.Null(Record.Exception(() => tokenService.CreateToken(_appUser)));
        }

        [Fact]
        public void CreateToken_WithTokenKeyOfInvalidLength_ThrowsArgumentException()
        {
            var tokenKey = "MEgCQQCPbDhohZXk+x+qmz7M49VenP4YsAmNdkNeHlLaeKY3oXRZxmVHePw006+";

            _configMock.Setup(config => config["TokenKey"]).Returns(tokenKey);

            var tokenService = new TokenService(_configMock.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => tokenService.CreateToken(_appUser));
        }
    }
}