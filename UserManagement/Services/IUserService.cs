using UserManagement.Models;

namespace UserManagement.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
    }
}
