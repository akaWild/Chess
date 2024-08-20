using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests.Utils
{
    public static class HubConnectionHelper
    {
        public static HubConnection GetHubConnection(HttpMessageHandler handler, string? token = null, string? matchId = null)
        {
            var url = $"http://localhost/{SharedData.HubEndpoint}";

            if (token != null)
                url += $"?access_token={token}";

            if (matchId != null)
                url += (token == null ? "?" : "&") + $"matchId={matchId}";

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(url, o =>
                {
                    o.HttpMessageHandlerFactory = _ => handler;
                })
                .Build();

            return hubConnection;
        }
    }
}
