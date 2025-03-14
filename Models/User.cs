namespace JobPortalAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Employer" or "Candidate"
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
