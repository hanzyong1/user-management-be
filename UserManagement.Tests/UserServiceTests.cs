using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using UserManagement.Data.Repositories;
using UserManagement.Dtos.UserDto;
using UserManagement.Models;
using UserManagement.Services;
using Xunit;

namespace UserManagement.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsDto_WhenUserExists()
        {
            var mockRepo = new Mock<IUserRepository>();
            var user = new User { Id = 10, FirstName = "test", LastName = "user", Email = "test@test.com", ProfilePicPath = "http://img" };
            mockRepo.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            var config = new ConfigurationBuilder().Build();

            var service = new UserService(mockRepo.Object, null!, config);

            var result = await service.GetUserByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.ProfilePicPath, result.ProfilePicPath);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var config = new ConfigurationBuilder().Build();

            var service = new UserService(mockRepo.Object, null!, config);

            var result = await service.GetUserByIdAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UpdatesAndReturnsDto()
        {
            var mockRepo = new Mock<IUserRepository>();
            var user = new User { Id = 5, FirstName = "Old", LastName = "Name", Email = "u@test.com", UpdatedAt = DateTime.UtcNow.AddHours(-1) };
            mockRepo.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var config = new ConfigurationBuilder().Build();

            var service = new UserService(mockRepo.Object, null!, config);

            var dto = new UpdateUserDto { FirstName = "New", LastName = "User" };

            var result = await service.UpdateUserProfileAsync(user.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(dto.FirstName, result!.FirstName);
            Assert.Equal(dto.LastName, result.LastName);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.FirstName == dto.FirstName && u.LastName == dto.LastName)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfilePictureAsync_NoUpload_WhenNoFileProvided()
        {
            var mockRepo = new Mock<IUserRepository>();
            var user = new User { Id = 7, FirstName = "Pic", LastName = "User", Email = "pic@test.com", ProfilePicPath = "http://existing" };
            mockRepo.Setup(r => r.GetUserByIdAsync(user.Id)).ReturnsAsync(user);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var config = new ConfigurationBuilder().Build();

            var service = new UserService(mockRepo.Object, null!, config);

            var dto = new UpdateUserProfilePicDto { ProfilePicPath = null };

            var result = await service.UpdateUserProfilePictureAsync(user.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(user.ProfilePicPath, result!.ProfilePicPath);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.ProfilePicPath == user.ProfilePicPath)), Times.Once);
        }
    }
}
