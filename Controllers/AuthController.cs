using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!IsValidEmail(request.Email))
                return BadRequest(new { message = "Invalid email format." });

            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "Email is already registered." });

            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest(new { message = "Username is already taken." });

            if (!IsValidPassword(request.Password))
                return BadRequest(new { message = "Weak password. Use at least 8 characters, an uppercase letter, a number, and a special character." });

            string salt = GenerateSalt();

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                HashedPassword = HashPassword(request.Password, salt),
                Salt = salt,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);
            if (user == null || user.HashedPassword != HashPassword(request.Password, user.Salt))
                return Unauthorized(new { message = "Invalid username or password." });

            var token = GenerateJwtToken(user);

            return Ok(new { message = "Login successful!", token });
        }

        [HttpPost("request-password-reset")]
        public IActionResult RequestPasswordReset([FromBody] ResetPasswordRequest request)
        {
            if (!IsValidEmail(request.Email))
                return BadRequest(new { message = "Invalid email format." });

            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            string resetToken = GenerateResetToken();

            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            _context.SaveChanges();

            return Ok(new { message = "Password reset token sent to email.", resetToken });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPassword request)
        {
            var user = _context.Users.SingleOrDefault(u => u.ResetToken == request.Token && u.Email == request.Email);
            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired reset token." });

            if (!IsValidPassword(request.NewPassword))
                return BadRequest(new { message = "Weak password. Use at least 8 characters, an uppercase letter, a number, and a special character." });

            if (VerifyPassword(request.NewPassword, user.HashedPassword, user.Salt))
                return BadRequest(new { message = "New password cannot be the same as the old password." });

            string salt = GenerateSalt();

            user.HashedPassword = HashPassword(request.NewPassword, salt);
            user.Salt = salt;
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            return Ok(new { message = "Password reset successful!" });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(deriveBytes.GetBytes(32));
            }
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,16}$");
        }

        private string GenerateResetToken()
        {
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            string hashedInput = HashPassword(password, salt);
            return hashedInput == hashedPassword;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }


    }
}
