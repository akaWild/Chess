using AutoFixture;
using EventsLib;
using MassTransit.Internals;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class TimedOutConsumerTests : EndpointTestsBase
    {
        private readonly string? _tolianToken;
        private readonly string? _kolianToken;

        private MatchFinishedDto? _matchFinishedDto;

        public TimedOutConsumerTests(CustomWebAppFactory factory) : base(factory)
        {
            _tolianToken = TokenHelper.GetAccessToken(factory, "Tolian");
            _kolianToken = TokenHelper.GetAccessToken(factory, "Kolian");
        }

        [Fact]
        public async Task ConsumeTimedOutEvent_WithUnknownMatchId()
        {
            //Arrange
            var message = Fixture.Create<TimedOut>();

            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: message.MatchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            await WaitForResponse(2000);

            //Assert
            Assert.NotNull(ClientErrorMessage);
            Assert.Matches("wasn't found", ClientErrorMessage);
        }

        [Fact]
        public async Task ConsumeTimedOutEvent_WithInvalidMatchStatus()
        {
            //Arrange
            var message = Fixture.Build<TimedOut>()
                .With(x => x.MatchId, Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F"))
                .Create();

            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tolianToken, matchId: message.MatchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await Harness.Bus.Publish(message));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.Null((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == message.MatchId));
        }

        [Fact]
        public async Task ConsumeTimedOutEvent_WithValidInputAsBlack()
        {
            //Arrange
            var message = Fixture.Build<TimedOut>()
                .With(x => x.MatchId, Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF"))
                .With(x => x.TimedOutSide, 0)
                .Create();

            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: message.MatchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await Harness.Bus.Publish(message));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(message.MatchId, _matchFinishedDto.MatchId);
            Assert.Equal("Kolian", _matchFinishedDto.Winner);
            Assert.Equal("OnTime", _matchFinishedDto.WinBy);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == message.MatchId));
        }

        [Fact]
        public async Task ConsumeTimedOutEvent_WithValidInputAsWhite()
        {
            //Arrange
            var message = Fixture.Build<TimedOut>()
                .With(x => x.MatchId, Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E"))
                .With(x => x.TimedOutSide, 1)
                .Create();

            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _kolianToken, matchId: message.MatchId.ToString());

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await Harness.Bus.Publish(message));

            await WaitForResponse(2000);

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchFinishedDto);
            Assert.Equal(message.MatchId, _matchFinishedDto.MatchId);
            Assert.Equal("Tolian", _matchFinishedDto.Winner);
            Assert.Equal("OnTime", _matchFinishedDto.WinBy);
            Assert.Null(_matchFinishedDto.DrawBy);
            Assert.NotNull((await Harness.Published.SelectAsync<MatchFinished>().ToListAsync()).FirstOrDefault(v => v.Context.Message.MatchId == message.MatchId));
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
