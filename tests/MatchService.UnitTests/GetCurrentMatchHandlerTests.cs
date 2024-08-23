using AutoFixture;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.GetCurrentMatch;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class GetCurrentMatchHandlerTests : HandlerTestsBase
    {
        [Fact]
        public async Task Handle_WithNotFoundMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new GetCurrentMatchHandler(MatchRepositoryMock.Object, MapperMock.Object);
            var matchQuery = new GetCurrentMatchQuery(It.IsNotNull<Guid>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchQuery.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchQuery, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithFoundMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(fixture);

            var sut = new GetCurrentMatchHandler(MatchRepositoryMock.Object, MapperMock.Object);
            var matchQuery = new GetCurrentMatchQuery(It.IsNotNull<Guid>());
            var match = fixture.Create<Match>();
            var matchInfo = fixture.Create<MatchInfo>();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchQuery.MatchId)).ReturnsAsync(match);
            MapperMock.Setup(x => x.Map<MatchInfo>(match)).Returns(matchInfo);

            //Act
            var result = await sut.Handle(matchQuery, CancellationToken.None);

            //Assert
            Assert.Equal(matchInfo, result);
        }
    }
}
