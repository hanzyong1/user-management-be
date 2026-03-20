using UserManagement.Dtos.UserDto;
using UserManagement.Models;

namespace UserManagement.Services
{
    public interface IUserService
    {
        Task<GetUserDto?> GetUserByIdAsync(int id);
    }
}
