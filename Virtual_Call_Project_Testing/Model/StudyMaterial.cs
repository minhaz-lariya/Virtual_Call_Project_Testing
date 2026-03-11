using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class StudyMaterial
    {
        public int Id { get; set; }
        public virtual Subject? Subject { get; set; }
        public int SubjectId { get; set; }
        public virtual Faculty? Faculty { get; set; }
        public int FacultyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;

        [NotMapped]
        public string? base64File { get; set; }

        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}