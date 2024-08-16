using AutoFixture;
using MatchService.DTOs;
using MatchService.Features;
using MatchService.Features.AcceptMatch;
using MatchService.Features.CancelMatch;
using MatchService.Features.CreateMatch;
using MatchService.Features.GetCurrentMatch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Moq;

namespace MatchService.UnitTests
{
    public class MatchHubTests
    {
        private readonly Fixture _fixture;
        private readonly DefaultHttpContext _defaultHttpContext;

        private readonly Mock<HubCallerContext> _contextMock;
        private readonly Mock<IGroupManager> _groupsMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<IHubCallerClients> _clientsMock;
        private readonly Mock<ISingleClientProxy> _clientProxyMock;

        public MatchHubTests()
        {
            _fixture = new Fixture();
            _defaultHttpContext = new DefaultHttpContext();

            _contextMock = new Mock<HubCallerContext>();
            _groupsMock = new Mock<IGroupManager>();
            _senderMock = new Mock<ISender>();
            _clientsMock = new Mock<IHubCallerClients>();
            _clientProxyMock = new Mock<ISingleClientProxy>();
        }

        [Fact]
        public async Task OnConnectedAsync_WithNullContext_NoExceptions()
        {
            //Arrange
            _contextMock.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext).Returns((HttpContext?)null);

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
            };

            //Act

            //Assert
            Assert.Null(await Record.ExceptionAsync(() => hub.OnConnectedAsync()));
        }

        [Fact]
        public async Task OnConnectedAsync_WithNoMatchIdInQuery_NoExceptions()
        {
            //Arrange

            _contextMock.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext).Returns(_defaultHttpContext);

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
            };

            //Act

            //Assert
            Assert.Null(await Record.ExceptionAsync(() => hub.OnConnectedAsync()));
        }

        [Fact]
        public async Task OnConnectedAsync_WithIncorrectMatchIdInQuery_ThrowsHubException()
        {
            //Arrange
            var groupsMock = new Mock<IGroupManager>();

            var defaultHttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Query = new QueryCollection(new Dictionary<string, StringValues>()
                    {
                        {"matchId", "1234"}
                    })
                }
            };

            _contextMock.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext).Returns(defaultHttpContext);
            groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.CompletedTask);

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = groupsMock.Object
            };

            //Act

            //Assert
            await Assert.ThrowsAsync<HubException>(() => hub.OnConnectedAsync());
        }

        [Fact]
        public async Task OnConnectedAsync_WithCorrectMatchIdInQuery_NoExceptions()
        {
            //Arrange
            var defaultHttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Query = new QueryCollection(new Dictionary<string, StringValues>()
                    {
                        {"matchId", "F25166DE-012A-487E-A01E-4F34FEE75B21" }
                    })
                }
            };

            _contextMock.Setup(x => x.Features.Get<IHttpContextFeature>().HttpContext).Returns(defaultHttpContext);
            _groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.CompletedTask);

            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(_fixture);

            var matchInfo = _fixture.Create<MatchInfo>();

            _senderMock.Setup(x => x.Send(It.IsAny<GetCurrentMatchQuery>(), CancellationToken.None)).ReturnsAsync(matchInfo);
            _clientsMock.Setup(clients => clients.Caller).Returns(_clientProxyMock.Object);

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object,
                Clients = _clientsMock.Object
            };

            //Act
            await hub.OnConnectedAsync();

            //Assert
            _groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
            _senderMock.Verify(x => x.Send(It.IsAny<GetCurrentMatchQuery>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("LoadMatchInfo", new object[] { matchInfo }, CancellationToken.None), Times.Once);
            _clientProxyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateMatch_WithVsBotFalse_VerifyCalls()
        {
            //Arrange
            var createMatchDto = _fixture.Build<CreateMatchDto>().With(x => x.VsBot, false).Create();
            var matchCreatedDto = _fixture.Create<MatchCreatedDto>();

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object,
                Clients = _clientsMock.Object
            };

            _groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _senderMock.Setup(x => x.Send(It.IsAny<CreateMatchCommand>(), CancellationToken.None)).ReturnsAsync(matchCreatedDto);
            _clientsMock.Setup(clients => clients.Caller).Returns(_clientProxyMock.Object);

            //Act
            await hub.CreateMatch(createMatchDto);

            //Arrange
            _groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
            _senderMock.Verify(x => x.Send(It.IsAny<CreateMatchCommand>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchCreated", new object[] { matchCreatedDto }, CancellationToken.None), Times.Once);
            _clientProxyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateMatch_WithVsBotTrue_VerifyCalls()
        {
            //Arrange
            var createMatchDto = _fixture.Build<CreateMatchDto>().With(x => x.VsBot, true).Create();
            var matchCreatedDto = _fixture.Create<MatchCreatedDto>();
            var matchStartedDto = _fixture.Create<MatchStartedDto>();

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object,
                Clients = _clientsMock.Object
            };

            _groupsMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _senderMock.Setup(x => x.Send(It.IsAny<CreateMatchCommand>(), CancellationToken.None)).ReturnsAsync(matchCreatedDto);
            _senderMock.Setup(x => x.Send(It.IsAny<AcceptMatchCommand>(), CancellationToken.None)).ReturnsAsync(matchStartedDto);
            _clientsMock.Setup(clients => clients.Caller).Returns(_clientProxyMock.Object);

            //Act
            await hub.CreateMatch(createMatchDto);

            //Arrange
            _groupsMock.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
            _senderMock.Verify(x => x.Send(It.IsAny<CreateMatchCommand>(), CancellationToken.None), Times.Once);
            _senderMock.Verify(x => x.Send(It.IsAny<AcceptMatchCommand>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchCreated", new object[] { matchCreatedDto }, CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchStarted", new object[] { matchStartedDto }, CancellationToken.None), Times.Once);
            _clientProxyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CancelMatch_VerifyCalls()
        {
            //Arrange
            var matchId = _fixture.Create<Guid>();

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object,
                Clients = _clientsMock.Object
            };

            _senderMock.Setup(x => x.Send(It.IsAny<CancelMatchCommand>(), CancellationToken.None)).ReturnsAsync(It.IsAny<Unit>());
            _clientsMock.Setup(clients => clients.Group(matchId.ToString())).Returns(_clientProxyMock.Object);

            //Act
            await hub.CancelMatch(matchId);

            //Arrange
            _senderMock.Verify(x => x.Send(It.IsAny<CancelMatchCommand>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchCancelled", new object[] { matchId }, CancellationToken.None), Times.Once);
            _clientProxyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AcceptMatch_VerifyCalls()
        {
            //Arrange
            var matchId = _fixture.Create<Guid>();
            var matchStartedDto = _fixture.Create<MatchStartedDto>();

            var hub = new MatchHub(_senderMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object,
                Clients = _clientsMock.Object
            };

            _senderMock.Setup(x => x.Send(It.IsAny<AcceptMatchCommand>(), CancellationToken.None)).ReturnsAsync(matchStartedDto);
            _clientsMock.Setup(clients => clients.Group(matchId.ToString())).Returns(_clientProxyMock.Object);

            //Act
            await hub.AcceptMatch(matchId);

            //Arrange
            _senderMock.Verify(x => x.Send(It.IsAny<AcceptMatchCommand>(), CancellationToken.None), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync("MatchStarted", new object[] { matchStartedDto }, CancellationToken.None), Times.Once);
            _clientProxyMock.VerifyNoOtherCalls();
        }
    }
}