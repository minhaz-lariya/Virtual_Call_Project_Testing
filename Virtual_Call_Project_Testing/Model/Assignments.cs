using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class Assignments
    {
        public int Id { get; set; }  // assignment_id
        public virtual Subject? Subject { get; set; }
        public int SubjectId { get; set; }  // SubjectId
        public virtual Faculty? Faculty { get; set; }
        public int FacultyId { get; set; }
        public string Title { get; set; } = string.Empty;  // title\
        public string FilePath { get; set; } = string.Empty;

        [NotMapped]
        public string? base64Data { get; set; } 
        public string? Description { get; set; }  // description
        public int TotalMarks { get; set; }  // total_marks
        public DateTime Deadline { get; set; }  // deadline
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // created_at
    }
}