using UserManagement.Dtos.AuthDto;

namespace UserManagement.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginUserDto dto);
        Task<bool> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string? refreshToken);
    }
}
