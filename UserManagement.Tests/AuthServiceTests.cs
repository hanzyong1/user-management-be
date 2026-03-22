using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using UserManagement.Services;
using UserManagement.Data.Repositories;
using UserManagement.Models;
using UserManagement.Dtos.AuthDto;
using Microsoft.Extensions.Configuration;

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
    }
}
