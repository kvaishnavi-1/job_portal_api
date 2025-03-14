namespace JobPortalAPI.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CandidateId { get; set; }
        public string Status { get; set; } // "Pending", "Accepted", "Rejected"
    }

    public class UpdateApplicationStatusRequest
    {
        public string Status { get; set; } // Accepted, Rejected
    }

}
