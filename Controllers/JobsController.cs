using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context) { _context = context; }

        [HttpGet]
        public IActionResult GetJobs()
        {
            var jobs = _context.Jobs.ToList();

            if (!jobs.Any())
                return NotFound(new { message = "No jobs found." });

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public IActionResult GetJobById(int id)
        {
            var job = _context.Jobs.Find(id);

            if (job == null)
                return NotFound(new { message = "Job not found." });

            return Ok(job);
        }

        [HttpGet("employer/{employerId}")]
        public async Task<IActionResult> GetJobs(int employerId)
        {
            var employer = await _context.Users.FindAsync(employerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            var jobs = await _context.Jobs
                .Where(j => j.EmployerId == employerId)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Company,
                    j.Location,
                    j.Description,
                    j.Type,
                    j.PostedAt,
                    j.ApplicationDeadline,
                })
                .ToListAsync();

            if (!jobs.Any())
                return NotFound(new { message = "No jobs found." });

            return Ok(jobs);
        }

        [HttpPost]
        public IActionResult PostJob([FromBody] Job job)
        {
            var employer = _context.Users.Find(job.EmployerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            if (job.ApplicationDeadline < DateTime.UtcNow)
                return BadRequest(new { message = "Application deadline must be in the future." });

            job.PostedAt = DateTime.UtcNow; 

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return Ok(new { message = "Job posted successfully!" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateJob(int id, [FromBody] Job updatedJob)
        {
            if (updatedJob == null || updatedJob.Id != id)
                return BadRequest(new { message = "Job not found." });

            var job = _context.Jobs.Find(id);
            if (job == null)
                return NotFound(new { message = "Job not found." });

            var employer = _context.Users.Find(updatedJob.EmployerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            if (job.EmployerId != updatedJob.EmployerId) 
                return Unauthorized(new { message = "You can only update your own job postings." });

            if (updatedJob.ApplicationDeadline < DateTime.UtcNow)
                return BadRequest(new { message = "Application deadline must be in the future." });

            job.Title = updatedJob.Title;
            job.Company = updatedJob.Company;
            job.Location = updatedJob.Location;
            job.Description = updatedJob.Description;
            job.Type = updatedJob.Type;
            job.ApplicationDeadline = updatedJob.ApplicationDeadline;

            _context.SaveChanges();

            return Ok(new { message = "Job updated successfully!" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteJob(int id, [FromQuery] int employerId) 
        {
            var employer = _context.Users.Find(employerId);
            if (employer == null || employer.Role != UserRole.Employer)
                return BadRequest(new { message = "Invalid employer." });

            var job = _context.Jobs.Find(id);
            if (job == null)
                return NotFound(new { message = "Job not found." });

            if (job.EmployerId != employerId) 
                return Unauthorized(new { message = "You can only delete your own job postings." });

            _context.Jobs.Remove(job);
            _context.SaveChanges();

            return Ok(new { message = "Job deleted successfully!" });
        }
    }
}
