using AutoFixture;
using MatchService.Exceptions;
using MatchService.Features.CancelMatch;
using MatchService.Models;
using MediatR;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class CancelMatchHandlerTests : HandlerTestsBase
    {
        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new CancelMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotFoundMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new CancelMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), Fixture.Create<string>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotCreatedMatchStatus_ThrowsMatchCancellationException()
        {
            //Arrange
            var sut = new CancelMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var matchStatus = Fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.Created);
            var match = Fixture.Build<Match>()
                .With(x => x.Status, matchStatus)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<MatchCancellationException>(exception);
            Assert.Matches("not started match can be cancelled", exception.Message);
        }

        [Fact]
        public async Task Handle_WithUserNotEqualToMatchCreator_ThrowsMatchCancellationException()
        {
            //Arrange
            var sut = new CancelMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.Created)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<MatchCancellationException>(exception);
            Assert.Matches("cancelled only by match creator", exception.Message);
        }

        [Fact]
        public async Task Handle_WithValidInput_ThrowsNoExceptions()
        {
            //Arrange
            var sut = new CancelMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.Created)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var result = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(Unit.Value, result);

            MatchRepositoryMock.Verify(x => x.RemoveMatch(It.IsAny<Match>()), Times.Once);
            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
