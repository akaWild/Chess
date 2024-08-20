using MatchService.IntegrationTests.Fixtures;
using System.Net;
using System.Security.Claims;

namespace MatchService.IntegrationTests.Utils
{
    public static class TokenHelper
    {
        public static string? GetAccessToken(CustomWebAppFactory factory, string? username = null)
        {
            var client = factory.CreateClient();
            var claims = new Dictionary<string, object>();

            if (username != null)
                claims.Add(ClaimTypes.Name, username);

            client.SetFakeJwtBearerToken(claims);

            return client.DefaultRequestHeaders.Authorization?.Parameter;
        }
    }
}
