using UserManagement.Models;

namespace UserManagement.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task AddAsync(User user);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}
