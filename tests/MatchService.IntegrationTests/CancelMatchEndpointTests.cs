using AutoFixture;
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

        public CancelMatchEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
            _tokenWithoutUser = TokenHelper.GetAccessToken(factory);
            _tolianToken = TokenHelper.GetAccessToken(factory, "Tolian");
            _kolianToken = TokenHelper.GetAccessToken(factory, "Kolian");
        }

        [Fact]
        public async Task CancelMatch_WithoutToken()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", Fixture.Create<Guid>()));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithoutUser()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", Fixture.Create<Guid>()));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithUnknownMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", Fixture.Create<Guid>()));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithInvalidMatchStatus()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", "38B56259-55C0-4821-AA4F-D83ED7B58FDF"));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Only not started match can be cancelled", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithInvalidMatchCreator()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", "7139D633-66F9-439F-8198-E5E18E9F6848"));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Match can be cancelled only by match creator", exception.Message);
        }

        [Fact]
        public async Task CancelMatch_WithValidInputData()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CancelMatch", "7139D633-66F9-439F-8198-E5E18E9F6848"));

            //Assert
            Assert.Null(exception);
        }
    }
}
