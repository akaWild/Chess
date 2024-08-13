using MatchService.Models;
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
                    MatchId = Guid.NewGuid(),
                    Creator = "Tolian"
                }
            };

            context.Matches.AddRange(matches);
            context.SaveChanges();
        }
    }
}
