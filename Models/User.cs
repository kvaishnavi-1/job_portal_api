using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JobPortalAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Candidate,
        Employer
    }

    public class User
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        [Required]
        public string Salt { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }

    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPassword
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
