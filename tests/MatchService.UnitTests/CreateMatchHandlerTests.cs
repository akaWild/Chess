using AutoFixture;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.CreateMatch;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class CreateMatchHandlerTests : HandlerTestsBase
    {
        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new CreateMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new CreateMatchCommand(It.IsAny<CreateMatchDto>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotNullMatch_ThrowsDuplicateMatchException()
        {
            //Arrange
            var sut = new CreateMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var createMatchDto = Fixture.Create<CreateMatchDto>();
            var matchCommand = new CreateMatchCommand(createMatchDto, Fixture.Create<string>());
            var match = Fixture.Create<Match>();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.CreateMatchDto.MatchId)).ReturnsAsync(match);

            //Act

            //Assert
            await Assert.ThrowsAsync<DuplicateMatchException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new CreateMatchHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var createMatchDto = Fixture.Create<CreateMatchDto>();
            var matchCommand = new CreateMatchCommand(createMatchDto, Fixture.Create<string>());
            Match? match = null;

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.CreateMatchDto.MatchId)).ReturnsAsync((Match?)null);
            MatchRepositoryMock.Setup(x => x.AddMatch(It.IsAny<Match>())).Callback<Match>(r => match = r);

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
