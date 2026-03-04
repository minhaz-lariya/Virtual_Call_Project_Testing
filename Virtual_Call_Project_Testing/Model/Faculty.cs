using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class Faculty
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public string Status { get; set; } = "Active"; // Active / Inactive
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}