using AutoFixture;
using EventsLib;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.RequestDraw;
using MatchService.Interfaces;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class RequestDrawHandlerTests : HandlerTestsBase
    {
        private readonly Mock<ILocalExpirationService> _expServiceMock;

        public RequestDrawHandlerTests()
        {
            _expServiceMock = new Mock<ILocalExpirationService>();
        }

        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithVsBot_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, 10)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only on human vs human match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithNotParticipant_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, (int?)null)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task Handle_WithNotInProgressMatchStatus_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var matchStatus = Fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.InProgress);
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, (int?)null)
                .With(x => x.Status, matchStatus)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedSideNotNull_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, (int?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, Fixture.Create<MatchSide>())
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("has been already requested", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByActiveSideAsWhite_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, (int?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only by idle side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByActiveSideAsBlack_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.AILevel, (int?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only by idle side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithValidInputAsWhiteNoExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.AILevel, (int?)null)
                .With(x => x.StartedAtUtc, DateTime.UtcNow)
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.WhiteSideTimeRemaining, 1000)
                .With(x => x.BlackSideTimeRemaining, 1000)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var drawRequestedDto = await sut.Handle(matchCommand, CancellationToken.None) as DrawRequestedDto;

            //Assert
            Assert.NotNull(drawRequestedDto);
            Assert.Equal(match.MatchId, drawRequestedDto.MatchId);
            Assert.Equal(1, drawRequestedDto.RequestSide);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<DrawRequested>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidInputAsWhiteWithExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.AILevel, (int?)null)
                .With(x => x.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMinutes(5))
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.WhiteSideTimeRemaining, 60)
                .With(x => x.BlackSideTimeRemaining, 60)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);
            _expServiceMock.Setup(x => x.GetWinner(match)).Returns(Fixture.Create<string>());

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, match.MatchId);
            Assert.NotNull(match.Winner);
            Assert.NotNull(match.WinBy);
            Assert.Null(match.DrawBy);
            Assert.Null(match.DrawRequestedSide);
            Assert.Equal(WinDescriptor.OnTime, match.WinBy);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<DrawRequested>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidInputAsBlackNoExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.AILevel, (int?)null)
                .With(x => x.StartedAtUtc, DateTime.UtcNow)
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .With(x => x.WhiteSideTimeRemaining, 1000)
                .With(x => x.BlackSideTimeRemaining, 1000)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var drawRequestedDto = await sut.Handle(matchCommand, CancellationToken.None) as DrawRequestedDto;

            //Assert
            Assert.NotNull(drawRequestedDto);
            Assert.Equal(match.MatchId, drawRequestedDto.MatchId);
            Assert.Equal(0, drawRequestedDto.RequestSide);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<DrawRequested>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidInputAsBlackWithExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.AILevel, (int?)null)
                .With(x => x.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMinutes(5))
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .With(x => x.WhiteSideTimeRemaining, 60)
                .With(x => x.BlackSideTimeRemaining, 60)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);
            _expServiceMock.Setup(x => x.GetWinner(match)).Returns(Fixture.Create<string>());

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, match.MatchId);
            Assert.NotNull(match.Winner);
            Assert.NotNull(match.WinBy);
            Assert.Null(match.DrawBy);
            Assert.Null(match.DrawRequestedSide);
            Assert.Equal(WinDescriptor.OnTime, match.WinBy);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<DrawRequested>(), CancellationToken.None), Times.Never);
        }
    }
}
