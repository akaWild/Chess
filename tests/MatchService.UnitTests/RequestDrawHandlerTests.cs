using AutoFixture;
using MassTransit;
using MatchService.Exceptions;
using MatchService.Features.RequestDraw;
using MatchService.Interfaces;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class RequestDrawHandlerTests
    {
        private readonly Mock<IMatchRepository> _matchRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        private readonly Fixture _fixture;

        public RequestDrawHandlerTests()
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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotParticipant_ThrowsMatchDrawRequestException()
        {
            //Arrange
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Create<Match>();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var matchStatus = _fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.InProgress);
            var match = _fixture.Build<Match>()
                .With(x => x.Status, matchStatus)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, _fixture.Create<MatchSide>())
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(_fixture.Create<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.White)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

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
            var sut = new RequestDrawHandler(_matchRepositoryMock.Object, _publishEndpoint.Object);
            var matchCommand = new RequestDrawCommand(_fixture.Create<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            var drawRequestedDto = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, drawRequestedDto.MatchId);
            Assert.Equal(0, drawRequestedDto.RequestSide);
        }
    }
}
