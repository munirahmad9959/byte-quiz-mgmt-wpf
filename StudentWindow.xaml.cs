using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ProjectQMSWpf.Models;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        public StudentViewModal ViewModel { get; private set; }

        public StudentWindow()
        {
            InitializeComponent();

            ViewModel = new StudentViewModal();

            // Load categories from database
            LoadCategories();

            // Set the DataContext to the ViewModel
            DataContext = ViewModel;
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Query to get categories from the database
                    var categories = context.Categories.ToList();

                    // Bind the categories to the ComboBox
                    QuizSelectionComboBox.ItemsSource = categories;
                    QuizSelectionComboBox.DisplayMemberPath = "Name"; // This will display the 'Name' property of the Category
                    QuizSelectionComboBox.SelectedValuePath = "CategoryID"; // This will bind the 'CategoryID' to the selected value
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}");
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private bool IsMaximize = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 800;
                    this.Height = 650;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }

        private void takeQuiz_Click(object sender, RoutedEventArgs e)
        {
            if(QuizSelectionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select quiz category first.");
            }
            else
            {
                QuizWindow quizWindow = new QuizWindow();
                quizWindow.Show();
                this.Close();
            }
        }
    }
}
