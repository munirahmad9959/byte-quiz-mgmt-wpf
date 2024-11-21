using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectQMSWpf.Models
{
    public class Submission
    {
        public int SubmissionID { get; set; }
        public int StudentID { get; set; }  
        public User Student { get; set; }
        public int QuizID { get; set; }
        public Quiz Quiz { get; set; }
        public int QuestionID { get; set; }
        public Question Question { get; set; }
        public string? SelectedAnswer { get; set; }
    }
}
