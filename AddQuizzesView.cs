﻿using ProjectQMSWpf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectQMSWpf
{
    public class AddQuizzesView
    {
        public ObservableCollection<Quiz> GridData { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        public AddQuizzesView()
        {
            GridData = new ObservableCollection<Quiz>();
            Categories = new ObservableCollection<Category>();
        }
    }
}
