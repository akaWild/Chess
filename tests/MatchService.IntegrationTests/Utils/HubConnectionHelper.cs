using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests.Utils
{
    public static class HubConnectionHelper
    {
        public static HubConnection GetHubConnection(HttpMessageHandler handler, string? matchId = null)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"http://localhost/{SharedData.HubEndpoint}" + (matchId == null ? "" : $"?matchId={matchId}"), o =>
                {
                    o.HttpMessageHandlerFactory = _ => handler;
                })
                .Build();

            return hubConnection;
        }
    }
}
