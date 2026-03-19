using Microsoft.AspNetCore.Identity;
using UserManagement.Models;

namespace UserManagement.Data.Seed
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            context.Database.EnsureCreated();

            await new UserSeeder(context, passwordHasher).Create();
        }
    }
}
