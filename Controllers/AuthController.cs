using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobPortalAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Username is already taken. Please choose a different one." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                                             _config["Jwt:Issuer"],
                                             claims,
                                             expires: DateTime.Now.AddHours(1),
                                             signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
