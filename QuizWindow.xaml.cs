using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using ProjectQMSWpf.Models;

namespace ProjectQMSWpf
{
    public partial class QuizWindow : Window
    {
        private int _categoryID;
        private ObservableCollection<Question> _questions;
        public string UserEmail { get; private set; }
        private User CurrentUser; // Object to hold user details

        public QuizWindow(int categoryID, string useremail)
        {
            InitializeComponent();
            _categoryID = categoryID;
            UserEmail = useremail;
            LoadUserDetails();

            LoadQuestions();
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


        private void LoadQuestions()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var questions = context.Questions
                        .Where(q => q.CategoryID == _categoryID)
                        .ToList();

                    if (questions.Count == 0)
                    {
                        MessageBox.Show("No questions available for this category.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        return;
                    }

                    _questions = new ObservableCollection<Question>(questions);
                }

                DisplayQuestions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading questions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void DisplayQuestions()
        {
            for (int i = 0; i < _questions.Count; i++)
            {
                var question = _questions[i];

                // Create a StackPanel for each question
                StackPanel questionPanel = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 20)
                };

                // Add question text
                TextBlock questionText = new TextBlock
                {
                    Text = $"{i + 1}. {question.QuestionText}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                questionPanel.Children.Add(questionText);

                // Create a vertical StackPanel for options
                StackPanel optionsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal, // Horizontal alignment
                    Margin = new Thickness(10, 5, 0, 5)
                };

                // Deserialize the options from JSON
                var options = JsonConvert.DeserializeObject<List<Option>>(question.Options);

                foreach (var option in options)
                {
                    // Create a styled RadioButton
                    RadioButton radioButton = new RadioButton
                    {
                        GroupName = $"Question{i}",
                        Style = (Style)FindResource("RadioButtonStyle"), // Apply custom style
                    };

                    // Add the text as part of a horizontal StackPanel
                    StackPanel radioContent = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    TextBlock optionText = new TextBlock
                    {
                        Text = option.Text,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    radioContent.Children.Add(optionText);
                    radioButton.Content = radioContent;

                    optionsPanel.Children.Add(radioButton);
                }

                questionPanel.Children.Add(optionsPanel);

                // Add question panel to the main QuestionsPanel
                QuestionsPanel.Children.Add(questionPanel);
            }
        }

        private void SubmitQuizButton_Click(object sender, RoutedEventArgs e)
        {
            int score = 0;

            for (int i = 0; i < _questions.Count; i++)
            {
                var questionPanel = QuestionsPanel.Children.OfType<StackPanel>().ElementAt(i);
                var optionsPanel = questionPanel.Children.OfType<StackPanel>().Last();
                var selectedRadioButton = optionsPanel.Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked == true);

                // Retrieve the text of the selected option
                var selectedOption = (selectedRadioButton?.Content as StackPanel)?
                    .Children.OfType<TextBlock>().FirstOrDefault()?.Text;

                if (selectedOption == _questions[i].CorrectAnswer)
                {
                    score++;
                }
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    var quizResult = new StudentQuizResult
                    {
                        StudentID = CurrentUser.UserId, // Use the logged-in user's ID
                        QuizID = _questions.First().QuizID,
                        Score = score,
                        ResultPDFPath = null // Optional: Generate a PDF if required
                    };

                    context.StudentQuizResults.Add(quizResult);
                    context.SaveChanges();

                    MessageBox.Show($"You scored {score} out of {_questions.Count}!", "Quiz Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save quiz result: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Navigate back to StudentWindow
            StudentWindow dashboard = new StudentWindow(CurrentUser.Email); // Pass the current user's email
            dashboard.Show();
            this.Close();
        }
    }

    public class Option
    {
        public int OptionID { get; set; }
        public string Text { get; set; }
    }

}
