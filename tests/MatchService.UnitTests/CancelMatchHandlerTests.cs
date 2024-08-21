using AutoFixture;
using MassTransit;
using MatchService.Exceptions;
using MatchService.Features.CancelMatch;
using MatchService.Interfaces;
using MatchService.Models;
using MediatR;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class CancelMatchHandlerTests
    {
        private readonly Mock<IMatchRepository> _matchRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        private readonly Fixture _fixture;

        public CancelMatchHandlerTests()
        {
            _matchRepositoryMock = new Mock<IMatchRepository>();
            _publishEndpoint = new Mock<IPublishEndpoint>();

            _fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(_fixture);
        }

        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new CancelMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotFoundMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new CancelMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotCreatedMatchStatus_ThrowsMatchCancellationException()
        {
            //Arrange
            var sut = new CancelMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var matchStatus = _fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.Created);
            var match = _fixture.Build<Match>().With(x => x.Status, matchStatus).Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchCancellationException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUserNotEqualToMatchCreator_ThrowsMatchCancellationException()
        {
            //Arrange
            var sut = new CancelMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>().With(x => x.Status, MatchStatus.Created).Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchCancellationException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithValidInput_ThrowsNoExceptions()
        {
            //Arrange
            var sut = new CancelMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new CancelMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.Created)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var result = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(Unit.Value, result);

            _matchRepositoryMock.Verify(x => x.RemoveMatch(It.IsAny<Match>()), Times.Once);
            _matchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
