using System;
using System.Linq;
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
        public string UserEmail { get; private set; }
        private User CurrentUser; // Object to hold user details

        public StudentWindow(string useremail)
        {
            InitializeComponent();

            UserEmail = useremail;

            ViewModel = new StudentViewModal();

            // Fetch and store user details
            LoadUserDetails();


            // Load categories from database
            LoadCategories();

            // Set the DataContext to the ViewModel
            DataContext = ViewModel;
            LoadQuizResults();
        }

        private void LoadQuizResults()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var quizResults = context.StudentQuizResults
                        .Where(r => r.StudentID == CurrentUser.UserId)
                        .Select(r => new
                        {
                            QuizName = r.Quiz.Name, // Assuming Quiz.Name exists
                            Score = r.Score,
                            TotalQuestions = r.Quiz.Questions.Count, // Assuming Quiz has a Questions collection
                            DateTaken = r.Quiz.DateCreated // Assuming Quiz.DateCreated exists
                        })
                        .ToList();

                    QuizRecordsDataGrid.ItemsSource = quizResults;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quiz results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserDetails()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Fetch the user details using the email
                    CurrentUser = context.Users.FirstOrDefault(u => u.Email == UserEmail);

                    if (CurrentUser != null)
                    {
                        // Display user information (for demonstration purposes)
                        MessageBox.Show($"Welcome, {CurrentUser.FirstName} {CurrentUser.LastName}!", "User Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        username.Text = $"{CurrentUser.FirstName} {CurrentUser.LastName}";
                    }
                    else
                    {
                        MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close(); // Close the window if the user is not found
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var categories = context.Categories.ToList();

                    QuizSelectionComboBox.ItemsSource = categories;
                    QuizSelectionComboBox.DisplayMemberPath = "Name";
                    QuizSelectionComboBox.SelectedValuePath = "CategoryID";
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
            if (QuizSelectionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz category first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedCategory = QuizSelectionComboBox.SelectedItem as Category;
            int categoryID = selectedCategory.CategoryID;

            try
            {
                QuizWindow quizWindow = new QuizWindow(categoryID, CurrentUser.Email);
                quizWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
