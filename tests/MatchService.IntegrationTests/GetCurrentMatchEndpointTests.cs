using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class GetCurrentMatchEndpointTests : EndpointTestsBase
    {
        private MatchInfo _matchInfo;

        public GetCurrentMatchEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetCurrentMatch_WithoutMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.Null(ClientErrorMessage);
            Assert.Null(ServerErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithIncorrectMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: "12345-6789");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.NotNull(ClientErrorMessage);
            Assert.Null(ServerErrorMessage);
            Assert.Matches("incorrect format", ClientErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithNotExistentMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: "38B56259-CCCC-4821-AA4F-D83ED7B58FDF");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.NotNull(ClientErrorMessage);
            Assert.Null(ServerErrorMessage);
            Assert.Matches("wasn't found", ClientErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithExistentMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: "38B56259-55C0-4821-AA4F-D83ED7B58FDF");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.Null(ClientErrorMessage);
            Assert.Null(ServerErrorMessage);
            Assert.NotEqual(default, _matchInfo);
        }

        public override Task DisposeAsync()
        {
            _matchInfo = default;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<MatchInfo>("LoadMatchInfo", (mi) =>
            {
                _matchInfo = mi;

                ResponseReceived = true;
            });
        }
    }
}
