using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAttemptController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public QuizAttemptController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1️⃣ START QUIZ ATTEMPT
        [HttpPost("start")]
        public async Task<IActionResult> StartQuizAttempt([FromBody] QuizAttempt model)
        {
            try
            {
                // Check if attempt already exists
                var existingAttempt = await _dbContext.QuizAttempts
                    .FirstOrDefaultAsync(x => x.StudentId == model.StudentId && x.QuizMasterId == model.QuizMasterId);

                if (existingAttempt != null)
                {
                    return Ok(new
                    {
                        status = "OK",
                        message = "Attempt already exists",
                        attemptId = existingAttempt.Id
                    });
                }

                model.CreatedAt = DateTime.Now;

                _dbContext.QuizAttempts.Add(model);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "OK",
                    message = "Quiz attempt started",
                    attemptId = model.Id
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
        // 2️⃣ GET ATTEMPT BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttempt(int id)
        {
            var attempt = await _dbContext.QuizAttempts
                .Include(x => x.QuizMaster)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (attempt == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Attempt not found"
                });
            }

            return Ok(new
            {
                status = "OK",
                result = attempt
            });
        }

        // 3️⃣ GET ALL ATTEMPTS BY STUDENT
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttemptsByStudent(int studentId)
        {
            var attempts = await _dbContext.QuizAttempts
                .Where(x => x.StudentId == studentId)
                .Include(x => x.QuizMaster)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = attempts
            });
        }

        // 4️⃣ SUBMIT QUIZ ATTEMPT
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuizAttempt(int attemptId, int answeredQuestions, int score, int warningCount)
        {
            var attempt = await _dbContext.QuizAttempts.FirstOrDefaultAsync(x => x.Id == attemptId);

            if (attempt == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Attempt not found"
                });
            }


            attempt.Warnings = warningCount;
            attempt.AnsweredQuestions = answeredQuestions;
            attempt.Score = score;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Quiz submitted successfully",
                result = attempt
            });
        }

        // 5️⃣ DELETE ATTEMPT (OPTIONAL)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttempt(int id)
        {
            var attempt = await _dbContext.QuizAttempts.FindAsync(id);

            if (attempt == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Attempt not found"
                });
            }

            _dbContext.QuizAttempts.Remove(attempt);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Attempt deleted"
            });
        }

        // 6️⃣ GET ALL ATTEMPTS BY STUDENT ID
        [HttpGet("GetAttemptsByStudentId/{studentId}")]
        public async Task<IActionResult> GetAttemptsByStudentId(int studentId)
        {
            try
            {
                var attempts = await _dbContext.QuizAttempts
                    .Where(x => x.StudentId == studentId)
                    .Include(x => x.QuizMaster!.Subject)
                    .Include(x => x.Student)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                if (attempts == null || attempts.Count == 0)
                {
                    return NotFound(new
                    {
                        status = "ERROR",
                        message = "No attempts found for this student"
                    });
                }

                return Ok(new
                {
                    status = "OK",
                    totalAttempts = attempts.Count,
                    result = attempts
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

        // 7️⃣ Get all students who attempted a specific quiz
        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetStudentsByQuizId(int quizId)
        {
            try
            {
                var attempts = await _dbContext.QuizAttempts
                    .Where(x => x.QuizMasterId == quizId)
                    .Include(x => x.Student)
                    .Include(x => x.QuizMaster)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                if (attempts == null || attempts.Count == 0)
                {
                    return NotFound(new
                    {
                        status = "ERROR",
                        message = "No students attempted this quiz"
                    });
                }

                var quizDetail = await _dbContext.QuizMasters
                    .Include(q => q.Subject)
                    .FirstOrDefaultAsync(q => q.Id == quizId);


                var result = attempts.Select(a => new
                {
                    AttemptId = a.Id,
                    QuizId = a.QuizMasterId,
                    QuizTitle = a.QuizMaster!.QuizTitle,
                    StudentId = a.StudentId,
                    StudentName = a.Student!.FullName,
                    a.AnsweredQuestions,
                    a.Score,
                    a.CreatedAt,
                    a.Warnings
                });

                return Ok(new
                {
                    status = "OK",
                    totalStudents = result.Count(),
                    result = new
                    {
                        result,
                        quizDetail
                    }});
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
    }
}