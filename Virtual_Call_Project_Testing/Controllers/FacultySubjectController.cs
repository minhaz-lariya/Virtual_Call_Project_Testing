using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultySubjectController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public FacultySubjectController(ApplicationDBContext context)
        {
            _dbContext = context;
        }


        // Add this to FacultySubjectController.cs
        [HttpPost("bulkAssign")]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAllocationDto dto)
        {
            if (dto.SubjectIds == null || !dto.SubjectIds.Any())
                return BadRequest(new { Status = "Fail", Result = "No subjects selected" });

            // 1. Remove existing assignments for this faculty (Optional: if you want a clean sync)
            // var existing = _dbContext.FacultySubjects.Where(x => x.FacultyId == dto.FacultyId);
            // _dbContext.FacultySubjects.RemoveRange(existing);

            // 2. Add new assignments
            foreach (var subId in dto.SubjectIds)
            {
                // Check if this specific pair already exists to avoid duplicates
                bool exists = await _dbContext.FacultySubjects
                    .AnyAsync(x => x.FacultyId == dto.FacultyId && x.SubjectId == subId);

                if (!exists)
                {
                    _dbContext.FacultySubjects.Add(new FacultySubject
                    {
                        FacultyId = dto.FacultyId,
                        SubjectId = subId,
                        AssignedDate = DateTime.Now
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { Status = "OK", Result = "Workload updated successfully" });
        }

        // Data Transfer Object
        public class BulkAllocationDto
        {
            public int FacultyId { get; set; }
            public List<int> SubjectIds { get; set; }
        }


        // ================= ASSIGN SUBJECT =================
        [HttpPost("assign")]
        public async Task<IActionResult> AssignSubject(FacultySubject model)
        {
            // Check faculty exists
            if (!await _dbContext.Faculties.AnyAsync(f => f.Id == model.FacultyId))
                return NotFound(new { Status = "Fail", Result = "Faculty not found" });

            // Check subject exists
            if (!await _dbContext.Subjects.AnyAsync(s => s.Id == model.SubjectId))
                return NotFound(new { Status = "Fail", Result = "Subject not found" });

            // Check if already assigned
            if (await _dbContext.FacultySubjects.AnyAsync(x => x.FacultyId == model.FacultyId && x.SubjectId == model.SubjectId))
                return Conflict(new { Status = "Fail", Result = "Subject already assigned to this faculty" });

            model.AssignedDate = DateTime.Now;

            _dbContext.FacultySubjects.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Subject assigned successfully" });
        }

        // ================= LIST ASSIGNMENTS =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.FacultySubjects
                .Include(f => f.Faculty)
                .Include(s => s.Subject)
                .Select(x => new
                {
                    x.Id,
                    FacultyName = x.Faculty!.FullName,
                    x.FacultyId,
                    SubjectName = x.Subject!.SubjectName,
                    x.SubjectId,
                    x.AssignedDate
                }).ToListAsync();

            return Ok(new { Status = "OK", Result = data });
        }

        // ================= REMOVE ASSIGNMENT =================
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveAssignment(int fsId)
        {
            var assignment = await _dbContext.FacultySubjects.FindAsync(fsId);
            if (assignment == null)
                return NotFound(new { Status = "Fail", Result = "Assignment not found" });

            _dbContext.FacultySubjects.Remove(assignment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Assignment removed successfully" });
        }
    }
}