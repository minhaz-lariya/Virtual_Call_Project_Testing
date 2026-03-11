using System;

namespace Virtual_Call_Project_Testing.Model
{
    public class QuestionMaster
    {
        public int Id { get; set; }       
        public int QuizMasterId { get; set; }           
        public virtual QuizMaster? QuizMaster { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}