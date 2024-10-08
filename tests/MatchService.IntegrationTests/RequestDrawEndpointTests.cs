﻿using AutoFixture;
using EventsLib;
using MassTransit.Internals;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class RequestDrawEndpointTests : EndpointTestsBase
    {
        private DrawRequestedDto? _drawRequestedDto;
        private MatchFinishedDto? _matchFinishedDto;

        public RequestDrawEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task RequestDraw_WithoutToken()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithoutUser()
        {
            //Arrange
            var matchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TokenWithoutUser, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithUnknownMatchId()
        {
            //Arrange
            var matchId = Fixture.Create<Guid>();
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("wasn't found", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithVsBot()
        {
            //Arrange
            var matchId = Guid.Parse("5AF2E004-6342-4FD1-B4F6-2E0A093D3025");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only on human vs human match", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithInvalidUser()
        {
            //Arrange
            var matchId = Guid.Parse("1DA76931-6686-4F74-BB49-32157C6FB67A");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithInvalidMatchStatus()
        {
            //Arrange
            var matchId = Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithAlreadyRequestedDraw()
        {
            //Arrange
            var matchId = Guid.Parse("88275D09-CC64-47E4-B433-7BE6B3DB47A1");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("already requested", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithRequestedDrawByActiveSideAsWhite()
        {
            //Arrange
            var matchId = Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by idle side of the match", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithRequestedDrawByActiveSideAsBlack()
        {
            //Arrange
            var matchId = Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("only by idle side of the match", exception.Message);
        }

        [Fact]
        public async Task RequestDraw_WithValidInputAsBlackWithoutTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_drawRequestedDto);
            Assert.Equal(matchId, _drawRequestedDto.MatchId);
            Assert.Equal(1, _drawRequestedDto.RequestSide);
            Assert.NotNull((await Harness.Published.SelectAsync<DrawRequested>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task RequestDraw_WithValidInputAsBlackWithTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("61711E02-A185-40EF-A0EA-67FE88D72E1D");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: KolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(matchId, _matchFinishedDto.MatchId);
            Assert.Equal("Kolian", _matchFinishedDto.Winner);
            Assert.Equal("OnTime", _matchFinishedDto.WinBy);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task RequestDraw_WithValidInputAsWhiteWithoutTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_drawRequestedDto);
            Assert.Equal(matchId, _drawRequestedDto.MatchId);
            Assert.Equal(0, _drawRequestedDto.RequestSide);
            Assert.NotNull((await Harness.Published.SelectAsync<DrawRequested>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        [Fact]
        public async Task RequestDraw_WithValidInputAsWhiteWithTimeout()
        {
            //Arrange
            var matchId = Guid.Parse("97B66057-C123-4671-9F14-BE33AA08916F");
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: TolianToken, matchId: matchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("RequestDraw", matchId));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(matchId, _matchFinishedDto.MatchId);
            Assert.Equal("Tolian", _matchFinishedDto.Winner);
            Assert.Equal("OnTime", _matchFinishedDto.WinBy);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == matchId));
        }

        public override Task DisposeAsync()
        {
            _drawRequestedDto = null;
            _matchFinishedDto = null;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<DrawRequestedDto>("DrawRequested", (dto) =>
            {
                _drawRequestedDto = dto;

                ResponseReceived = true;
            });
            hubConnection.On<MatchFinishedDto>("MatchFinished", (dto) =>
            {
                _matchFinishedDto = dto;

                ResponseReceived = true;
            });
        }
    }
}
