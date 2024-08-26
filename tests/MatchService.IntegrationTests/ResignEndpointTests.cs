using AutoFixture;
using EventsLib;
using MassTransit.Internals;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using MatchService.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class ResignEndpointTests : EndpointTestsBase
    {
        private MatchFinishedDto? _matchFinishedDto;

        public ResignEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Resign_WithoutToken()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task Resign_WithoutUser()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TokenWithoutUser, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task Resign_WithUnknownMatchId()
        {
            //Arrange
            var matchId = Fixture.Create<Guid>();
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task Resign_WithInvalidUser()
        {
            //Arrange
            var matchId = Guid.Parse("1DA76931-6686-4F74-BB49-32157C6FB67A");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task Resign_WithInvalidMatchStatus()
        {
            //Arrange
            var matchId = Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task Resign_WithValidInputWithoutTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("BD0C1A6B-6BD0-4457-B4BA-74BD3ABD45C3");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("Resign", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(matchId, _matchFinishedDto.MatchId);
            Assert.InRange(_matchFinishedDto.EndedAtUtc, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.NotNull(_matchFinishedDto.Winner);
            Assert.Equal("Kolian", _matchFinishedDto.Winner);
            Assert.NotNull(_matchFinishedDto.WinBy);
            Assert.Equal(nameof(WinDescriptor.Resignation), _matchFinishedDto.WinBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        public override Task DisposeAsync()
        {
            _matchFinishedDto = null;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<MatchFinishedDto>("MatchFinished", (dto) =>
            {
                _matchFinishedDto = dto;

                ResponseReceived = true;
            });
        }
    }
}
