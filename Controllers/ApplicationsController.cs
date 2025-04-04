using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context) { _context = context; }

        [HttpPost]
        public async Task<IActionResult> ApplyJob([FromBody] Application application)
        {
            var job = await _context.Jobs.FindAsync(application.JobId);
            if (job == null)
                return NotFound(new { message = "Job not found." });

            if (job.ApplicationDeadline < DateTime.UtcNow)
                return BadRequest(new { message = "Application deadline has passed." });

            var candidate = await _context.Users.FindAsync(application.CandidateId);
            if (candidate == null || candidate.Role != UserRole.Candidate)
                return BadRequest(new { message = "Invalid candidate." });

            if (await _context.Applications.AnyAsync(a => a.JobId == application.JobId && a.CandidateId == application.CandidateId))
                return BadRequest(new { message = "You have already applied for this job." });

            application.Status = ApplicationStatus.Pending;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Application submitted successfully!" });
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetApplications(int jobId, [FromQuery] int employerId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                return NotFound(new { message = "Job not found." });

            var employer = await _context.Users.FindAsync(employerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            if (job.EmployerId != employerId)
                return StatusCode(403, new { message = "Only the employer who posted this job can view applications." });

            var applications = await _context.Applications
                .Where(a => a.JobId == jobId)
                .Include(a => a.Candidate)
                .Select(a => new
                {
                    a.Id,
                    a.Status,
                    Candidate = new { a.Candidate.Id, a.Candidate.Username, a.Candidate.Email }
                })
                .ToListAsync();

            if (!applications.Any())
                return BadRequest(new { message = "No applications found." });

            return Ok(applications);
        }

        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetApplications(int candidateId)
        {
            var candidate = await _context.Users.FindAsync(candidateId);
            if (candidate == null || candidate.Role != UserRole.Candidate)
                return BadRequest(new { message = "Invalid candidate." });

            var applications = await _context.Applications
                .Where(a => a.CandidateId == candidateId)
                .Include(a => a.Job)
                .Select(a => new
                {
                    a.Id,
                    a.Status,
                    Job = new { a.Job.Id, a.Job.Title, a.Job.Company, a.Job.Location, a.Job.Description, a.Job.Type, a.Job.ApplicationDeadline }
                })
                .ToListAsync();

            if (!applications.Any())
                return BadRequest(new { message = "No applications found." });

            return Ok(applications);
        }

        [HttpPatch("{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromQuery] int employerId, 
            [FromBody] UpdateApplicationStatusRequest request)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return NotFound(new { message = "Application not found." });

            var employer = await _context.Users.FindAsync(employerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            if (employer.Id != application.Job.EmployerId)
                return BadRequest(new { message = "You can update application status only for your own job postings." });

            application.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Application status updated successfully!." });
        }

    }
}
