using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;
using System.Text;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public AdminMasterController(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        // ================== Helper: Generate Random Password ==================
        private string GeneratePassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
                sb.Append(chars[random.Next(chars.Length)]);

            return sb.ToString();
        }

        // ================== SIGN UP ==================
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(AdminMaster model)
        {
            if (await _dbContext.AdminMasters
                .AnyAsync(x => x.Email == model.Email || x.Phone == model.Phone))
            {
                return Conflict(new { Status = "Fail", Result = "Email or Phone already exists" });
            }

            model.Password = GeneratePassword();
            model.CreatedAt = DateTime.Now;

            _dbContext.AdminMasters.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Status = "OK",
                Result = "Save Successfully"
            });
        }

        // ================== SIGN IN ==================
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var admin = await _dbContext.AdminMasters
                .FirstOrDefaultAsync(x => x.Email == email && x.Password == password);

            if (admin == null)
            {
                return Ok(new { Status = "Fail", Result = "Invalid Email or Password" });
            }

            return Ok(new { Status = "OK", Result = admin });
        }

        // ================== CHANGE PASSWORD ==================
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(int id, string oldPassword, string newPassword)
        {
            var admin = await _dbContext.AdminMasters.FindAsync(id);

            if (admin == null || admin.Password != oldPassword)
            {
                return Ok(new { Status = "Fail", Result = "Invalid credentials" });
            }

            admin.Password = newPassword;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Password Changed Successfully" });
        }

        // ================== CHANGE PROFILE ==================
        [HttpPost("changeProfile")]
        public async Task<IActionResult> ChangeProfile(AdminMaster model)
        {
            var admin = await _dbContext.AdminMasters.FindAsync(model.Id);

            if (admin == null)
                return Ok(new { Status = "Fail", Result = "User Not Found" });

            if (await _dbContext.AdminMasters
                .AnyAsync(x => (x.Email == model.Email || x.Phone == model.Phone) && x.Id != model.Id))
            {
                return Conflict(new { Status = "Fail", Result = "Email or Phone already exists" });
            }

            admin.FullName = model.FullName;
            admin.Email = model.Email;
            admin.Phone = model.Phone;
            admin.Status = model.Status;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = "Profile Updated Successfully" });
        }

        // ================== FORGOT PASSWORD ==================
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var admin = await _dbContext.AdminMasters
                .FirstOrDefaultAsync(x => x.Email == email);

            if (admin == null)
                return Ok(new { Status = "Fail", Result = "Email Not Found" });

            string newPassword = GeneratePassword();
            admin.Password = newPassword;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Status = "OK", Result = newPassword });
        }

        // ================== LIST ==================
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var data = await _dbContext.AdminMasters.ToListAsync();

            return Ok(new { Status = "OK", Result = data });
        }

        [HttpGet("getProfile")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var admin = await _dbContext.AdminMasters.FindAsync(id);

            if (admin == null)
            {
                return Ok(new { Status = "Fail", Result = "User Not Found" });
            }

            // Returning the full admin object to match your Profile UI needs
            return Ok(new { Status = "OK", Result = admin });
        }

    }
}