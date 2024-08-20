using AutoFixture;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class CreateMatchEndpointTests : EndpointTestsBase
    {
        private readonly string? _tokenWithoutUser;
        private readonly string? _validToken;
        private readonly Fixture _fixture;

        private MatchCreatedDto? _matchCreatedDto;
        private MatchStartedDto? _matchStartedDto;

        public CreateMatchEndpointTests(CustomWebAppFactory factory) : base(factory)
        {
            _fixture = new Fixture();

            _tokenWithoutUser = TokenHelper.GetAccessToken(factory);
            _validToken = TokenHelper.GetAccessToken(factory, "Tolian");
        }

        [Fact]
        public async Task CreateMatch_WithoutToken()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", _fixture.Create<CreateMatchDto>()));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("user is unauthorized", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithNullRequestDto()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", null));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Match data object can't be null", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithDefaultMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>().Without(p => p.MatchId).Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Match id must be provided", exception.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(28)]
        [InlineData(-5)]
        public async Task CreateMatch_WithInvalidAILevel(int aiLevel)
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>().With(p => p.AILevel, aiLevel).Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("AI level must be in the range", exception.Message);
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(false, 25)]
        [InlineData(null, 20)]
        public async Task CreateMatch_WithInvalidAISettings(bool? vsBot, int? aiLevel)
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, vsBot)
                .With(p => p.AILevel, aiLevel)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("AI settings are inconsistent", exception.Message);
        }

        [Theory]
        [InlineData(179)]
        [InlineData(7201)]
        public async Task CreateMatch_WithInvalidTimeLimit(int timeLimit)
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, timeLimit)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Time limit must be in the range", exception.Message);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(301)]
        public async Task CreateMatch_WithInvalidExtraTime(int extraTime)
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, 5000)
                .With(p => p.ExtraTimePerMove, extraTime)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Extra time per move must be in the range", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithInvalidTimeSettings()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, (int?)null)
                .With(p => p.ExtraTimePerMove, 100)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("Extra time per move value can't be provided together", exception.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task CreateMatch_WithInvalidFirstSideToAct(int firstSideToAct)
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, 5000)
                .With(p => p.ExtraTimePerMove, 50)
                .With(p => p.FirstToActSide, firstSideToAct)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("First side to act value must be", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithoutUser()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _tokenWithoutUser);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, 5000)
                .With(p => p.ExtraTimePerMove, 100)
                .With(p => p.FirstToActSide, 0)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("User is not authenticated", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithExistentMatchId()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _validToken);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.MatchId, Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848"))
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 10)
                .With(p => p.TimeLimit, 5000)
                .With(p => p.ExtraTimePerMove, 100)
                .With(p => p.FirstToActSide, 0)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.NotNull(exception);
            Assert.Matches("already exists", exception.Message);
        }

        [Fact]
        public async Task CreateMatch_WithValidInputVsHuman()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _validToken);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, false)
                .With(p => p.AILevel, (int?)null)
                .With(p => p.TimeLimit, 5000)
                .With(p => p.ExtraTimePerMove, 100)
                .With(p => p.FirstToActSide, 1)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchCreatedDto);
            Assert.Equal(createMatchDto.MatchId, _matchCreatedDto.MatchId);
            Assert.Equal("Tolian", _matchCreatedDto.Creator);
            Assert.InRange(_matchCreatedDto.CreatedAtUtc, DateTime.UtcNow - TimeSpan.FromMilliseconds(1000), DateTime.UtcNow);
            Assert.Null(_matchStartedDto);
        }

        [Fact]
        public async Task CreateMatch_WithValidInputVsBot()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(HttpMessageHandler, token: _validToken);

            SetConnectionHandlers(hubConnection);

            var createMatchDto = _fixture.Build<CreateMatchDto>()
                .With(p => p.VsBot, true)
                .With(p => p.AILevel, 5)
                .With(p => p.TimeLimit, 800)
                .With(p => p.ExtraTimePerMove, 25)
                .With(p => p.FirstToActSide, 0)
                .Create();

            //Act
            await hubConnection.StartAsync();

            Exception? exception = await Record.ExceptionAsync(async () => await hubConnection.InvokeAsync("CreateMatch", createMatchDto));

            //Assert
            Assert.Null(exception);
            Assert.NotNull(_matchCreatedDto);
            Assert.Equal(createMatchDto.MatchId, _matchCreatedDto.MatchId);
            Assert.Equal("Tolian", _matchCreatedDto.Creator);
            Assert.InRange(_matchCreatedDto.CreatedAtUtc, DateTime.UtcNow - TimeSpan.FromMilliseconds(1000), DateTime.UtcNow);
            Assert.NotNull(_matchStartedDto);
        }

        public override Task DisposeAsync()
        {
            _matchCreatedDto = null;
            _matchStartedDto = null;

            return base.DisposeAsync();
        }

        protected override void SetConnectionHandlers(HubConnection hubConnection)
        {
            base.SetConnectionHandlers(hubConnection);

            hubConnection.On<MatchCreatedDto>("MatchCreated", (mi) =>
            {
                _matchCreatedDto = mi;

                ResponseReceived = true;
            });
            hubConnection.On<MatchStartedDto>("MatchStarted", (mi) =>
            {
                _matchStartedDto = mi;

                ResponseReceived = true;
            });
        }
    }
}
