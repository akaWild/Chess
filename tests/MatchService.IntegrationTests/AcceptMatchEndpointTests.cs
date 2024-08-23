using AutoFixture;
using EventsLib;
using MassTransit.Internals;
using MassTransit.Testing;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class AcceptMatchEndpointTests : EndpointTestsBase
    {
        private readonly string? _tokenWithoutUser;
        private readonly string? _tolianToken;
        private readonly string? _kolianToken;

        private MatchStartedDto? _matchStartedDto;

        public AcceptMatchEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
            _tokenWithoutUser = TokenHelper.GetAccessToken(factory);
            _tolianToken = TokenHelper.GetAccessToken(factory, "Tolian");
            _kolianToken = TokenHelper.GetAccessToken(factory, "Kolian");
        }

        [Fact]
        public async Task AcceptMatch_WithoutToken()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task AcceptMatch_WithoutUser()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task AcceptMatch_WithUnknownMatchId()
        {
            //Arrange
            var matchId = Fixture.Create<Guid>();
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task AcceptMatch_WithInvalidMatchStatus()
        {
            //Arrange
            var matchId = Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Only not started match can be accepted", exception.Message);
        }

        [Fact]
        public async Task AcceptMatch_WithInvalidMatchCreator()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Match can't be accepted by match creator", exception.Message);
        }

        [Fact]
        public async Task AcceptMatch_WithValidInputWhiteSideIsCreator()
        {
            //Arrange
            var matchId = Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchStartedDto);
            Assert.Equal(matchId, _matchStartedDto.MatchId);
            Assert.InRange(_matchStartedDto.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMilliseconds(1000), DateTime.UtcNow);
            Assert.Equal("Kolian", _matchStartedDto.Acceptor);
            Assert.Equal("Tolian", _matchStartedDto.WhiteSidePlayer);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchStarted>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task AcceptMatch_WithValidInputWhiteSideIsAcceptor()
        {
            //Arrange
            var matchId = Guid.Parse("03010934-839C-4DF9-BA11-643EC0FB120C");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchStartedDto);
            Assert.Equal(matchId, _matchStartedDto.MatchId);
            Assert.InRange(_matchStartedDto.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMilliseconds(1000), DateTime.UtcNow);
            Assert.Equal("Kolian", _matchStartedDto.Acceptor);
            Assert.Equal("Kolian", _matchStartedDto.WhiteSidePlayer);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchStarted>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task AcceptMatch_WithValidInputWhiteSideIsAcceptorAndTimeLimitIsNotNull()
        {
            //Arrange
            var matchId = Guid.Parse("03ABA126-1ABA-4CA0-A2CF-F7B9255C787D");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptMatch", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchStartedDto);
            Assert.Equal(matchId, _matchStartedDto.MatchId);
            Assert.InRange(_matchStartedDto.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMilliseconds(1000), DateTime.UtcNow);
            Assert.Equal("Kolian", _matchStartedDto.Acceptor);
            Assert.Equal("Tolian", _matchStartedDto.WhiteSidePlayer);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchStarted>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
            Assert.NotNull((await Harness.Published.SelectAsync<SideToActChanged>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        public override Task DisposeAsync()
        {
            _matchStartedDto = null;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<MatchStartedDto>("MatchStarted", (dto) =>
            {
                _matchStartedDto = dto;

                ResponseReceived = true;
            });
        }
    }
}
