using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultyController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public FacultyController(ApplicationDBContext context)
        {
            _dbContext = context;
        }

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
        public async Task<IActionResult> SignUp(Faculty model)
        {
            // Fetch any faculty that matches Email or Phone
            var existingFaculty = await _dbContext.Faculties.FirstOrDefaultAsync(x => x.Email == model.Email || (x.Phone != null && x.Phone == model.Phone));

            if (existingFaculty != null)
            {
                if (existingFaculty.Email == model.Email)
                    return Conflict(new { Status = "Fail", Result = "Email already exists" });

                if (!string.IsNullOrEmpty(model.Phone) && existingFaculty.Phone == model.Phone)
                    return Conflict(new { Status = "Fail", Result = "Phone already exists" });
            }

            model.Password = GeneratePassword();
            model.CreatedAt = DateTime.Now;

            _dbContext.Faculties.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Save Successfully" });
        }

        // ================= SIGN IN =================
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var faculty = await _dbContext.Faculties
                .FirstOrDefaultAsync(x => x.Email == email && x.Password == password);

            if (faculty == null)
                return Ok(new { Status = "Fail", Result = "Invalid Email or Password" });

            return Ok(new { Status = "OK", Result = faculty });
        }

        // ================= CHANGE PASSWORD =================
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var faculty = await _dbContext.Faculties.FindAsync(id);

            if (faculty == null || faculty.Password != oldPassword)
                return Ok(new { Status = "Fail", Result = "Invalid credentials" });

            faculty.Password = newPassword;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Password Changed Successfully" });
        }

        // ================= CHANGE PROFILE =================
        [HttpPost("changeProfile")]
        public async Task<IActionResult> ChangeProfile(Faculty model)
        {
            var faculty = await _dbContext.Faculties.FindAsync(model.Id);

            if (faculty == null)
                return Ok(new { Status = "Fail", Result = "User Not Found" });

            if (await _dbContext.Faculties
                .AnyAsync(x => (x.Email == model.Email || x.Phone == model.Phone)
                && x.Id != model.Id))
            {
                return Conflict(new { Status = "Fail", Result = "Email or Phone already exists" });
            }

            faculty.FullName = model.FullName;
            faculty.Email = model.Email;
            faculty.Phone = model.Phone;
            faculty.Department = model.Department;
            faculty.Status = model.Status;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Profile Updated Successfully" });
        }

        // ================= FORGOT PASSWORD =================
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var faculty = await _dbContext.Faculties.FirstOrDefaultAsync(x => x.Email == email);

            if (faculty == null)
                return Ok(new { Status = "Fail", Result = "Email Not Found" });

            string newPassword = GeneratePassword();
            faculty.Password = newPassword;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = newPassword });
        }

        // ================= LIST =================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.Faculties.ToListAsync();
            return Ok(new { Status = "OK", Result = data });
        }

        // ================= GET PROFILE =================
        [HttpGet("getProfile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var faculty = await _dbContext.Faculties.FirstOrDefaultAsync(x => x.Id == id);

            if (faculty == null)
                return Ok(new { Status = "Fail", Result = "User Not Found" });

            return Ok(new { Status = "OK", Result = faculty });
        }

    }
}