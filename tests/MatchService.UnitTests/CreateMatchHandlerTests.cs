using AutoFixture;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.CreateMatch;
using MatchService.Interfaces;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class CreateMatchHandlerTests
    {
        private readonly Mock<IMatchRepository> _matchRepositoryMock;
        private readonly Fixture _fixture;

        public CreateMatchHandlerTests()
        {
            _matchRepositoryMock = new Mock<IMatchRepository>();

            _fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(_fixture);
        }

        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new CreateMatchHandler(_matchRepositoryMock.Object);
            var matchCommand = new CreateMatchCommand(It.IsAny<CreateMatchDto>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotNullMatch_ThrowsDuplicateMatchException()
        {
            //Arrange
            var sut = new CreateMatchHandler(_matchRepositoryMock.Object);
            var createMatchDto = _fixture.Create<CreateMatchDto>();
            var matchCommand = new CreateMatchCommand(createMatchDto, _fixture.Create<string>());
            var match = _fixture.Create<Match>();

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.CreateMatchDto.MatchId)).ReturnsAsync(match);

            //Act

            //Assert
            await Assert.ThrowsAsync<DuplicateMatchException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new CreateMatchHandler(_matchRepositoryMock.Object);
            var createMatchDto = _fixture.Create<CreateMatchDto>();
            var matchCommand = new CreateMatchCommand(createMatchDto, _fixture.Create<string>());
            Match? match = null;

            _matchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.CreateMatchDto.MatchId)).ReturnsAsync((Match?)null);
            _matchRepositoryMock.Setup(x => x.AddMatch(It.IsAny<Match>())).Callback<Match>(r => match = r);

            //Act
            var result = await sut.Handle(matchCommand, CancellationToken.None);

            //Assert

            var matchCreatedDto = new MatchCreatedDto
            {
                MatchId = match!.MatchId,
                CreatedAtUtc = match.CreatedAtUtc,
                Creator = match.Creator
            };

            Assert.Equal(matchCreatedDto, result);
        }
    }
}
