using Microsoft.EntityFrameworkCore;
using ProjectQMSWpf.Models;

namespace ProjectQMSWpf
{
    class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<StudentQuizResult> StudentQuizResults { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>().HasNoKey();
            // Configure User - StudentQuizResults relationship
            modelBuilder.Entity<StudentQuizResult>()
                .HasOne(r => r.Student)
                .WithMany(u => u.QuizResults)
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Quiz - StudentQuizResults relationship
            modelBuilder.Entity<StudentQuizResult>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.QuizResults)
                .HasForeignKey(r => r.QuizID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Submissions relationships
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Quiz)
                .WithMany(q => q.Submissions)
                .HasForeignKey(s => s.QuizID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Question)
                .WithMany(q => q.Submissions)
                .HasForeignKey(s => s.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Quiz - Category relationship
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}