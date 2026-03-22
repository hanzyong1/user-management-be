using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Data.Repositories;
using UserManagement.Dtos.AuthDto;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginUserDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return null;

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Failed)
                return null;

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresDays"] ?? "7"));

            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        //public async Task<bool> RegisterAsync(RegisterUserDto dto)
        //{
        //    var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);

        //    if (existingUser != null)
        //        return false;

        //    var user = new User
        //    {
        //        FirstName = dto.FirstName,
        //        LastName = dto.LastName,
        //        Email = dto.Email,
        //    };

        //    user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        //    await _userRepository.AddAsync(user);

        //    return true;
        //}

        private string GenerateRefreshToken()
        {
            // create a secure random token
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return null;

            var accessToken = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken();

            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresDays"] ?? "7"));

            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefresh
            };
        }

        public async Task LogoutAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
                return;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _userRepository.UpdateAsync(user);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "15")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
