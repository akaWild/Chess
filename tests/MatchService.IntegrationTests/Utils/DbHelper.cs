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
                    MatchId = Guid.Parse("3979B95F-BA5D-4EF7-8405-C9D23BD9609E"),
                    Creator = "Tolian",
                    StartedAtUtc = DateTime.UtcNow,
                    WhiteSidePlayer = "Tolian",
                    ActingSide = MatchSide.Black,
                    Status = MatchStatus.InProgress,
                    Acceptor = "Kolian",
                    Board = "rnbqkbnr/ppp1pppp/8/3p4/4P3/3B4/PPPP1PPP/RNBQK1NR b KQkq - 1 2",
                    History = new []{"e4", "d5", "Bd3"}
                }
            };
        }
    }
}
