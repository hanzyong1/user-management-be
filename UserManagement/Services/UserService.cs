using Azure.Identity;
using Azure.Storage.Blobs;
using UserManagement.Data.Repositories;
using UserManagement.Dtos.UserDto;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, BlobServiceClient blobServiceClient, IConfiguration config)
        {
            _userRepository = userRepository;
            _blobServiceClient = blobServiceClient;
            _config = config;
        }

        public async Task<GetUserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null) 
            {
                return null;
            }

            return MapToDto(user);
        }

        public async Task<GetUserDto?> UpdateUserProfileAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToDto(user);
        }

        public async Task<GetUserDto?> UpdateUserProfilePictureAsync(int id, UpdateUserProfilePicDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            //Upload only if a new file is provided
            if (dto.ProfilePicPath != null)
            {
                var imageUrl = await UploadAsync(dto.ProfilePicPath);
                user.ProfilePicPath = imageUrl;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToDto(user);
        }

        private async Task<string> UploadAsync(IFormFile file)
        {
            var containerName = _config["AzureBlob:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString(); 
        }

        private static GetUserDto MapToDto(User user) => new GetUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ProfilePicPath = user.ProfilePicPath
        };
    }
}
