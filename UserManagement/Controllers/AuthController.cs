using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.Dtos.AuthDto;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        //Log in and sets a HttpOnly cookie with Jwt
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginUserDto dto)
        {
            var auth = await _authService.LoginAsync(dto);

            if (auth == null)
            {
                return Unauthorized("Invalid credentials");
            }

            Response.Cookies.Append("jwt", auth.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "15"))
            });

            Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresDays"] ?? "7"))
            });

            return Ok(new { message = "Logged in" });
        }

        //Log out and remove the tokens from cookie
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var refresh = Request.Cookies["refreshToken"];

            await _authService.LogoutAsync(refresh);

            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { message = "Logged out" });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh()
        {
            var refresh = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refresh))
                return Unauthorized();

            var auth = await _authService.RefreshTokenAsync(refresh);
            if (auth == null)
                return Unauthorized();

            Response.Cookies.Append("jwt", auth.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "15"))
            });

            Response.Cookies.Append("refreshToken", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresDays"] ?? "7"))
            });

            return Ok(new { message = "Refreshed" });
        }

        //Validates that the user is currently logged in
        [Authorize]
        [HttpGet("me")]
        public ActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            });
        }
        
        //Register new user
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == false)
            {
                return BadRequest(new { message = "Email already taken" });
            }

            return Ok(new { message = "Registered successfully" });
        }
    }
}
