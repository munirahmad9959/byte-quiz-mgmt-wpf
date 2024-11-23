using Microsoft.EntityFrameworkCore;
using ProjectQMSWpf.Models;

namespace ProjectQMSWpf
{
    class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Submission> Submissions { get; set; }  // Added DbSet for Submissions

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing relationship between Question and Category
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship between Quiz and User
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.User)  // Quiz has one User
                .WithMany(u => u.Quizzes)  // User can have many quizzes
                .HasForeignKey(q => q.UserId)  // Foreign key in Quiz table
                .OnDelete(DeleteBehavior.Cascade);  // Optional: Cascade delete quizzes when the user is deleted

            // Relationship between Submission and User
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.User)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete submissions when the user is deleted

            // Relationship between Submission and Quiz
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Quiz)
                .WithMany()  // A submission belongs to one quiz
                .HasForeignKey(s => s.QuizID)
                .OnDelete(DeleteBehavior.Restrict);  // Optionally, restrict deletion of quizzes

            // Relationship between Submission and Category
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);  // Optionally, restrict deletion of categories
        }
    }
}
