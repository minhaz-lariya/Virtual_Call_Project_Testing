using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;
using System.IO;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmitAssignmentController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public SubmitAssignmentController(ApplicationDBContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        // 1️⃣ Submit Assignment (Student)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAssignment([FromBody] AssignmentSubmit model)
        {
            try
            {
                var alreadySubmitted = await _dbContext.AssignmentSubmits.FirstOrDefaultAsync(x => x.AssignmentsId == model.AssignmentsId && x.StudentId == model.StudentId);

                if (alreadySubmitted != null)
                {
                    return BadRequest(new
                    {
                        status = "ERROR",
                        message = "Assignment already submitted"
                    });
                }

                model.SubmittedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(model.Base64Data))
                {
                    var fileName = $"{Guid.NewGuid()}_{DateTime.Now.Ticks}";
                    var folder = Path.Combine(_environment.WebRootPath, "Content", "AssignmentSubmissions");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string extension = "bin";
                    var base64Parts = model.Base64Data.Split(',');
                    string base64Content = model.Base64Data;

                    if (base64Parts.Length == 2)
                    {
                        base64Content = base64Parts[1];

                        if (base64Parts[0].Contains("pdf")) extension = "pdf";
                        else if (base64Parts[0].Contains("image/jpeg")) extension = "jpg";
                        else if (base64Parts[0].Contains("image/png")) extension = "png";
                        else if (base64Parts[0].Contains("doc")) extension = "doc";
                        else if (base64Parts[0].Contains("docx")) extension = "docx";
                    }

                    var fullFileName = $"{fileName}.{extension}";
                    var fullPath = Path.Combine(folder, fullFileName);

                    var fileBytes = Convert.FromBase64String(base64Content);
                    await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

                    model.FilePath = $"/Content/AssignmentSubmissions/{fullFileName}";
                }

                _dbContext.AssignmentSubmits.Add(model);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "OK",
                    message = "Assignment submitted successfully",
                    result = model
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = "ERROR",
                    message = ex.Message
                });
            }
        }

        // 2️⃣ Get submissions by AssignmentId (Faculty view)
        [HttpGet("by-assignment/{assignmentId}")]
        public async Task<IActionResult> GetSubmissionsByAssignment(int assignmentId)
        {
            var submissions = await _dbContext.AssignmentSubmits
                .Where(x => x.AssignmentsId == assignmentId)
                .Include(x => x.Student)
                .Include(x => x.Assignment)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = submissions
            });
        }

        // 3️⃣ Get submissions by Student
        [HttpGet("by-student/{studentId}")]
        public async Task<IActionResult> GetSubmissionsByStudent(int studentId)
        {
            var submissions = await _dbContext.AssignmentSubmits
                .Where(x => x.StudentId == studentId)
                .Include(x => x.Assignment)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = submissions
            });
        }

        // 4️⃣ Give Marks (Faculty)
        [HttpPut("give-marks/{submissionId}")]
        public async Task<IActionResult> GiveMarks(int submissionId, [FromBody] AssignmentSubmit model)
        {
            var submission = await _dbContext.AssignmentSubmits.FindAsync(submissionId);

            if (submission == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Submission not found"
                });
            }

            submission.MarksObtained = model.MarksObtained;
            submission.Remarks = model.Remarks;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Marks updated successfully",
                result = submission
            });
        }

        // 5️⃣ Delete submission
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var submission = await _dbContext.AssignmentSubmits.FindAsync(id);

            if (submission == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Submission not found"
                });
            }

            _dbContext.AssignmentSubmits.Remove(submission);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Submission deleted successfully"
            });
        }


        // 6️⃣ Get student submissions by Semester
        [HttpGet("by-student-semester/{studentId}/{semester}")]
        public async Task<IActionResult> GetStudentSubmissionsBySemester(int studentId, string semester)
        {
            var submissions = await _dbContext.AssignmentSubmits.Include(x => x.Assignment).ThenInclude(a => a!.Subject).Include(x => x.Student)
            .Where(x => x.StudentId == studentId && x.Assignment!.Subject!.Semester == semester).OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();

            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "No submissions found for this semester"
                });
            }

            var result = submissions.Select(s => new
            {
                s.Id,
                AssignmentId = s.Id,
                AssignmentTitle = s.Assignment!.Title,
                Subject = s.Assignment!.Subject!.SubjectName,
                Semester = s.Assignment!.Subject!.Semester,
                s.FilePath,
                s.MarksObtained,
                s.Remarks,
                s.SubmittedAt
            });

            return Ok(new
            {
                status = "OK",
                result = result
            });
        }


        // 7️⃣ Get submissions by AssignmentId
        [HttpGet("by-assignments/{AssignmentId}")]
        public async Task<IActionResult> GetSubmissionsBySubject(int AssignmentId)
        {
            var submissions = await _dbContext.AssignmentSubmits
                .Include(x => x.Assignment)
                .ThenInclude(a => a!.Subject)
                .Include(x => x.Student)
                .Where(x => x.AssignmentsId == AssignmentId)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "No submissions found for this subject"
                });
            }

            var result = submissions.Select(s => new
            {
                SubmissionId = s.Id,
                AssignmentId = s.AssignmentsId,
                AssignmentTitle = s.Assignment!.Title,
                SubjectId = s.Assignment.SubjectId,
                SubjectName = s.Assignment.Subject!.SubjectName,
                StudentId = s.StudentId,
                StudentName = s.Student!.FullName,
                s.FilePath,
                s.MarksObtained,
                s.Remarks,
                s.SubmittedAt
            });

            return Ok(new
            {
                status = "OK",
                result = result
            });
        }


        // 8️⃣ Get all submitted assignments ordered by submission date
        [HttpGet("all/{facultyid}")]
        public async Task<IActionResult> GetAllSubmissions(int facultyid)
        {
            var submissions = await _dbContext.AssignmentSubmits
                .Where(o=> _dbContext.FacultySubjects.Where(o=> o.FacultyId == facultyid).Select(S=> S.SubjectId).Contains(o.Assignment!.SubjectId))
                .Include(x => x.Student)
                .Include(x => x.Assignment)
                .ThenInclude(a => a!.Subject)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            if (submissions == null || submissions.Count == 0)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "No submissions found"
                });
            }

            var result = submissions.Select(s => new
            {
                SubmissionId = s.Id,
                AssignmentId = s.AssignmentsId,
                AssignmentTitle = s.Assignment!.Title,
                SubjectId = s.Assignment.SubjectId,
                SubjectName = s.Assignment.Subject!.SubjectName,
                StudentId = s.StudentId,
                StudentName = s.Student!.FullName,
                StudentClass = s.Student!.ClassName,
                StudentSemester = s.Student!.Semester,
                s.FilePath,
                s.MarksObtained,
                s.Remarks,
                s.SubmittedAt
            });

            return Ok(new
            {
                status = "OK",
                result = result
            });
        }


    }
}