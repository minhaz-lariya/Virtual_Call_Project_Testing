using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class Subject
    {
        public int Id { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int Credits { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}