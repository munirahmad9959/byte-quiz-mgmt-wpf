using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectQMSWpf.Models
{
    public class StudentQuizResult
    {
        [Key]
        public int ResultID { get; set; }
        public int StudentID { get; set; }
        public User Student { get; set; }
        public int QuizID { get; set; }
        public Quiz Quiz { get; set; }
        public int Score { get; set; }
        public string? ResultPDFPath { get; set; }
    }
}
