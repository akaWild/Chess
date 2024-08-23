using AutoFixture;
using AutoMapper;
using MassTransit;
using MatchService.Interfaces;
using Moq;

namespace MatchService.UnitTests
{
    public abstract class HandlerTestsBase
    {
        protected readonly Mock<IMapper> MapperMock;
        protected readonly Mock<IPublishEndpoint> PublishEndpoint;
        protected readonly Mock<IMatchRepository> MatchRepositoryMock;
        protected readonly Fixture Fixture;

        protected HandlerTestsBase()
        {
            MapperMock = new Mock<IMapper>();
            PublishEndpoint = new Mock<IPublishEndpoint>();
            MatchRepositoryMock = new Mock<IMatchRepository>();

            Fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(Fixture);
        }
    }
}
