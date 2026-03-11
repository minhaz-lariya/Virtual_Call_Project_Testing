using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public QuestionMasterController(ApplicationDBContext context)
        {
            _dbContext = context;
        }

        // ================= CREATE QUESTION =================
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionMaster model)
        {
            if (!await _dbContext.QuizMasters.AnyAsync(q => q.Id == model.QuizMasterId))
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            model.CreatedAt = DateTime.Now;

            _dbContext.QuestionMasters.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Question created successfully" });
        }


        [HttpPost("createBulk")]
        public async Task<IActionResult> CreateMultipleQuestions([FromBody] List<QuestionMaster> questions)
        {
            if (questions == null || !questions.Any())
                return BadRequest(new { Status = "Fail", Result = "No questions provided" });

            // Check if quiz exists
            int quizId = questions.First().QuizMasterId;
            if (!await _dbContext.QuizMasters.AnyAsync(q => q.Id == quizId))
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            // Set CreatedAt for all questions
            foreach (var q in questions)
            {
                q.CreatedAt = DateTime.Now;
            }

            // Add all questions in one go
            await _dbContext.QuestionMasters.AddRangeAsync(questions);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = $"{questions.Count} questions created successfully" });
        }


        // ================= GET ALL QUESTIONS =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.QuestionMasters
                .Include(q => q.QuizMaster)
                .Select(q => new
                {
                    q.Id,
                    q.QuizMasterId,
                    QuizTitle = q.QuizMaster!.QuizTitle,
                    q.QuestionText,
                    q.Option1,
                    q.Option2,
                    q.Option3,
                    q.Option4,
                    q.Answer,
                    q.CreatedAt
                })
                .ToListAsync();

            return Ok(new { Status = "OK", Result = data });
        }

        // ================= GET QUESTIONS BY QUIZ =================
        [HttpGet("byQuiz/{quizId}")]
        public async Task<IActionResult> GetByQuiz(int quizId)
        {
            if (!await _dbContext.QuizMasters.AnyAsync(q => q.Id == quizId))
                return NotFound(new { Status = "Fail", Result = "Quiz not found" });

            var data = await _dbContext.QuestionMasters
                .Where(q => q.QuizMasterId == quizId)
                .Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Option1,
                    q.Option2,
                    q.Option3,
                    q.Option4,
                    q.Answer,
                    q.CreatedAt
                }).ToListAsync();

            var quizDetail = await _dbContext.QuizMasters.Where(q => q.Id == quizId).FirstOrDefaultAsync();

            return Ok(new { Status = "OK", Result = new { QuizDetail = quizDetail, Questions = data } });
        }

        // ================= UPDATE QUESTION =================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] QuestionMaster model)
        {
            var existing = await _dbContext.QuestionMasters.FindAsync(id);
            if (existing == null)
                return NotFound(new { Status = "Fail", Result = "Question not found" });

            existing.QuestionText = model.QuestionText;
            existing.Option1 = model.Option1;
            existing.Option2 = model.Option2;
            existing.Option3 = model.Option3;
            existing.Option4 = model.Option4;
            existing.Answer = model.Answer;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Question updated successfully" });
        }

        // ================= DELETE QUESTION =================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _dbContext.QuestionMasters.FindAsync(id);
            if (question == null)
                return NotFound(new { Status = "Fail", Result = "Question not found" });

            _dbContext.QuestionMasters.Remove(question);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Question deleted successfully" });
        }
    }
}