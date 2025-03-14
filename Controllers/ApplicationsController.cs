using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobPortalAPI.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ApplicationsController(ApplicationDbContext context) { _context = context; }

        [HttpPost]
        public IActionResult ApplyJob([FromBody] Application application)
        {
            var job = _context.Jobs.Find(application.JobId);
            if (job == null)
                return NotFound("Job not found.");

            var candidate = _context.Users.Find(application.CandidateId);
            if (candidate == null || candidate.Role != "Candidate")
                return BadRequest("Invalid candidate.");

            if (candidate.Role == "Employer")
                return BadRequest("Employers cannot apply for jobs.");

            bool alreadyApplied = _context.Applications.Any(a => a.JobId == application.JobId && a.CandidateId == application.CandidateId);
            if (alreadyApplied)
                return BadRequest("You have already applied for this job.");

            application.Status = "Pending";

            _context.Applications.Add(application);
            _context.SaveChanges();

            return Ok(new { message = "Application submitted successfully!" });
        }

        [HttpGet("job/{jobId}")]
        public IActionResult GetApplications(int jobId)
        {
            var applications = _context.Applications.Where(a => a.JobId == jobId).ToList();
            return Ok(applications);
        }

        [HttpGet("candidate/{candidateId}")]
        public IActionResult GetApplicationsByCandidate(int candidateId)
        {
            var candidate = _context.Users.Find(candidateId);
            if (candidate == null || candidate.Role != "Candidate")
                return BadRequest(new { message = "Invalid candidate Id." });

            var applications = _context.Applications
                .Where(a => a.CandidateId == candidateId)
                .Select(a => new
                {
                    a.Id,
                    a.Status,
                    Job = _context.Jobs
                        .Where(j => j.Id == a.JobId)
                        .Select(j => new { j.Title, j.Company, j.Location })
                        .FirstOrDefault()
                })
                .ToList();

            if (applications.Count == 0)
                return NotFound(new { message = "You have not applied for any jobs yet." });

            return Ok(applications);
        }

        [HttpPatch("{applicationId}/status")]
        public IActionResult UpdateApplicationStatus(int applicationId, [FromBody] UpdateApplicationStatusRequest request)
        {
            var application = _context.Applications.Find(applicationId);
            if (application == null)
                return NotFound("Application not found.");

            if (request.Status != "Accepted" && request.Status != "Rejected")
                return BadRequest("Invalid status. Allowed: Accepted, Rejected");

            application.Status = request.Status;
            _context.SaveChanges();

            return Ok(new { message = $"Application status updated to {request.Status}." });
        }
    }

}
