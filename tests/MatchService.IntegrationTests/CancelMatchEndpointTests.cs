using AutoFixture;
using EventsLib;
using MassTransit.Internals;
using MassTransit.Testing;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class CancelMatchEndpointTests : EndpointTestsBase
    {
        private readonly string? _tokenWithoutUser;
        private readonly string? _tolianToken;
        private readonly string? _kolianToken;

        private readonly ITestHarness _harness;

        private Guid _matchId;

        public CancelMatchEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
            _harness = Factory.Services.GetTestHarness();

            _tokenWithoutUser = TokenHelper.GetAccessToken(factory);
            _tolianToken = TokenHelper.GetAccessToken(factory, "Tolian");
            _kolianToken = TokenHelper.GetAccessToken(factory, "Kolian");
        }

        [Fact]
        public async Task CancelMatch_WithoutToken()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithoutUser()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithUnknownMatchId()
        {
            //Arrange
            var matchId = Fixture.Create<Guid>();
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithInvalidMatchStatus()
        {
            //Arrange
            var matchId = Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Only not started match can be cancelled", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithInvalidMatchCreator()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Match can be cancelled only by match creator", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithValidInputData()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", matchId));

            await WaitForResponse(5000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull((await _harness.Published.SelectAsync<MatchCancelled>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
            Assert.Equal(matchId, _matchId);
        }

        public override Task DisposeAsync()
        {
            _matchId = default;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<Guid>("MatchCancelled", (matchId) =>
            {
                _matchId = matchId;

                ResponseReceived = true;
            });
        }
    }
}
