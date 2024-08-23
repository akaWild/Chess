﻿using AutoFixture;
using EventsLib;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features.AcceptDraw;
using MatchService.Models;
using Moq;
using Match = MatchService.Models.Match;

namespace MatchService.UnitTests
{
    public class AcceptDrawHandlerTests : HandlerTestsBase
    {
        [Fact]
        public async Task Handle_WithNoUser_ThrowsUserNotAuthenticated()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), null);

            //Act

            //Assert
            await Assert.ThrowsAsync<UserNotAuthenticated>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNullMatch_ThrowsMatchNotFoundException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync((Match?)null);

            //Act

            //Assert
            await Assert.ThrowsAsync<MatchNotFoundException>(() => sut.Handle(matchCommand, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNotParticipant_ThrowsMatchDrawAcceptException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Create<Match>();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawAcceptException>(exception);
            Assert.Matches("only by match participant", exception.Message);
        }

        [Fact]
        public async Task Handle_WithNotInProgressMatchStatus_ThrowsMatchDrawAcceptException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var matchStatus = Fixture.Create<Generator<MatchStatus>>().First(s => s != MatchStatus.InProgress);
            var match = Fixture.Build<Match>()
                .With(x => x.Status, matchStatus)
                .With(x => x.Creator, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawAcceptException>(exception);
            Assert.Matches("only on active match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedSideNotNull_ThrowsMatchDrawAcceptException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.DrawRequestedSide, (MatchSide?)null)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawAcceptException>(exception);
            Assert.Matches("there wasn't previous request", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByIdleSideAsWhite_ThrowsMatchDrawAcceptException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.White)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawAcceptException>(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithDrawRequestedByIdleSideAsBlack_ThrowsMatchDrawAcceptException()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(It.IsAny<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.Black)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            Exception? exception = await Record.ExceptionAsync(() => sut.Handle(matchCommand, CancellationToken.None));

            //Assert
            Assert.NotNull(exception);
            Assert.IsType<DrawAcceptException>(exception);
            Assert.Matches("only by active side of the match", exception.Message);
        }

        [Fact]
        public async Task Handle_WithValidInput_ShouldReturnCorrectDto()
        {
            //Arrange
            var sut = new AcceptDrawHandler(MatchRepositoryMock.Object, PublishEndpoint.Object, MapperMock.Object);
            var matchCommand = new AcceptDrawCommand(Fixture.Create<Guid>(), Fixture.Create<string>());
            var match = Fixture.Build<Match>()
                .With(x => x.MatchId, matchCommand.MatchId)
                .With(x => x.Status, MatchStatus.InProgress)
                .With(x => x.Creator, matchCommand.User)
                .With(x => x.ActingSide, MatchSide.White)
                .With(x => x.WhiteSidePlayer, matchCommand.User)
                .Create();

            MatchRepositoryMock.Setup(x => x.GetMatchById(matchCommand.MatchId)).ReturnsAsync(match);

            //Act
            await sut.Handle(matchCommand, CancellationToken.None);

            //Assert
            Assert.Equal(match.MatchId, matchCommand.MatchId);
            Assert.NotNull(match.EndedAtUtc);
            Assert.InRange(match.EndedAtUtc.Value, DateTime.UtcNow - TimeSpan.FromSeconds(1), DateTime.UtcNow);
            Assert.Null(match.Winner);
            Assert.Null(match.WinBy);
            Assert.NotNull(match.DrawBy);
            Assert.Equal(DrawDescriptor.Agreement, match.DrawBy);

            MatchRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinished>(It.IsAny<Match>()), Times.Once);
            MapperMock.Verify(x => x.Map<MatchFinishedDto>(It.IsAny<Match>()), Times.Once);
            PublishEndpoint.Verify(x => x.Publish(It.IsAny<MatchFinished>(), CancellationToken.None), Times.Once);
        }
    }
}
