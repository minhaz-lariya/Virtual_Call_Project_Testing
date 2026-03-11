using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.Model;

namespace Virtual_Call_Project_Testing.ApplicationContext
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }

        public DbSet<AdminMaster> AdminMasters { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<FacultySubject> FacultySubjects { get; set; }
        public DbSet<QuizMaster> QuizMasters { get; set; }
        public DbSet<QuestionMaster> QuestionMasters { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<StudyMaterial> StudyMaterial { get; set; }
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<AssignmentSubmit> AssignmentSubmits { get; set; }
    }
}
