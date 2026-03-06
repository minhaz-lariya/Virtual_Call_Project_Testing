using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public StudentController(ApplicationDBContext context)
        {
            _dbContext = context;
        }

        // ================= PASSWORD GENERATOR =================
        private string GeneratePassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
                sb.Append(chars[random.Next(chars.Length)]);

            return sb.ToString();
        }

        // ================= SIGN UP =================
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(Student model)
        {
            var existingStudent = await _dbContext.Students.FirstOrDefaultAsync(x => x.Email == model.Email || (x.Phone != null && x.Phone == model.Phone) || x.EnrollmentNo == model.EnrollmentNo);

            if (existingStudent != null)
            {
                if (existingStudent.Email == model.Email)
                    return Conflict(new { Status = "Fail", Result = "Email already exists" });

                if (!string.IsNullOrEmpty(model.Phone) && existingStudent.Phone == model.Phone)
                    return Conflict(new { Status = "Fail", Result = "Phone already exists" });

                if (existingStudent.EnrollmentNo == model.EnrollmentNo)
                    return Conflict(new { Status = "Fail", Result = "Enrollment Number already exists" });
            }

            // Auto-generate password
            model.Password = GeneratePassword();
            model.CreatedAt = DateTime.Now;

            _dbContext.Students.Add(model);
            await _dbContext.SaveChangesAsync();

            // Return same structured response
            return Ok(new
            {
                Status = "OK",
                Result = "Save Successfully"
            });
        }
        // ================= SIGN IN =================
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);

            if (student == null)
                return Ok(new { Status = "Fail", Result = "Invalid Email or Password" });

            return Ok(new { Status = "OK", Result = student });
        }

        // ================= CHANGE PASSWORD =================
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var student = await _dbContext.Students.FindAsync(id);

            if (student == null || student.Password != oldPassword)
                return Ok(new { Status = "Fail", Result = "Invalid credentials" });

            student.Password = newPassword;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Password Changed Successfully" });
        }

        // ================= CHANGE PROFILE =================
        [HttpPost("changeProfile")]
        public async Task<IActionResult> ChangeProfile(Student model)
        {
            var student = await _dbContext.Students.FindAsync(model.Id);

            if (student == null)
                return Ok(new { Status = "Fail", Result = "User Not Found" });

            if (await _dbContext.Students.AnyAsync(x => (x.Email == model.Email || x.Phone == model.Phone || x.EnrollmentNo == model.EnrollmentNo) && x.Id != model.Id))
            {
                return Conflict(new { Status = "Fail", Result = "Duplicate data exists" });
            }

            student.FullName = model.FullName;
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.EnrollmentNo = model.EnrollmentNo;
            student.ClassName = model.ClassName;
            student.Division = model.Division;
            student.Semester = model.Semester;
            student.Status = model.Status;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Profile Updated Successfully" });
        }

        // ================= FORGOT PASSWORD =================
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(x => x.Email == email);

            if (student == null)
                return Ok(new { Status = "Fail", Result = "Email Not Found" });

            string newPassword = GeneratePassword();
            student.Password = newPassword;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = newPassword });
        }

        // ================= LIST =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.Students.ToListAsync();
            return Ok(new { Status = "OK", Result = data });
        }

        [HttpGet("list/{semester}")]
        public async Task<IActionResult> List(string semester)
        {
            var data = await _dbContext.Students.Where(o=> o.Semester == semester).ToListAsync();
            return Ok(new { Status = "OK", Result = data });
        }

        // ================= GET PROFILE =================
        [HttpGet("getProfile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var student = await _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id);

            if (student == null)
                return Ok(new { Status = "Fail", Result = "User Not Found" });

            return Ok(new { Status = "OK", Result = student });
        }

    }
}