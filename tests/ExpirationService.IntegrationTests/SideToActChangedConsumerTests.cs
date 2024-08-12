using AutoFixture;
using EventsLib;
using ExpirationService.Consumers;
using ExpirationService.Interfaces;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ExpirationService.IntegrationTests
{
    public class SideToActChangedConsumerTests
    {
        private readonly ITestHarness _testHarness;
        private readonly IExpirationCollectionService _expirationCollectionService;
        private readonly Fixture _fixture;

        public SideToActChangedConsumerTests()
        {
            var factory = new CustomWebAppFactory().WithWebHostBuilder(builder =>
            {
                builder.Configure(_ => { });
            });

            _testHarness = factory.Services.GetTestHarness();
            _expirationCollectionService = factory.Services.GetRequiredService<IExpirationCollectionService>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task SideToActChanged_ShouldAdd1MatchToExpirationCollectionService()
        {
            //Arrange
            var consumerHarness = _testHarness.GetConsumerHarness<SideToActChangedConsumer>();
            var sideToActChangedEvent = _fixture.Create<SideToActChanged>();

            //Act
            await _testHarness.Bus.Publish(sideToActChangedEvent);

            //Assert
            Assert.True(await consumerHarness.Consumed.Any<SideToActChanged>());
            Assert.Equal(1, _expirationCollectionService.Count(sideToActChangedEvent.MatchId));
        }
    }
}