﻿using MatchService.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchService.Data
{
    public class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            SeedData(scope.ServiceProvider.GetService<DataContext>());
        }

        private static void SeedData(DataContext? context)
        {
            if (context == null)
                return;

            context.Database.Migrate();

            if (context.Matches.Any())
                return;

            var matches = new[]
            {
                new Match
                {
                    MatchId = Guid.Parse("7139D633-66F9-439F-8198-E5E18E9F6848"),
                    Creator = "Tolian"
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

            context.Matches.AddRange(matches);
            context.SaveChanges();
        }
    }
}
