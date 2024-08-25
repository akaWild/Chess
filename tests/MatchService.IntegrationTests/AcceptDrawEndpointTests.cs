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
    public class AcceptDrawEndpointTests : EndpointTestsBase
    {
        private readonly string? _tokenWithoutUser;
        private readonly string? _tolianToken;
        private readonly string? _kolianToken;

        private MatchFinishedDto? _matchFinishedDto;

        public AcceptDrawEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
            _tokenWithoutUser = TokenHelper.GetAccessToken(factory);
            _tolianToken = TokenHelper.GetAccessToken(factory, "Tolian");
            _kolianToken = TokenHelper.GetAccessToken(factory, "Kolian");
        }

        public override Task DisposeAsync()
        {
            _matchFinishedDto = null;

            return base.DisposeAsync();
        }

        [Fact]
        public async Task AcceptDraw_WithoutToken()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithoutUser()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithUnknownMatchId()
        {
            //Arrange
            var matchId = Fixture.Create<Guid>();
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithInvalidUser()
        {
            //Arrange
            var matchId = Guid.Parse("1DA76931-6686-4F74-BB49-32157C6FB67A");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithInvalidMatchStatus()
        {
            //Arrange
            var matchId = Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithoutPrevDrawRequest()
        {
            //Arrange
            var matchId = Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't previous request", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithAcceptedDrawByIdleSideAsWhite()
        {
            //Arrange
            var matchId = Guid.Parse("BD0C1A6B-6BD0-4457-B4BA-74BD3ABD45C3");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithAcceptedDrawByIdleSideAsBlack()
        {
            //Arrange
            var matchId = Guid.Parse("E9439209-2F4D-417E-8905-C5264756248B");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task AcceptDraw_WithValidInputWithoutTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("BD0C1A6B-6BD0-4457-B4BA-74BD3ABD45C3");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(matchId, _matchFinishedDto.MatchId);
            Assert.InRange(_matchFinishedDto.EndedAtUtc, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.Null(_matchFinishedDto.Winner);
            Assert.Null(_matchFinishedDto.WinBy);
            Assert.NotNull(_matchFinishedDto.DrawBy);
            Assert.Equal(nameof(DrawDescriptor.Agreement), _matchFinishedDto.DrawBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task AcceptDraw_WithValidInputWithTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("CB3E242A-86F0-4D76-818E-C79C7C268C5E");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("AcceptDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(matchId, _matchFinishedDto.MatchId);
            Assert.InRange(_matchFinishedDto.EndedAtUtc, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.NotNull(_matchFinishedDto.Winner);
            Assert.NotNull(_matchFinishedDto.WinBy);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.Equal(nameof(WinDescriptor.OnTime), _matchFinishedDto.WinBy);
            Assert.Equal("Kolian", _matchFinishedDto.Winner);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
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
