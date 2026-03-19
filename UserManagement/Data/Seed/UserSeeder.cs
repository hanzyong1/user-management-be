using Microsoft.AspNetCore.Identity;
using UserManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Data.Seed
{
    public class UserSeeder
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserSeeder(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        private User CreateUser(string firstName, string lastName, string email, string password)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            return user;
        }

        private List<User> GetInitialUsers()
        {
            return new List<User>
            {
                CreateUser("test", "user", "testuser@test.com", "test")
            };
        }

        public async Task Create()
        {
            if (!await _context.Users.AnyAsync())
            {
                await _context.Users.AddRangeAsync(GetInitialUsers());
                await _context.SaveChangesAsync();
            }
        }
    }
}
