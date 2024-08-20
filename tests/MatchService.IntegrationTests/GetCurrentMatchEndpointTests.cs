using MatchService.Data;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class GetCurrentMatchEndpointTests : IAsyncLifetime
    {
        private readonly CustomWebAppFactory _factory;
        private readonly HttpMessageHandler _httpMessageHandler;

        private bool _responseReceived = false;
        private MatchInfo _matchInfo;
        private string? _clientErrorMessage;
        private string? _serverErrorMessage;

        public GetCurrentMatchEndpointTests(CustomWebAppFactory factory)
        {
            _factory = factory;
            _httpMessageHandler = _factory.Server.CreateHandler();
        }

        [Fact]
        public async Task GetCurrentMatch_WithoutMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.Null(_clientErrorMessage);
            Assert.Null(_serverErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithIncorrectMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, matchId: "12345-6789");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.NotNull(_clientErrorMessage);
            Assert.Null(_serverErrorMessage);
            Assert.Matches("incorrect format", _clientErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithNotExistentMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, matchId: "38B56259-CCCC-4821-AA4F-D83ED7B58FDF");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.NotNull(_clientErrorMessage);
            Assert.Null(_serverErrorMessage);
            Assert.Matches("wasn't found", _clientErrorMessage);
            Assert.Equal(default, _matchInfo);
        }

        [Fact]
        public async Task GetCurrentMatch_WithExistentMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, matchId: "38B56259-55C0-4821-AA4F-D83ED7B58FDF");

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();
            await WaitForResponse(1000);

            //Assert
            Assert.Null(_clientErrorMessage);
            Assert.Null(_serverErrorMessage);
            Assert.NotEqual(default, _matchInfo);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            _responseReceived = false;
            _matchInfo = default;
            _clientErrorMessage = null;
            _serverErrorMessage = null;

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            DbHelper.ReinitDbForTests(db);

            return Task.CompletedTask;
        }

        private void SetConnectionHandlers(HubConnection hubConnection)
        {
            hubConnection.On<MatchInfo>("LoadMatchInfo", (mi) =>
            {
                _matchInfo = mi;

                _responseReceived = true;
            });
            hubConnection.On<string, string>("ClientError", (errMsg, _) =>
            {
                _clientErrorMessage = errMsg;

                _responseReceived = true;
            });
            hubConnection.On<string>("ServerError", (errMsg) =>
            {
                _serverErrorMessage = errMsg;

                _responseReceived = true;
            });
        }

        private Task WaitForResponse(int timeout)
        {
            var startTime = DateTime.Now;
            while (!_responseReceived)
            {
                if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(timeout))
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
