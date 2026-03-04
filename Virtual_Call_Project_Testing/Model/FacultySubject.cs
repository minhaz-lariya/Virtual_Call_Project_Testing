using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class FacultySubject
    {
        public int Id { get; set; }
        public virtual Faculty? Faculty { get; set; } 
        public int FacultyId { get; set; }
        public virtual Subject? Subject { get; set; }
        public int SubjectId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime createdAt { get; set; } = DateTime.Now;
    }
}