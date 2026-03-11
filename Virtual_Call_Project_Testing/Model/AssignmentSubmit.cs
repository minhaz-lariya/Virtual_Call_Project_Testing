using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class AssignmentSubmit
    {
        public int Id { get; set; }
        public int AssignmentsId { get; set; }
        public int StudentId { get; set; }
        public string? FilePath { get; set; }
        [NotMapped]
        public string? Base64Data { get; set; } // for upload
        public string? Remarks { get; set; }
        public int? MarksObtained { get; set; }
        public DateTime SubmittedAt { get; set; }
        public virtual Assignments? Assignment { get; set; }
        public virtual Student? Student { get; set; }
        public DateTime uploadedAt { get; set; }
    }
}