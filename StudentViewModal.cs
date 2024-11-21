using ProjectQMSWpf.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectQMSWpf
{
    public class StudentViewModal
    {
        public ObservableCollection<Quiz> GridData { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        public StudentViewModal()
        {
            GridData = new ObservableCollection<Quiz>();
            Categories = new ObservableCollection<Category>();
        }
    }
}
