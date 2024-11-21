using System.Collections.Generic;

namespace ProjectQMSWpf.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }

        // Relationships
        public ICollection<Quiz> Quizzes { get; set; }
    }
}