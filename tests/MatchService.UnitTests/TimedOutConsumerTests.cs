using AutoFixture;
using EventsLib;
using MassTransit;
using MatchService.Consumers;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features;
using MatchService.Models;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class TimedOutConsumerTests : HandlerTestsBase
    {
        private readonly Mock<IHubContext<MatchHub>> _hubContextMock;
        private readonly Mock<ConsumeContext<TimedOut>> _consumeContextMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<ISingleClientProxy> _clientProxyMock;

        public TimedOutConsumerTests()
        {
            _hubContextMock = new Mock<IHubContext<MatchHub>>();
            _consumeContextMock = new Mock<ConsumeContext<TimedOut>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<ISingleClientProxy>();
        }

        [Fact]
        public async Task Consume_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new TimedOutConsumer(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _hubContextMock.Object);
            var message = Fixture.Create<TimedOut>();

            MatchRepositoryMock.Setup(x => x.GetMatchById(message.MatchId)).ReturnsAsync((Match?)null);
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Consume(_consumeContextMock.Object));
        }

        [Fact]
        public async Task Consume_WithNotInProgressMatchStatus_ThrowsNoException()
        {
            //Arrange
            var sut = new TimedOutConsumer(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _hubContextMock.Object);
            var message = Fixture.Create<TimedOut>();
            var matchStatus = Fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.InProgress);
            var match = Fixture.Build<Match>()
                .With(x => x.Status, matchStatus)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(message.MatchId)).ReturnsAsync(match);
            _consumeContextMock.Setup(x => x.Message).Returns(message);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Consume(_consumeContextMock.Object));

            //Assert
            Assert.Null(exception);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            MapperMock.Verify(x => x.Map<MatchFinished>(It.IsAny<Match>()), Times.Never);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task Consume_WithValidInputAsWhiteSide_ReturnsCorrectData(int timeoutSide)
        {
            //Arrange
            var sut = new TimedOutConsumer(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _hubContextMock.Object);
            var message = Fixture.Build<TimedOut>()
                .With(x => x.TimedOutSide, timeoutSide)
                .Create();
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .Create();

            string? winner = null;

            if (timeoutSide == 0)
                winner = match.WhiteSidePlayer == match.Creator ? match.Acceptor : match.Creator;
            else if (timeoutSide == 1)
                winner = match.WhiteSidePlayer == match.Creator ? match.Creator : match.Acceptor;

            MatchRepositoryMock.Setup(x => x.GetMatchById(message.MatchId)).ReturnsAsync(match);
            _consumeContextMock.Setup(x => x.Message).Returns(message);
            _hubContextMock.Setup(x => x.Clients).Returns(_clientsMock.Object);
            _clientsMock.Setup(clients => clients.Group(message.MatchId.ToString())).Returns(_clientProxyMock.Object);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Consume(_consumeContextMock.Object));

            //Assert
            Assert.Null(exception);
            Assert.NotNull(match.EndedAtUtc);
            Assert.InRange(match.EndedAtUtc.Value, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.NotNull(match.Winner);
            Assert.NotNull(match.WinBy);
            Assert.Null(match.DrawBy);
            Assert.Equal(MatchStatus.Finished, match.Status);
            Assert.Equal(WinDescriptor.OnTime, match.WinBy);
            Assert.Equal(winner, match.Winner);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinished>(It.IsAny<Match>()), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchFinished", new object[] { It.IsAny<MatchFinishedDto>() }, CancellationToken.None), Times.Once);
        }
    }
}
