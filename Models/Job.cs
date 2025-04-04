using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JobPortalAPI.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobType
    {
        FullTime,
        PartTime,
        Contract,
        Remote,
        Internship
    }

    public class Job
    {
        public int Id { get; set; }

        [Required, ForeignKey("Employer")]
        public int EmployerId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, StringLength(100)]
        public string Company { get; set; }

        [Required, StringLength(100)]
        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public JobType Type { get; set; }

        [JsonIgnore]
        public DateTime PostedAt { get; set; } = DateTime.UtcNow; 

        public DateTime ApplicationDeadline { get; set; }
    }
}
