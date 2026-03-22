using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Data.Repositories;
using UserManagement.Dtos.AuthDto;
using UserManagement.Models;
using UserManagement.Services;
using Xunit;

namespace UserManagement.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsFalse_WhenEmailExists()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User());

            var mockHasher = new Mock<IPasswordHasher<User>>();
            var mockConfig = new Mock<IConfiguration>();

            var service = new AuthService(mockRepo.Object, mockHasher.Object, mockConfig.Object);

            var dto = new RegisterUserDto { FirstName = "test", LastName = "user", Email = "testuser@test.com", Password = "test" };

            var result = await service.RegisterAsync(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsValid()
        {
            var mockRepo = new Mock<IUserRepository>();
            var user = new User { Id = 1, Email = "login@test.com", PasswordHash = "hashed" };
            mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var mockHasher = new Mock<IPasswordHasher<User>>();
            mockHasher.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(PasswordVerificationResult.Success);

            var config = CreateTestConfiguration();

            var service = new AuthService(mockRepo.Object, mockHasher.Object, config); ;

            var dto = new LoginUserDto { Email = "login@test.com", Password = "password" };

            var result = await service.LoginAsync(dto);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.RefreshToken == result.RefreshToken)), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNewTokens_WhenRefreshTokenValid()
        {
            var mockRepo = new Mock<IUserRepository>();
            var existingRefresh = "old_refresh";
            var user = new User { Id = 2, Email = "refresh@test.com", RefreshToken = existingRefresh, RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(30) };
            mockRepo.Setup(r => r.GetUserByRefreshTokenAsync(existingRefresh)).ReturnsAsync(user);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var mockHasher = new Mock<IPasswordHasher<User>>();

            var config = CreateTestConfiguration();

            var service = new AuthService(mockRepo.Object, mockHasher.Object, config);

            var result = await service.RefreshTokenAsync(existingRefresh);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result!.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
            Assert.NotEqual(existingRefresh, result.RefreshToken);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.RefreshToken == result.RefreshToken)), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ClearsRefreshToken_WhenTokenExists()
        {
            var mockRepo = new Mock<IUserRepository>();
            var token = "to_logout";
            var user = new User { Id = 3, Email = "logout@test.com", RefreshToken = token, RefreshTokenExpiry = DateTime.UtcNow.AddDays(1) };
            mockRepo.Setup(r => r.GetUserByRefreshTokenAsync(token)).ReturnsAsync(user);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var mockHasher = new Mock<IPasswordHasher<User>>();
            var mockConfig = new Mock<IConfiguration>();

            var service = new AuthService(mockRepo.Object, mockHasher.Object, mockConfig.Object);

            await service.LogoutAsync(token);

            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.RefreshToken == null && u.RefreshTokenExpiry == null)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsTrue_WhenEmailNotExists()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var mockHasher = new Mock<IPasswordHasher<User>>();
            mockHasher.Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("hashed");

            var mockConfig = new Mock<IConfiguration>();

            var service = new AuthService(mockRepo.Object, mockHasher.Object, mockConfig.Object);

            var dto = new RegisterUserDto { FirstName = "test", LastName = "user", Email = "testuser@test.com", Password = "test" };

            var result = await service.RegisterAsync(dto);

            Assert.True(result);
            mockRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashed")), Times.Once);
        }

        private IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "super_secret_test_key_which_is_long_enough" },
                { "Jwt:Issuer", "test" },
                { "Jwt:Audience", "test" },
                { "Jwt:ExpiresMinutes", "60" },
                { "Jwt:RefreshTokenExpiresDays", "7" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }
    }
}
