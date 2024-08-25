using MatchService.Data;
using MatchService.Models;

namespace MatchService.IntegrationTests.Utils
{
    public static class DbHelper
    {
        public static void InitDbForTests(DataContext db)
        {
            db.Matches.AddRange(GetMatchesForTest());
            db.SaveChanges();
        }

        public static void ReinitDbForTests(DataContext db)
        {
            db.Matches.RemoveRange(db.Matches);
            db.SaveChanges();

            InitDbForTests(db);
        }

        private static Match[] GetMatchesForTest()
        {
            return new[]
            {
                new Match
                {
                    MatchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848"),
                    Creator = "Tolian"
                },
                new Match
                {
                    MatchId = Guid.Parse("34730E34-4D6E-463D-A9BA-8EC26BEBB63F"),
                    Creator = "Tolian",
                    WhiteSidePlayer = "0"
                },
                new Match
                {
                    MatchId = Guid.Parse("03010934-839C-4DF9-BA11-643EC0FB120C"),
                    Creator = "Tolian",
                    WhiteSidePlayer = "1"
                },
                new Match
                {
                    MatchId = Guid.Parse("03ABA126-1ABA-4CA0-A2CF-F7B9255C787D"),
                    Creator = "Tolian",
                    WhiteSidePlayer = "0",
                    TimeLimit = 3600,
                    ExtraTimePerMove = 30
                },
                new Match
                {
                    MatchId = Guid.Parse("38B56259-55C0-4821-AA4F-D83ED7B58FDF"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
                },
                new Match
                {
                    MatchId = Guid.Parse("BD0C1A6B-6BD0-4457-B4BA-74BD3ABD45C3"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                    DrawRequestedSide = MatchSide.Black
                },
                new Match
                {
                    MatchId = Guid.Parse("61711E02-A185-40EF-A0EA-67FE88D72E1D"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    TimeLimit = 3600,
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                    WhiteSideTimeRemaining = 60
                },
                new Match
                {
                    MatchId = Guid.Parse("CB3E242A-86F0-4D76-818E-C79C7C268C5E"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    TimeLimit = 3600,
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                    DrawRequestedSide = MatchSide.Black,
                    WhiteSideTimeRemaining = 60
                },
                new Match
                {
                    MatchId = Guid.Parse("1DA76931-6686-4F74-BB49-32157C6FB67A"),
                    Creator = "Dimon",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Dimon",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
                },
                new Match
                {
                    MatchId = Guid.Parse("5AF2E004-6342-4FD1-B4F6-2E0A093D3025"),
                    AILevel = 20,
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
                },
                new Match
                {
                    MatchId = Guid.Parse("88275D09-CC64-47E4-B433-7BE6B3DB47A1"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.White,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
                    DrawRequestedSide = MatchSide.Black
                },
                new Match
                {
                    MatchId = Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.Black,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/ppp1pppp/8/3p4/4P3/3B4/PPPP1PPP/RNBQK1NR b KQkq - 1 2",
                    History = new []{"e4", "d5", "Bd3"}
                },
                new Match
                {
                    MatchId = Guid.Parse("E9439209-2F4D-417E-8905-C5264756248B"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.Black,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/ppp1pppp/8/3p4/4P3/3B4/PPPP1PPP/RNBQK1NR b KQkq - 1 2",
                    History = new []{"e4", "d5", "Bd3"},
                    DrawRequestedSide = MatchSide.White
                },
                new Match
                {
                    MatchId = Guid.Parse("97B66057-C123-4671-9F14-BE33AA08916F"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow - TimeSpan.FromMinutes(10),
                    LastMoveMadeAtUtc = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.Black,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    TimeLimit = 3600,
                    Board = "rnbqkbnr/ppp1pppp/8/3p4/4P3/3B4/PPPP1PPP/RNBQK1NR b KQkq - 1 2",
                    History = new []{"e4", "d5", "Bd3"},
                    BlackSideTimeRemaining = 60
                }
            };
        }
    }
}
