using AuthService.Models;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthService.UnitTests
{
    public class TokenServiceFixture
    {
        public readonly Mock<IConfiguration> ConfigMock;
        public readonly AppUser AppUser;

        public TokenServiceFixture()
        {
            ConfigMock = new Mock<IConfiguration>();
            AppUser = new AppUser
            {
                DisplayName = "Bobby",
                UserName = "bob",
                Email = "bob@test.com"
            };
        }
    }

    public class TokenServiceTests : IClassFixture<TokenServiceFixture>
    {
        private readonly TokenServiceFixture _fixture;

        public TokenServiceTests(TokenServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateToken_WithTokenKeyOfValidLength_ThrowsNoException()
        {
            var tokenKey = "MEgCQQCPbDhohZXk+x+qmz7M49VenP4YsAmNdkNeHlLaeKY3oXRZxmVHePw006+U54VubXfshn7izM4mujXE48x9py/zAgMBAAE=";

            _fixture.ConfigMock.Setup(config => config["TokenKey"]).Returns(tokenKey);

            var tokenService = new TokenService(_fixture.ConfigMock.Object);

            Assert.Null(Record.Exception(() => tokenService.CreateToken(_fixture.AppUser)));
        }

        [Fact]
        public void CreateToken_WithTokenKeyOfInvalidLength_ThrowsArgumentException()
        {
            var tokenKey = "MEgCQQCPbDhohZXk+x+qmz7M49VenP4YsAmNdkNeHlLaeKY3oXRZxmVHePw006+";

            _fixture.ConfigMock.Setup(config => config["TokenKey"]).Returns(tokenKey);

            var tokenService = new TokenService(_fixture.ConfigMock.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => tokenService.CreateToken(_fixture.AppUser));
        }
    }
}