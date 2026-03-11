using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Virtual_Call_Project_Testing.Model
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public virtual QuizMaster? QuizMaster { get; set; }
        public int QuizMasterId { get; set; }
        public virtual Student? Student { get; set; }
        public int StudentId { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int? Score { get; set; }
        public int? Warnings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}