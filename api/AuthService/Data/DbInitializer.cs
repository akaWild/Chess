using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetService<DataContext>();
            var userManager = scope.ServiceProvider.GetService<UserManager<AppUser>>();

            if (dbContext != null && userManager != null)
                await SeedData(dbContext, userManager);
        }

        private static async Task SeedData(DataContext context, UserManager<AppUser> userManager)
        {
            await context.Database.MigrateAsync();

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

            foreach (var user in users)
                await userManager.CreateAsync(user, "Password123");
        }
    }
}
