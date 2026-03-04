using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string EnrollmentNo { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";  // Active / Inactive
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}