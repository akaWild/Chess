using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetService<DataContext>();

            if (dbContext != null)
                SeedData(dbContext);
        }

        private static void SeedData(DataContext context)
        {
            context.Database.Migrate();

            if (context.Users.Any())
                return;

            var users = new AppUser[]
            {
                new AppUser
                {
                    DisplayName = "John Connor",
                    Email = "john@connor.com",
                    UserName = "john"
                },
                new AppUser
                {
                    DisplayName = "Tom Cruise",
                    Email = "tom@cruise.com",
                    UserName = "tom"
                },
            };

            context.Users.AddRange(users);

            context.SaveChanges();
        }
    }
}
