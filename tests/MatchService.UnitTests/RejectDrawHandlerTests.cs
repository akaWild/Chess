using AutoFixture;
using EventsLib;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.RejectDraw;
using MatchService.Interfaces;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class RejectDrawHandlerTests : HandlerTestsBase
    {
        private readonly Mock<ILocalExpirationService> _expServiceMock;

        public RejectDrawHandlerTests()
        {
            _expServiceMock = new Mock<ILocalExpirationService>();
        }

        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotParticipant_ThrowsMatchDrawRejectException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Create<Match>();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRejectException>(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task Handle_WithNotInProgressMatchStatus_ThrowsMatchDrawRejectException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var matchStatus = Fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.InProgress);
            var match = Fixture.Build<Match>()
                .With(x => x.Status, matchStatus)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRejectException>(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedSideNull_ThrowsMatchDrawRejectException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRejectException>(exception);
            Assert.Matches("there wasn't previous request", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByIdleSideAsWhite_ThrowsMatchDrawRejectException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.White)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRejectException>(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByIdleSideAsBlack_ThrowsMatchDrawRejectException()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawRejectException>(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithValidInputNoExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.StartedAtUtc, DateTime.UtcNow)
                .With(x => x.EndedAtUtc, (DateTime?)null)
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.DrawBy, (DrawDescriptor?)null)
                .With(x => x.WinBy, (WinDescriptor?)null)
                .With(x => x.Winner, (string?)null)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .With(x => x.WhiteSideTimeRemaining, 1000)
                .With(x => x.BlackSideTimeRemaining, 1000)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, matchCommand.MatchId);
            Assert.Null(match.EndedAtUtc);
            Assert.Null(match.Winner);
            Assert.Null(match.WinBy);
            Assert.Null(match.DrawBy);
            Assert.Null(match.DrawRequestedSide);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinished>(It.IsAny<Match>()), Times.Never);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Never);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<DrawRejected>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidInputAndExpiration_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RejectDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object, _expServiceMock.Object);
            var matchCommand = new RejectDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.StartedAtUtc, DateTime.UtcNow - TimeSpan.FromMinutes(5))
                .With(x => x.LastMoveMadeAtUtc, (DateTime?)null)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.DrawBy, (DrawDescriptor?)null)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .With(x => x.WhiteSideTimeRemaining, 60)
                .With(x => x.BlackSideTimeRemaining, 60)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);
            _expServiceMock.Setup(x => x.GetWinner(match)).Returns(Fixture.Create<string>());

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, matchCommand.MatchId);
            Assert.NotNull(match.EndedAtUtc);
            Assert.InRange(match.EndedAtUtc.Value, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.NotNull(match.Winner);
            Assert.NotNull(match.WinBy);
            Assert.Null(match.DrawBy);
            Assert.Equal(WinDescriptor.OnTime, match.WinBy);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinished>(It.IsAny<Match>()), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Once);
        }
    }
}
