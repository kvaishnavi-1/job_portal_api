using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JobPortalAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApplicationStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class Application
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        [JsonIgnore]
        public Job? Job { get; set; }

        [Required]
        public int CandidateId { get; set; }
        [JsonIgnore]
        public User? Candidate { get; set; }

        [JsonIgnore]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }

    public class UpdateApplicationStatusRequest
    {
        [Required]
        public ApplicationStatus Status { get; set; }
    }
}
