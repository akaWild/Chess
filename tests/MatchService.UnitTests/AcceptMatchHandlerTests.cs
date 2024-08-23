using AutoFixture;
using AutoMapper;
using ChessDotNet.Public;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.AcceptMatch;
using MatchService.Interfaces;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class AcceptMatchHandlerTests
    {
        private readonly Mock<IMatchRepository> _matchRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpoint;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Fixture _fixture;

        public AcceptMatchHandlerTests()
        {
            _matchRepositoryMock = new Mock<IMatchRepository>();
            _publishEndpoint = new Mock<IPublishEndpoint>();
            _mapperMock = new Mock<IMapper>();

            _fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(_fixture);
        }

        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new AcceptMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object, _mapperMock.Object);
            var matchCommand = new AcceptMatchCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new AcceptMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object, _mapperMock.Object);
            var matchCommand = new AcceptMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotCreatedMatchStatus_ThrowsMatchMatchAcceptanceException()
        {
            //Arrange
            var sut = new AcceptMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object, _mapperMock.Object);
            var matchCommand = new AcceptMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var matchStatus = _fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.Created);
            var match = _fixture.Build<Match>().With(x => x.Status, matchStatus).Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<MatchAcceptanceException>(exception);
            Assert.Matches("not started match can be accepted", exception.Message);
        }

        [Fact]
        public async Task Handle_WithIsMatchCreator_ThrowsMatchMatchAcceptanceException()
        {
            //Arrange
            var sut = new AcceptMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object, _mapperMock.Object);
            var matchCommand = new AcceptMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());
            var match = _fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.Created)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<MatchAcceptanceException>(exception);
            Assert.Matches("can't be accepted by match creator", exception.Message);
        }

        [Fact]
        public async Task Handle_WithIsMatchCreator_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new AcceptMatchHandler(_matchRepositoryMock.Object, _publishEndpoint.Object, _mapperMock.Object);
            var matchCommand = new AcceptMatchCommand(It.IsAny<Guid>(), _fixture.Create<string>());

            string? whiteSidePlayer = null;
            int? timeLimit = null;

            if (Random.Shared.Next(0, 2) == 0)
                whiteSidePlayer = $"{Random.Shared.Next(0, 2)}";

            if (Random.Shared.Next(0, 2) == 0)
                timeLimit = Random.Shared.Next(0, 1000);

            var match = _fixture.Build<Match>()
                .Without(x => x.WhiteSideTimeRemaining)
                .Without(x => x.BlackSideTimeRemaining)
                .With(x => x.Status, MatchStatus.Created)
                .With(x => x.WhiteSidePlayer, whiteSidePlayer)
                .With(x => x.TimeLimit, timeLimit)
                .Create();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(matchCommand.User, match.Acceptor);
            Assert.NotNull(match.StartedAtUtc);
            Assert.InRange(match.StartedAtUtc.Value, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.NotNull(match.WhiteSidePlayer);

            if (whiteSidePlayer == null)
                Assert.Matches($"({match.Creator}|{match.Acceptor})", match.WhiteSidePlayer);
            else
            {
                var vspValue = string.Empty;

                if (whiteSidePlayer == "0")
                    vspValue = match.Creator;
                else if (whiteSidePlayer == "1")
                    vspValue = matchCommand.User;

                Assert.Equal(vspValue, match.WhiteSidePlayer);
            }

            Assert.Equal(MatchSide.White, match.ActingSide);
            Assert.Equal(MatchStatus.InProgress, match.Status);
            Assert.Equal(PublicData.DefaultChessPosition, match.Board);

            if (match.TimeLimit == null)
            {
                Assert.Null(match.WhiteSideTimeRemaining);
                Assert.Null(match.BlackSideTimeRemaining);
            }
            else
            {
                Assert.Equal(match.TimeLimit, match.WhiteSideTimeRemaining);
                Assert.Equal(match.TimeLimit, match.BlackSideTimeRemaining);
            }

            _matchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(x => x.Map<MatchStartedDto>(It.IsAny<Match>()), Times.Once);
        }
    }
}
