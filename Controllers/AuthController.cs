using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderSystem.Data;
using RestaurantOrderSystem.Models;
using RestaurantOrderSystem.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestaurantOrderSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtAuthService _jwtAuthService;

        public AuthController(AppDbContext context, IJwtAuthService jwtAuthService)
        {
            _context = context;
            _jwtAuthService = jwtAuthService;
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already taken.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Role = Role.Unassigned
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            var tokenString = _jwtAuthService.GenerateToken(user);

            // no cookies – client must store and send Authorization: Bearer <token>
            return Ok(new
            {
                token = tokenString,
                user_id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role.ToString()
            });
        }

        [HttpPost("logout")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("verify-token")]
        [AllowAnonymous]
        public IActionResult VerifyToken([FromBody] VerifyTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Ok(new { valid = false, error = "Token is empty" });

            try
            {
                var principal = _jwtAuthService.ValidateToken(request.Token, validateLifetime: true);
                if (principal == null)
                {
                    return Ok(new { valid = false });
                }

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(request.Token);

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = principal.Identity?.Name;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    valid = true,
                    user_id = userId,
                    username = username,
                    role = role,
                    expires = jwtToken.ValidTo
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token verification failed: {ex.Message}");
                return Ok(new { valid = false, error = ex.Message });
            }
        }

        //[HttpGet("user-info")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public IActionResult GetUserInfo()
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value;
        //    var email = User.FindFirst(ClaimTypes.Email)?.Value;
        //    var role = User.FindFirst(ClaimTypes.Role)?.Value;

        //    return Ok(new
        //    {
        //        user_id = userId,
        //        username = username,
        //        email = email,
        //        role = role
        //    });
        //}
    }

    public class SignUpRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(3)] public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class VerifyTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
