using UserManagement.Dtos.UserDto;

namespace UserManagement.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginUserDto request);
    }
}
