using UserManagement.Data.Repositories;
using UserManagement.Dtos.UserDto;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GetUserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null) 
            {
                return null;
            }

            return new GetUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicPath = user.ProfilePicPath
            };
        }
    }
}
