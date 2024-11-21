using System;
using System.Collections.Generic;

namespace ProjectQMSWpf.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; } // Navigation property

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int TotalMarks { get; set; }

        // Relationships
        public ICollection<StudentQuizResult> QuizResults { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<Question> Questions { get; set; } // Added for clarity
    }
}
