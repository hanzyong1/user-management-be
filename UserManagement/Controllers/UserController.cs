using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Dtos.UserDto;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult> GetUserById()
        {
            //Use ClaimTypes.NameIdentifier to get Id
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            //Use ClaimTypes.NameIdentifier to get Id
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = await _userService.UpdateUserProfileAsync(userId, dto);

            if (user == null)
            { 
                return NotFound(); 
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPost("profile-picture")]
        public async Task<ActionResult> UpdateProfilePicture([FromForm] UpdateUserProfilePicDto dto)
        {
            //Use ClaimTypes.NameIdentifier to get Id
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = await _userService.UpdateUserProfilePictureAsync(userId, dto);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
