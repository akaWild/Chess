using AutoFixture;
using MatchService.Data;
using MatchService.DTOs;
using MatchService.IntegrationTests.Fixtures;
using MatchService.IntegrationTests.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;

namespace MatchService.IntegrationTests
{
    [Collection("Shared collection")]
    public class CreateMatchEndpointTests : IAsyncLifetime
    {
        private readonly string? _tokenWithoutUser;
        private readonly string? _validToken;
        private readonly CustomWebAppFactory _factory;
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly Fixture _fixture;

        private bool _responseReceived = false;
        private MatchCreatedDto? _matchCreatedDto;
        private MatchStartedDto? _matchStartedDto;
        private string? _clientErrorMessage;
        private string? _serverErrorMessage;

        public CreateMatchEndpointTests(CustomWebAppFactory factory)
        {
            _factory = factory;
            _httpMessageHandler = _factory.Server.CreateHandler();

            _fixture = new Fixture();

            var clientWithoutUser = _factory.CreateClient();
            var validClient = _factory.CreateClient();

            var emptyClaims = new Dictionary<string, object>() { };
            var validClaims = new Dictionary<string, object>()
            {
                { ClaimTypes.Name, "Tolian" }
            };

            clientWithoutUser.SetFakeJwtBearerToken(emptyClaims);
            validClient.SetFakeJwtBearerToken(validClaims);

            _tokenWithoutUser = clientWithoutUser.DefaultRequestHeaders.Authorization?.Parameter;
            _validToken = validClient.DefaultRequestHeaders.Authorization?.Parameter;
        }

        [Fact]
        public async Task CreateMatch_WithoutToken()
        {
            //Arrange
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _tokenWithoutUser);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _validToken);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _validToken);

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
            var hubConnection = HubConnectionHelper.GetHubConnection(_httpMessageHandler, token: _validToken);

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

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            _responseReceived = false;
            _matchCreatedDto = default;
            _clientErrorMessage = null;
            _serverErrorMessage = null;

            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            DbHelper.ReinitDbForTests(db);

            return Task.CompletedTask;
        }

        private void SetConnectionHandlers(HubConnection hubConnection)
        {
            hubConnection.On<MatchCreatedDto>("MatchCreated", (mi) =>
            {
                _matchCreatedDto = mi;

                _responseReceived = true;
            });
            hubConnection.On<MatchStartedDto>("MatchStarted", (mi) =>
            {
                _matchStartedDto = mi;

                _responseReceived = true;
            });
            hubConnection.On<string, string>("ClientError", (errMsg, _) =>
            {
                _clientErrorMessage = errMsg;

                _responseReceived = true;
            });
            hubConnection.On<string>("ServerError", (errMsg) =>
            {
                _serverErrorMessage = errMsg;

                _responseReceived = true;
            });
        }

        private Task WaitForResponse(int timeout)
        {
            var startTime = DateTime.Now;
            while (!_responseReceived)
            {
                if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(timeout))
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
