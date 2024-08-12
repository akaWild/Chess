using AutoFixture;
using EventsLib;
using ExpirationService.Interfaces;
using ExpirationService.Models;
using ExpirationService.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ExpirationService.UnitTests
{
    public class CheckMatchExpiredServiceTests
    {
        private readonly Fixture _fixture;
        private readonly IServiceCollection _services;
        private readonly Mock<IExpirationCollectionService> _expirationServiceMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointServiceMock;

        public CheckMatchExpiredServiceTests()
        {
            _fixture = new Fixture();
            _services = new ServiceCollection();
            _expirationServiceMock = new Mock<IExpirationCollectionService>();
            _publishEndpointServiceMock = new Mock<IPublishEndpoint>();
        }

        [Fact]
        public async Task CheckExpiredMatches_WithNoExpiredMatches_ShouldNotPublishEvents()
        {
            //Arrange
            _expirationServiceMock.Setup(x => x.GetExpiredMatches()).Returns(new MatchExpirationInfo[] { });
            _publishEndpointServiceMock.Setup(x => x.Publish(It.IsAny<TimedOut>(), CancellationToken.None)).Returns(Task.CompletedTask);

            _services.AddSingleton<IHostedService, CheckMatchExpiredService>();
            _services.AddSingleton(_expirationServiceMock.Object);
            _services.AddSingleton(_publishEndpointServiceMock.Object);

            var serviceProvider = _services.BuildServiceProvider();
            var hostedService = serviceProvider.GetService<IHostedService>();

            //Act
            await hostedService.StartAsync(CancellationToken.None);
            await Task.Delay(100);
            await hostedService.StopAsync(CancellationToken.None);

            //Assert
            _expirationServiceMock.Verify(x => x.GetExpiredMatches(), Times.Once);
            _publishEndpointServiceMock.Verify(x => x.Publish(It.IsAny<TimedOut>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task CheckExpiredMatches_With1ExpiredMatch_ShouldPublish1Event()
        {
            //Arrange
            var match = new Fixture().Create<MatchExpirationInfo>();
            var timedOutEvent = new TimedOut(match.MatchId, match.SideToAct);

            _expirationServiceMock.Setup(x => x.GetExpiredMatches()).Returns(new MatchExpirationInfo[] { match });
            _publishEndpointServiceMock.Setup(x => x.Publish(It.IsAny<TimedOut>(), CancellationToken.None)).Returns(Task.CompletedTask);

            _services.AddSingleton<IHostedService, CheckMatchExpiredService>();
            _services.AddSingleton(_expirationServiceMock.Object);
            _services.AddSingleton(_publishEndpointServiceMock.Object);

            var serviceProvider = _services.BuildServiceProvider();
            var hostedService = serviceProvider.GetService<IHostedService>();

            //Act
            await hostedService.StartAsync(CancellationToken.None);
            await Task.Delay(100);
            await hostedService.StopAsync(CancellationToken.None);

            //Assert
            _expirationServiceMock.Verify(x => x.GetExpiredMatches(), Times.Once);
            _publishEndpointServiceMock.Verify(x => x.Publish(timedOutEvent, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CheckExpiredMatches_With2ExpiredMatches_ShouldPublish2Events()
        {
            //Arrange
            var matches = new Fixture().CreateMany<MatchExpirationInfo>(2).ToArray();
            var timedOutEvents = matches.Select(m => new TimedOut(m.MatchId, m.SideToAct)).ToArray();

            _expirationServiceMock.Setup(x => x.GetExpiredMatches()).Returns(matches);
            _publishEndpointServiceMock.Setup(x => x.Publish(It.IsAny<TimedOut>(), CancellationToken.None)).Returns(Task.CompletedTask);

            _services.AddSingleton<IHostedService, CheckMatchExpiredService>();
            _services.AddSingleton(_expirationServiceMock.Object);
            _services.AddSingleton(_publishEndpointServiceMock.Object);

            var serviceProvider = _services.BuildServiceProvider();
            var hostedService = serviceProvider.GetService<IHostedService>();

            //Act
            await hostedService.StartAsync(CancellationToken.None);
            await Task.Delay(100);
            await hostedService.StopAsync(CancellationToken.None);

            //Assert
            _expirationServiceMock.Verify(x => x.GetExpiredMatches(), Times.Once);
            _publishEndpointServiceMock.Verify(x => x.Publish(timedOutEvents[0], CancellationToken.None), Times.Once);
            _publishEndpointServiceMock.Verify(x => x.Publish(timedOutEvents[1], CancellationToken.None), Times.Once);
        }
    }
}
