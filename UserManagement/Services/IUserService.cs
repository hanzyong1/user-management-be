using UserManagement.Dtos.UserDto;
using UserManagement.Models;

namespace UserManagement.Services
{
    public interface IUserService
    {
        Task<GetUserDto?> GetUserByIdAsync(int id);
        Task<GetUserDto?> UpdateUserProfileAsync(int id, UpdateUserDto dto);
        Task<GetUserDto?> UpdateUserProfilePictureAsync(int id, UpdateUserProfilePicDto dto);
    }
}
