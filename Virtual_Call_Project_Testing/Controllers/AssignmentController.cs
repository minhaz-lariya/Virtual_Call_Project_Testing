using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;
using System.IO;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public AssignmentsController(ApplicationDBContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        // 1️⃣ Create Assignment with file upload
        [HttpPost("create")]
        public async Task<IActionResult> CreateAssignment([FromBody] Assignments model)
        {
            try
            {
                model.CreatedAt = DateTime.Now;

                // Handle base64 file if provided
                if (!string.IsNullOrEmpty(model.base64Data))
                {
                    var fileName = $"{Guid.NewGuid()}_{DateTime.Now.Ticks}";
                    var filePath = Path.Combine(_environment.WebRootPath, "Content", "Assignments");

                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    // Detect file type from base64 header
                    string extension = "bin"; // default
                    var base64Parts = model.base64Data.Split(',');
                    string base64Content = model.base64Data;
                    if (base64Parts.Length == 2)
                    {
                        base64Content = base64Parts[1];
                        if (base64Parts[0].Contains("pdf")) extension = "pdf";
                        else if (base64Parts[0].Contains("image/jpeg")) extension = "jpg";
                        else if (base64Parts[0].Contains("image/png")) extension = "png";
                        else if (base64Parts[0].Contains("mp4")) extension = "mp4";
                    }

                    var fullFileName = $"{fileName}.{extension}";
                    var fullPath = Path.Combine(filePath, fullFileName);

                    var fileBytes = Convert.FromBase64String(base64Content);
                    await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

                    model.FilePath = $"/Content/Assignments/{fullFileName}";
                }

                _dbContext.Assignments.Add(model);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "OK",
                    message = "Assignment created successfully",
                    result = model
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "ERROR", message = ex.Message });
            }
        }

        // 2️⃣ Get all assignments
        [HttpGet]
        public async Task<IActionResult> GetAllAssignments()
        {
            var assignments = await _dbContext.Assignments
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(new { status = "OK", result = assignments });
        }

        // 3️⃣ Get assignment by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            var assignment = await _dbContext.Assignments
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (assignment == null)
                return NotFound(new { status = "ERROR", message = "Assignment not found" });

            return Ok(new { status = "OK", result = assignment });
        }

        // 4️⃣ Update assignment
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] Assignments model)
        {
            var assignment = await _dbContext.Assignments.FindAsync(id);
            if (assignment == null)
                return NotFound(new { status = "ERROR", message = "Assignment not found" });

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.TotalMarks = model.TotalMarks;
            assignment.Deadline = model.Deadline;
            assignment.SubjectId = model.SubjectId;
            assignment.FacultyId = model.FacultyId;

            // Update file if base64 provided
            if (!string.IsNullOrEmpty(model.base64Data))
            {
                var fileName = $"{Guid.NewGuid()}_{DateTime.Now.Ticks}";
                var filePath = Path.Combine(_environment.WebRootPath, "Content", "Assignments");

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                string extension = "bin";
                var base64Parts = model.base64Data.Split(',');
                string base64Content = model.base64Data;
                if (base64Parts.Length == 2)
                {
                    base64Content = base64Parts[1];
                    if (base64Parts[0].Contains("pdf")) extension = "pdf";
                    else if (base64Parts[0].Contains("image/jpeg")) extension = "jpg";
                    else if (base64Parts[0].Contains("image/png")) extension = "png";
                    else if (base64Parts[0].Contains("mp4")) extension = "mp4";
                }

                var fullFileName = $"{fileName}.{extension}";
                var fullPath = Path.Combine(filePath, fullFileName);

                var fileBytes = Convert.FromBase64String(base64Content);
                await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

                assignment.FilePath = $"/Content/Assignments/{fullFileName}";
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { status = "OK", message = "Assignment updated successfully", result = assignment });
        }

        // 5️⃣ Delete assignment
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _dbContext.Assignments.FindAsync(id);
            if (assignment == null)
                return NotFound(new { status = "ERROR", message = "Assignment not found" });

            _dbContext.Assignments.Remove(assignment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { status = "OK", message = "Assignment deleted successfully" });
        }

        // 6️⃣ Get assignments by Faculty, grouped by Subject
        [HttpGet("by-faculty/{facultyId}")]
        public async Task<IActionResult> GetAssignmentsByFaculty(int facultyId)
        {
            var assignments = await _dbContext.Assignments
                .Where(a => a.FacultyId == facultyId)
                .Include(a => a.Subject)
                .Include(a => a.Faculty)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Optional: group by subject
            var grouped = assignments
                .GroupBy(a => a.Subject != null ? a.Subject.SubjectName : "Unknown Subject")
                .Select(g => new
                {
                    Subject = g.Key,
                    Assignments = g.Select(a => new
                    {
                        a.Id,
                        a.Title,
                        a.Description,
                        a.TotalMarks,
                        a.Deadline,
                        a.FilePath,
                        a.CreatedAt
                    }).ToList()
                });

            return Ok(new
            {
                status = "OK",
                result = grouped
            });
        }


        // 7️⃣ Get assignments by SubjectId
        [HttpGet("by-subject/{subjectId}")]
        public async Task<IActionResult> GetAssignmentsBySubject(int subjectId)
        {
            var assignments = await _dbContext.Assignments
                .Where(a => a.SubjectId == subjectId)
                .Include(a => a.Subject)
                .Include(a => a.Faculty)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (assignments == null || assignments.Count == 0)
            {
                return NotFound(new
                {
                    status = "Fail",
                    result = "No assignments found for this subject"
                });
            }

            return Ok(new
            {
                status = "OK",
                result = assignments.Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Description,
                    a.TotalMarks,
                    a.Deadline,
                    a.FilePath,
                    a.CreatedAt,
                    Subject = a.Subject != null ? a.Subject.SubjectName : null,
                    Faculty = a.Faculty != null ? a.Faculty.FullName : null
                })
            });
        }


        // 8️⃣ Get all assignments by Semester (through Subject)
        [HttpGet("by-semester/{semester}")]
        public async Task<IActionResult> GetAssignmentsBySemester(string semester)
        {
            var assignments = await _dbContext.Assignments
                .Include(a => a.Subject)
                .Include(a => a.Faculty)
                .Where(a => a.Subject!.Semester == semester)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (assignments == null || assignments.Count == 0)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "No assignments found for this semester"
                });
            }

            var result = assignments.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.TotalMarks,
                a.Deadline,
                a.FilePath,
                a.CreatedAt,
                Subject = a.Subject != null ? a.Subject.SubjectName : null,
                Semester = a.Subject != null ? a.Subject.Semester : null,
                Faculty = a.Faculty != null ? a.Faculty.FullName : null
            });

            return Ok(new
            {
                status = "OK",
                result = result
            });
        }

    }
}