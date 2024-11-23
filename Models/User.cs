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

        public string? ResetToken { get; set; } // Nullable string
        public DateTime? TokenExpiry { get; set; } // Nullable DateTime

        public string Role { get; set; }

        // Navigation property to quizzes and submissions
        public ICollection<Quiz> Quizzes { get; set; }  // User can have many quizzes
        public ICollection<Submission> Submissions { get; set; }  // User can have many submissions
    }
}
