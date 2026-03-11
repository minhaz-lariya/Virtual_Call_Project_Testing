namespace Virtual_Call_Project_Testing.Model
{
    public class QuizMaster
    {
        public int Id { get; set; }

        public virtual Subject? Subject { get; set; }
        public int SubjectId { get; set; }

        public virtual Faculty? Faculty { get; set; }
        public int FacultyId { get; set; }

        public string QuizTitle { get; set; } = string.Empty;

        public string? QuizDescription { get; set; }

        public DateTime QuizStart { get; set; }
        public DateTime QuizEnd { get; set; }
        
        public int TotalQuestions { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }
    }
}