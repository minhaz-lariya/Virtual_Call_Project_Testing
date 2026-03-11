using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public SubjectController(ApplicationDBContext context)
        {
            _dbContext = context;
        }

        // ================= ADD SUBJECT =================
        [HttpPost("add")]
        public async Task<IActionResult> AddSubject(Subject model)
        {
            // Check for unique subject_code
            var existing = await _dbContext.Subjects
                .FirstOrDefaultAsync(x => x.SubjectCode == model.SubjectCode);

            if (existing != null)
                return Conflict(new { Status = "Fail", Result = "Subject Code already exists" });

            model.CreatedAt = DateTime.Now;

            _dbContext.Subjects.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Subject added successfully" });
        }

        // ================= UPDATE SUBJECT =================
        [HttpPost("update")]
        public async Task<IActionResult> UpdateSubject(Subject model)
        {
            var subject = await _dbContext.Subjects.FindAsync(model.Id);
            if (subject == null)
                return NotFound(new { Status = "Fail", Result = "Subject not found" });

            // Check unique subject code for other records
            if (await _dbContext.Subjects.AnyAsync(x => x.SubjectCode == model.SubjectCode && x.Id != model.Id))
                return Conflict(new { Status = "Fail", Result = "Subject Code already exists" });

            subject.SubjectName = model.SubjectName;
            subject.SubjectCode = model.SubjectCode;
            subject.Semester = model.Semester;
            subject.Credits = model.Credits;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Subject updated successfully" });
        }

        // ================= LIST SUBJECTS =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var subjects = await _dbContext.Subjects.ToListAsync();
            return Ok(new { Status = "OK", Result = subjects });
        }

        [HttpGet("list/unsign")]
        public async Task<IActionResult> Unsign()
        {
            var subjects = await _dbContext.Subjects.Where(o => !(_dbContext.FacultySubjects.Select(k => k.SubjectId).Contains(o.Id))).ToListAsync();
            return Ok(new { Status = "OK", Result = subjects });
        }

        [HttpGet("list/{semester}")]
        public async Task<IActionResult> List(string semester)
        {
            var subjects = await _dbContext.FacultySubjects
            .Select(o=> new
            {
                o.Subject!.SubjectName,
                o.Subject!.SubjectCode,
                o.Subject!.Semester,
                o.Subject!.Id,
                o.Subject!.Credits,
                faculty = new
                {
                    o.Faculty!.Id,
                    o.Faculty!.FullName,
                }
            }).ToListAsync();
            return Ok(new { Status = "OK", Result = subjects });
        }
    }
}