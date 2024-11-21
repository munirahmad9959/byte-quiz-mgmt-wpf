using System;
using System.Collections.Generic;

namespace ProjectQMSWpf.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        // Allow ResetToken and TokenExpiry to be null for regular registration
        public string? ResetToken { get; set; }  // Nullable string
        public DateTime? TokenExpiry { get; set; } // Nullable DateTime

        public string Role { get; set; }

        // Navigation Properties
        public ICollection<StudentQuizResult> QuizResults { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
