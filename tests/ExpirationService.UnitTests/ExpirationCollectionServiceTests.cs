using AutoFixture;
using ExpirationService.Models;
using ExpirationService.Services;

namespace ExpirationService.UnitTests
{
    public class ExpirationCollectionServiceTests
    {
        private readonly ExpirationCollectionService _expirationCollectionService;
        private readonly Fixture _fixture;

        public ExpirationCollectionServiceTests()
        {
            _expirationCollectionService = new();
            _fixture = new Fixture();
        }

        [Fact]
        public void Add_Add1Match_ShouldReturnTrue()
        {
            //Arrange

            //Act
            var addResult = _expirationCollectionService.Add(_fixture.Create<MatchExpirationInfo>());

            //Assert
            Assert.True(addResult);
        }

        [Fact]
        public void Add_AddDuplicates_ShouldReturnFalse()
        {
            //Arrange
            var match = _fixture.Create<MatchExpirationInfo>();

            //Act
            _expirationCollectionService.Add(match);
            var addSecondResult = _expirationCollectionService.Add(match);

            //Assert
            Assert.False(addSecondResult);
        }

        [Fact]
        public void Remove_RemoveNonExistentMatch_ShouldReturn0()
        {
            //Arrange

            //Act
            var removeCount = _expirationCollectionService.Remove(_fixture.Create<Guid>());

            //Assert
            Assert.Equal(0, removeCount);
        }

        [Fact]
        public void Remove_RemoveExistentMatch_ShouldReturn1AndContain0Matches()
        {
            //Arrange
            var match = _fixture.Create<MatchExpirationInfo>();

            //Act
            _expirationCollectionService.Add(match);

            var removeCount = _expirationCollectionService.Remove(match.MatchId);
            var count = _expirationCollectionService.Count();

            //Assert
            Assert.Equal(1, removeCount);
            Assert.Equal(0, count);
        }

        [Fact]
        public void GetExpiredMatches_Add1ExpiredMatch_ShouldReturn1MatchAndContain0Matches()
        {
            //Arrange
            var expiredMatch = _fixture.Build<MatchExpirationInfo>().With(p => p.ExpTimeUtc, DateTime.UtcNow - TimeSpan.FromHours(1)).Create();

            //Act
            _expirationCollectionService.Add(expiredMatch);

            var outputMatches = _expirationCollectionService.GetExpiredMatches();
            var count = _expirationCollectionService.Count();

            //Assert
            Assert.Single(outputMatches);
            Assert.Equal(expiredMatch, outputMatches[0]);
            Assert.Equal(0, count);
        }

        [Fact]
        public void GetExpiredMatches_Add1ExpiredAnd1PendingMatch_ShouldReturn1MatchAndContain1Match()
        {
            //Arrange
            var expiredMatch = _fixture.Build<MatchExpirationInfo>().With(p => p.ExpTimeUtc, DateTime.UtcNow - TimeSpan.FromHours(1)).Create();
            var pendingMatch = _fixture.Build<MatchExpirationInfo>().With(p => p.ExpTimeUtc, DateTime.UtcNow + TimeSpan.FromHours(1)).Create();

            //Act
            _expirationCollectionService.Add(expiredMatch);
            _expirationCollectionService.Add(pendingMatch);

            var outputMatches = _expirationCollectionService.GetExpiredMatches();
            var count = _expirationCollectionService.Count();
            var countByExpiredMatchId = _expirationCollectionService.Count(expiredMatch.MatchId);

            //Assert
            Assert.Single(outputMatches);
            Assert.Equal(expiredMatch, outputMatches[0]);
            Assert.Equal(1, count);
            Assert.Equal(0, countByExpiredMatchId);
        }
    }
}