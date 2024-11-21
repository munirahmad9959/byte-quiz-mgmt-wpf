using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectQMSWpf.Models
{
    public class Question
    {

        public int QuestionID { get; set; }  // Primary key (ensure this is present)

        public int QuizID { get; set; }     // Foreign key
        public Quiz Quiz { get; set; }      // Navigation property

        public string QuestionText { get; set; }
        public string Options { get; set; }  // JSON format
        public string CorrectAnswer { get; set; }

        // Navigation Property
        public ICollection<Submission> Submissions { get; set; }
    }
}