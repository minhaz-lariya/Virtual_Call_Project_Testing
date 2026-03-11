using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyMaterialController : ControllerBase
    {
        private readonly ApplicationDBContext _dbContext;

        public StudyMaterialController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1️⃣ Upload Study Material
        [HttpPost("upload")]
        public async Task<IActionResult> UploadMaterial([FromBody] StudyMaterial model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.base64File))
                {
                    // Folder Path
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "StudyMaterial");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Remove base64 header
                    var base64Data = model.base64File.Contains(",")
                        ? model.base64File.Substring(model.base64File.IndexOf(",") + 1)
                        : model.base64File;

                    byte[] fileBytes = Convert.FromBase64String(base64Data);

                    // Generate unique file name using GUID
                    string uid = Guid.NewGuid().ToString();

                    string extension = GetFileExtension(model.FileType);

                    string fileName = uid + extension;

                    string fullPath = Path.Combine(folderPath, fileName);

                    await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

                    // Save file path to database
                    model.FilePath = "Content/StudyMaterial/" + fileName;
                }

                model.UploadedAt = DateTime.Now;

                _dbContext.StudyMaterial.Add(model);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    status = "OK",
                    message = "Study material uploaded successfully",
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

        private string GetFileExtension(string fileType)
        {
            switch (fileType.ToLower())
            {
                case "image":
                case "jpg":
                case "jpeg":
                case "png":
                    return ".jpg";

                case "pdf":
                    return ".pdf";

                case "video":
                case "mp4":
                    return ".mp4";

                case "doc":
                case "docx":
                    return ".docx";

                case "ppt":
                case "pptx":
                    return ".pptx";

                default:
                    return ".bin";
            }
        }

        // 2️⃣ Get All Study Materials
        [HttpGet]
        public async Task<IActionResult> GetAllMaterials()
        {
            var materials = await _dbContext.StudyMaterial
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = materials
            });
        }

        // 3️⃣ Get Materials By Subject
        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetMaterialsBySubject(int subjectId)
        {
            var materials = await _dbContext.StudyMaterial
                .Where(x => x.SubjectId == subjectId)
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = materials
            });
        }

        // 4️⃣ Get Materials By Faculty
        [HttpGet("faculty/{facultyId}")]
        public async Task<IActionResult> GetMaterialsByFaculty(int facultyId)
        {
            var materials = await _dbContext.StudyMaterial
                .Where(x => x.FacultyId == facultyId)
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return Ok(new
            {
                status = "OK",
                result = materials
            });
        }

        // 5️⃣ Get Single Material
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterial(int id)
        {
            var material = await _dbContext.StudyMaterial
                .Include(x => x.Subject)
                .Include(x => x.Faculty)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (material == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Material not found"
                });
            }

            return Ok(new
            {
                status = "OK",
                result = material
            });
        }

        // 6️⃣ Update Study Material
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] StudyMaterial model)
        {
            var material = await _dbContext.StudyMaterial.FindAsync(id);

            if (material == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Material not found"
                });
            }

            material.Title = model.Title;
            material.Description = model.Description;
            material.FilePath = model.FilePath;
            material.FileType = model.FileType;
            material.SubjectId = model.SubjectId;

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Material updated successfully",
                result = material
            });
        }

        // 7️⃣ Delete Study Material
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _dbContext.StudyMaterial.FindAsync(id);

            if (material == null)
            {
                return NotFound(new
                {
                    status = "ERROR",
                    message = "Material not found"
                });
            }

            _dbContext.StudyMaterial.Remove(material);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                status = "OK",
                message = "Material deleted successfully"
            });
        }
    }
}