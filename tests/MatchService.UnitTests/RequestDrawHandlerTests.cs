using AutoFixture;
using MatchService.Exceptions;
using MatchService.Features.RequestDraw;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class RequestDrawHandlerTests : HandlerTestsBase
    {
        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotParticipant_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Create<Match>();

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
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
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
            Assert.IsType<DrawRequestException>(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedSideNotNull_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
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
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
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
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
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
        public async Task Handle_WithValidInputAsWhite_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var drawRequestedDto = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, drawRequestedDto.MatchId);
            Assert.Equal(1, drawRequestedDto.RequestSide);
        }

        [Fact]
        public async Task Handle_WithValidInputAsBlack_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new RequestDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var drawRequestedDto = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, drawRequestedDto.MatchId);
            Assert.Equal(0, drawRequestedDto.RequestSide);
        }
    }
}
