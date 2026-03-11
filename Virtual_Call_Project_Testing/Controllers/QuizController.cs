using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public QuizController(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        // ================= CREATE QUIZ =================
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz(QuizMaster model)
        {
            if (!await _dbContext.Subjects.AnyAsync(x => x.Id == model.SubjectId))
                return NotFound(new { Status = "Fail", Result = "Subject not found" });

            if (!await _dbContext.Faculties.AnyAsync(x => x.Id == model.FacultyId))
                return NotFound(new { Status = "Fail", Result = "Faculty not found" });

            model.CreatedAt = DateTime.Now;

            _dbContext.QuizMasters.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Quiz created successfully" });
        }

        // ================= LIST QUIZZES =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.QuizMasters
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .Select(x => new
                {
                    x.Id,
                    x.QuizTitle,
                    x.QuizDescription,
                    x.QuizStart,
                    x.QuizEnd,
                    x.TotalQuestions,
                    x.SubjectId,
                    SubjectName = x.Subject!.SubjectName,
                    x.FacultyId,
                    FacultyName = x.Faculty!.FullName,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .ToListAsync();

            return Ok(new { Status = "OK", Result = data });
        }

        // ================= GET QUIZ BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var quiz = await _dbContext.QuizMasters
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.QuizTitle,
                    x.QuizDescription,
                    x.QuizStart,
                    x.QuizEnd,
                    x.TotalQuestions,
                    x.SubjectId,
                    SubjectName = x.Subject!.SubjectName,
                    x.FacultyId,
                    FacultyName = x.Faculty!.FullName,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .FirstOrDefaultAsync();

            if (quiz == null)
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            return Ok(new { Status = "OK", Result = quiz });
        }

        // ================= UPDATE QUIZ =================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, QuizMaster model)
        {
            var quiz = await _dbContext.QuizMasters.FindAsync(id);

            if (quiz == null)
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            quiz.QuizTitle = model.QuizTitle;
            quiz.QuizDescription = model.QuizDescription;
            quiz.SubjectId = model.SubjectId;
            quiz.FacultyId = model.FacultyId;
            quiz.QuizStart = model.QuizStart;
            quiz.QuizEnd = model.QuizEnd;
            quiz.TotalQuestions = model.TotalQuestions;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Quiz updated successfully" });
        }

        // ================= DELETE QUIZ =================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _dbContext.QuizMasters.FindAsync(id);

            if (quiz == null)
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            _dbContext.QuizMasters.Remove(quiz);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Quiz deleted successfully" });
        }

        // ================= QUIZ BY SUBJECT =================
        [HttpGet("bySubject/{subjectId}")]
        public async Task<IActionResult> GetQuizBySubject(int subjectId)
        {
            var data = await _dbContext.QuizMasters
                .Where(q => q.SubjectId == subjectId)
                .Include(q => q.Subject)
                .Include(q => q.Faculty)
                .Select(x => new
                {
                    x.Id,
                    x.QuizTitle,
                    x.QuizDescription,
                    x.QuizStart,
                    x.QuizEnd,
                    x.TotalQuestions,
                    x.SubjectId,
                    SubjectName = x.Subject!.SubjectName,
                    x.FacultyId,
                    FacultyName = x.Faculty!.FullName,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .ToListAsync();

            if (data == null || !data.Any())
                return NotFound(new { Status = "Fail", Result = "No quizzes found for this subject" });

            return Ok(new { Status = "OK", Result = data });
        }


        // ================= UPCOMING QUIZZES =================
        [HttpGet("upcoming/{semester}/{studentId}")]
        public async Task<IActionResult> GetUpcomingQuizzes(string semester, int StudentId)
        {
            var now = DateTime.Now;
            var data = await _dbContext.QuizMasters
                .Where(q => q.QuizEnd > now && q.Subject!.Semester == semester && !_dbContext.QuizAttempts.Where(o=> o.StudentId == StudentId).Select(o=> o.QuizMasterId).Contains(q.Id))
                .Include(q => q.Subject)
                .Include(q => q.Faculty)
                .Select(x => new
                {
                    x.Id,
                    x.QuizTitle,
                    x.QuizDescription,
                    x.QuizStart,
                    x.QuizEnd,
                    x.TotalQuestions,
                    x.SubjectId,
                    SubjectName = x.Subject!.SubjectName,
                    x.FacultyId,
                    FacultyName = x.Faculty!.FullName,
                    x.CreatedAt,
                    x.CreatedBy
                }).OrderBy(x => x.QuizStart)
                .ToListAsync();

            if (!data.Any())
                return Ok(new { Status = "Fail", Result = "No upcoming quizzes found" });

            return Ok(new { Status = "OK", Result = data });
        }

    }
}