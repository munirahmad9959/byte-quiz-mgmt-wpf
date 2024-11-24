using Newtonsoft.Json;
using ProjectQMSWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for AddQuizzes.xaml
    /// </summary>
    public partial class AddQuizzes : Window
    {
        public AddQuizzesView ViewModel { get; private set; }
        public string UserEmail { get; private set; }
        private User CurrentUser;

        // Define the custom class for quiz data
        internal class QuizData
        {
            public int QuestionID { get; set; }
            public string CatName { get; set; }
            public string Quest { get; set; }
            public string Options { get; set; }
            public string CorrectOptions { get; set; }
        }

        public AddQuizzes(string useremail)
        {
            InitializeComponent();
            ViewModel = new AddQuizzesView();
            DataContext = ViewModel;
            UserEmail = useremail;
            LoadUserDetails();
            LoadCategories();
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

        private void LoadUserDetails()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    CurrentUser = context.Users.FirstOrDefault(u => u.Email == UserEmail);
                    if (CurrentUser != null)
                    {
                        MessageBox.Show($"Welcome, {CurrentUser.FirstName} {CurrentUser.LastName}!", "User Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowQuiz_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a category is selected
            if (QuizSelectionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz category first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the selected category
            var selectedCategory = QuizSelectionComboBox.SelectedItem as Category;
            if (selectedCategory == null)
            {
                MessageBox.Show("Invalid category selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int categoryID = selectedCategory.CategoryID;

            try
            {
                // Fetch and filter questions based on the selected category
                using (var context = new AppDbContext())
                {
                    var quizResults = context.Questions
                        .Where(q => q.CategoryID == categoryID) // Filter by CategoryID
                        .Select(q => new QuizData
                        {
                            QuestionID = q.QuestionID,
                            CatName = q.Category.Name,
                            Quest = q.QuestionText,
                            Options = q.Options,
                            CorrectOptions = q.CorrectAnswer
                        })
                        .ToList();

                    // Update the DataGrid with filtered results
                    QuizRecordsDataGrid.ItemsSource = quizResults;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching quizzes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddQuiz_Click(object sender, RoutedEventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(categoryNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(questionTextBox.Text) ||
                string.IsNullOrWhiteSpace(correctOptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(optionA.Text) ||
                string.IsNullOrWhiteSpace(optionB.Text) ||
                string.IsNullOrWhiteSpace(optionC.Text) ||
                string.IsNullOrWhiteSpace(optionD.Text))
            {
                MessageBox.Show("Please fill all the fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    // Get Category ID from Category Name
                    var category = context.Categories.FirstOrDefault(c => c.Name == categoryNameTextBox.Text.Trim());
                    if (category == null)
                    {
                        MessageBox.Show("Category not found. Please enter a valid category name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int categoryId = category.CategoryID;

                    // Create a list of options in the required JSON format
                    var options = new List<OptionText>
                    {
                        new OptionText { Option = "A", Text = optionA.Text.Trim() },
                        new OptionText { Option = "B", Text = optionB.Text.Trim() },
                        new OptionText { Option = "C", Text = optionC.Text.Trim() },
                        new OptionText { Option = "D", Text = optionD.Text.Trim() }
                    };

                    // Serialize options to JSON
                    string serializedOptions = JsonConvert.SerializeObject(options);

                    // Create a new Question instance
                    var question = new Question
                    {
                        CategoryID = categoryId,
                        QuestionText = questionTextBox.Text.Trim(),
                        Options = serializedOptions,
                        CorrectAnswer = correctOptionTextBox.Text.Trim()
                    };

                    // Add and save the question
                    context.Questions.Add(question);
                    context.SaveChanges();

                    MessageBox.Show("Quiz successfully added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear input fields
                    categoryNameTextBox.Clear();
                    questionTextBox.Clear();
                    correctOptionTextBox.Clear();
                    optionA.Clear();
                    optionB.Clear();
                    optionC.Clear();
                    optionD.Clear();

                    // Refresh DataGrid if necessary
                    ShowQuiz_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the quiz: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditQuiz_Click(object sender, RoutedEventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(categoryNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(questionTextBox.Text) ||
                string.IsNullOrWhiteSpace(correctOptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(optionA.Text) ||
                string.IsNullOrWhiteSpace(optionB.Text) ||
                string.IsNullOrWhiteSpace(optionC.Text) ||
                string.IsNullOrWhiteSpace(optionD.Text))
            {
                MessageBox.Show("Please fill all the fields", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    // Get the selected quiz from the DataGrid
                    var selectedQuiz = QuizRecordsDataGrid.SelectedItem as QuizData;
                    if (selectedQuiz == null)
                    {
                        MessageBox.Show("Please select a quiz to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Find the question in the database by its ID
                    var questionToEdit = context.Questions.FirstOrDefault(q => q.QuestionID == selectedQuiz.QuestionID);
                    if (questionToEdit == null)
                    {
                        MessageBox.Show("Quiz not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Get Category ID from Category Name
                    var category = context.Categories.FirstOrDefault(c => c.Name == categoryNameTextBox.Text.Trim());
                    if (category == null)
                    {
                        MessageBox.Show("Category not found. Please enter a valid category name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int categoryId = category.CategoryID;

                    // Create a list of options in the required JSON format
                    var options = new List<OptionText>
            {
                new OptionText { Option = "A", Text = optionA.Text.Trim() },
                new OptionText { Option = "B", Text = optionB.Text.Trim() },
                new OptionText { Option = "C", Text = optionC.Text.Trim() },
                new OptionText { Option = "D", Text = optionD.Text.Trim() }
            };

                    // Serialize options to JSON
                    string serializedOptions = JsonConvert.SerializeObject(options);

                    // Update the question with new data
                    questionToEdit.CategoryID = categoryId;
                    questionToEdit.QuestionText = questionTextBox.Text.Trim();
                    questionToEdit.Options = serializedOptions;
                    questionToEdit.CorrectAnswer = correctOptionTextBox.Text.Trim();

                    // Save changes
                    context.SaveChanges();

                    MessageBox.Show("Quiz successfully updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear input fields after saving
                    categoryNameTextBox.Clear();
                    questionTextBox.Clear();
                    correctOptionTextBox.Clear();
                    optionA.Clear();
                    optionB.Clear();
                    optionC.Clear();
                    optionD.Clear();

                    // Refresh DataGrid if necessary
                    ShowQuiz_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while editing the quiz: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected quiz from the DataGrid
                var selectedQuiz = QuizRecordsDataGrid.SelectedItem as QuizData;
                if (selectedQuiz == null)
                {
                    MessageBox.Show("Please select a quiz to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Confirm deletion
                var result = MessageBox.Show("Are you sure you want to delete this quiz?", "Delete Quiz", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                using (var context = new AppDbContext())
                {
                    // Find the question in the database by its ID
                    var questionToDelete = context.Questions.FirstOrDefault(q => q.QuestionID == selectedQuiz.QuestionID);
                    if (questionToDelete == null)
                    {
                        MessageBox.Show("Quiz not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Remove the quiz from the database
                    context.Questions.Remove(questionToDelete);
                    context.SaveChanges();

                    MessageBox.Show("Quiz successfully deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh DataGrid if necessary
                    ShowQuiz_Click(null, null);
                    categoryNameTextBox.Clear();
                    questionTextBox.Clear();
                    correctOptionTextBox.Clear();
                    optionA.Clear();
                    optionB.Clear();
                    optionC.Clear();
                    optionD.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the quiz: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuizRecordsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected item (row) in the DataGrid
            var selectedQuiz = QuizRecordsDataGrid.SelectedItem as QuizData;

            if (selectedQuiz != null)
            {
                // Populate the textboxes with the data from the selected row
                categoryNameTextBox.Text = selectedQuiz.CatName;  // Category Name
                questionTextBox.Text = selectedQuiz.Quest;        // Question Text
                correctOptionTextBox.Text = selectedQuiz.CorrectOptions;  // Correct Option

                // Deserialize and populate options
                var options = JsonConvert.DeserializeObject<List<OptionText>>(selectedQuiz.Options);
                if (options != null && options.Count == 4)
                {
                    optionA.Text = options.FirstOrDefault(o => o.Option == "A")?.Text;
                    optionB.Text = options.FirstOrDefault(o => o.Option == "B")?.Text;
                    optionC.Text = options.FirstOrDefault(o => o.Option == "C")?.Text;
                    optionD.Text = options.FirstOrDefault(o => o.Option == "D")?.Text;
                }
            }
        }

        internal class OptionText
        {
            public string Option { get; set; }
            public string Text { get; set; }
        }

        private void BackBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TeacherWindow window = new TeacherWindow(CurrentUser.Email);
            window.Show();
            this.Close();
        }
    }
}
