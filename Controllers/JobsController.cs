using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(jobs);
        }

        [HttpPost]
        public IActionResult PostJob([FromBody] Job job)
        {
            if (string.IsNullOrEmpty(job.Title) || string.IsNullOrEmpty(job.Company) || string.IsNullOrEmpty(job.Location))
                return BadRequest(new { message = "Title, Company, and Location are required fields." });

            if (job.EmployerId <= 0)
                return BadRequest(new { message = "EmployerId is required." });

            var employer = _context.Users.Find(job.EmployerId);
            if (employer == null || employer.Role != "Employer")
                return BadRequest(new { message = "Invalid EmployerId. Only employers can post jobs." });

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return Ok(new { message = "Job posted successfully!" });
        }
    }
}
