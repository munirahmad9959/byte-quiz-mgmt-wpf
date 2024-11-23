using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using ProjectQMSWpf.Models;

namespace ProjectQMSWpf
{
    public partial class QuizWindow : Window
    {
        private int _categoryID;
        private ObservableCollection<Question> _questions;
        public string UserEmail { get; private set; }
        private User CurrentUser;
        private DispatcherTimer _submissionTimer;
        private TimeSpan _timeRemaining;

        public QuizWindow(int categoryID, string useremail)
        {
            InitializeComponent();
            _categoryID = categoryID;
            UserEmail = useremail;
            LoadUserDetails();
            LoadQuestions();

            // Initialize and start the timer
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timeRemaining = TimeSpan.FromSeconds(30); // Set timer duration
            TimerTextBlock.Text = $"Time Remaining: {_timeRemaining:mm\\:ss}";

            _submissionTimer = new DispatcherTimer();
            _submissionTimer.Interval = TimeSpan.FromSeconds(1); // Update every second
            _submissionTimer.Tick += TimerTick;
            _submissionTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            TimerTextBlock.Text = $"Time Remaining: {_timeRemaining:mm\\:ss}";

            if (_timeRemaining <= TimeSpan.Zero)
            {
                _submissionTimer.Stop();
                MessageBox.Show("Time's up! Your quiz will be submitted automatically.", "Time Over", MessageBoxButton.OK, MessageBoxImage.Information);
                SubmitQuiz();
            }
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            // Stop the timer and submit the quiz automatically
            _submissionTimer.Stop();
            MessageBox.Show("Time's up! Your quiz will be submitted automatically.", "Time Over", MessageBoxButton.OK, MessageBoxImage.Information);
            SubmitQuiz();
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
                        MessageBox.Show($"Welcome, {CurrentUser.FirstName} {CurrentUser.LastName}!",
                                        "User Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
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

                StackPanel questionPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

                TextBlock questionText = new TextBlock
                {
                    Text = $"{i + 1}. {question.QuestionText}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.AliceBlue,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                questionPanel.Children.Add(questionText);

                StackPanel optionsPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10, 5, 0, 5)
                };

                try
                {
                    var options = JsonConvert.DeserializeObject<List<Options>>(question.Options) ?? new List<Options>();

                    foreach (var option in options)
                    {
                        RadioButton radioButton = new RadioButton
                        {
                            GroupName = $"Question{i}",
                            Style = (Style)FindResource("RadioButtonStyle"),
                        };

                        StackPanel radioContent = new StackPanel { Orientation = Orientation.Horizontal };
                        TextBlock optionText = new TextBlock { Text = option.Text, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.AliceBlue };

                        radioContent.Children.Add(optionText);
                        radioButton.Content = radioContent;

                        optionsPanel.Children.Add(radioButton);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error parsing options for question {i + 1}: {ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                questionPanel.Children.Add(optionsPanel);
                QuestionsPanel.Children.Add(questionPanel);
            }
        }

        private void SubmitQuizButton_Click(object sender, RoutedEventArgs e)
        {
            _submissionTimer.Stop(); // Stop the timer if the user submits manually
            SubmitQuiz();
        }

        private void SubmitQuiz()
        {
            int score = 0;

            // Calculate the user's score
            for (int i = 0; i < _questions.Count; i++)
            {
                var questionPanel = QuestionsPanel.Children.OfType<StackPanel>().ElementAt(i);
                var optionsPanel = questionPanel.Children.OfType<StackPanel>().Last();
                var selectedRadioButton = optionsPanel.Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked == true);

                var selectedText = (selectedRadioButton?.Content as StackPanel)?
                    .Children.OfType<TextBlock>().FirstOrDefault()?.Text;

                if (selectedText != null)
                {
                    var options = JsonConvert.DeserializeObject<List<Options>>(_questions[i].Options);
                    var selectedOption = options.FirstOrDefault(opt => opt.Text == selectedText);

                    if (selectedOption != null)
                    {
                        bool isCorrect = selectedOption.Option == _questions[i].CorrectAnswer;
                        if (isCorrect) score++;
                    }
                }
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    // Save quiz result
                    var quizResult = new Quiz
                    {
                        UserId = CurrentUser.UserId,
                        CategoryName = context.Categories
                            .Where(c => c.CategoryID == _categoryID)
                            .Select(c => c.Name)
                            .FirstOrDefault(),
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        MarksObtained = score,
                        TotalMarks = _questions.Count
                    };

                    context.Quizzes.Add(quizResult);
                    context.SaveChanges();

                    // Save submission details
                    var submission = new Submission
                    {
                        UserId = CurrentUser.UserId,
                        CategoryID = _categoryID,
                        QuizID = quizResult.QuizID,
                        MarksObtained = score,
                        TotalMarks = _questions.Count,
                        StartTime = quizResult.StartTime,
                        EndTime = quizResult.EndTime,
                        AnsweredQuestions = GenerateAnsweredQuestionsJson()
                    };

                    context.Submissions.Add(submission);
                    context.SaveChanges();

                    MessageBox.Show($"You scored {score} out of {_questions.Count}!",
                                    "Quiz Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save quiz and submission data: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Navigate back to StudentWindow
            StudentWindow dashboard = new StudentWindow(CurrentUser.Email);
            dashboard.Show();
            this.Close();
        }

        private string GenerateAnsweredQuestionsJson()
        {
            var answeredQuestions = new List<object>();

            for (int i = 0; i < _questions.Count; i++)
            {
                var questionPanel = QuestionsPanel.Children.OfType<StackPanel>().ElementAt(i);
                var optionsPanel = questionPanel.Children.OfType<StackPanel>().Last();
                var selectedRadioButton = optionsPanel.Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked == true);

                var selectedText = (selectedRadioButton?.Content as StackPanel)?
                    .Children.OfType<TextBlock>().FirstOrDefault()?.Text;

                answeredQuestions.Add(new
                {
                    QuestionID = _questions[i].QuestionID,
                    SelectedAnswer = selectedText ?? "Not Answered"
                });
            }

            return JsonConvert.SerializeObject(answeredQuestions);
        }
    }

    public class Options
    {
        public string Option { get; set; }
        public string Text { get; set; }
    }
}
