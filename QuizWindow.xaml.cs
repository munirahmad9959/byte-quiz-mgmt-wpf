using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectQMSWpf
{
    public partial class QuizWindow : Window
    {
        // Dummy quiz data
        private List<Question> questions;

        public QuizWindow()
        {
            InitializeComponent();
            LoadDummyData();
            DisplayQuestions();
        }

        private void LoadDummyData()
        {
            questions = new List<Question>
            {
                new Question
                {
                    QuestionText = "What is the capital of France?",
                    Options = new List<string> { "Paris", "Berlin", "Madrid", "Rome" },
                    CorrectOption = "Paris"
                },
                new Question
                {
                    QuestionText = "Which programming language is used for WPF?",
                    Options = new List<string> { "C#", "Java", "Python", "JavaScript" },
                    CorrectOption = "C#"
                },
                new Question
                {
                    QuestionText = "What is 2 + 2?",
                    Options = new List<string> { "3", "4", "5", "6" },
                    CorrectOption = "4"
                }
            };
        }

        private void DisplayQuestions()
        {
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];

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
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10, 5, 0, 5)
                };

                foreach (var option in question.Options)
                {
                    // Create a styled RadioButton
                    RadioButton radioButton = new RadioButton
                    {
                        GroupName = $"Question{i}",
                        Style = (Style)FindResource("RadioButtonStyle"), // Apply the style
                    };

                    // Add the text as part of a horizontal StackPanel
                    StackPanel radioContent = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    TextBlock optionText = new TextBlock
                    {
                        Text = option,
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

            for (int i = 0; i < questions.Count; i++)
            {
                string groupName = $"Question{i}";

                // Find the corresponding StackPanel for this question
                var questionPanel = QuestionsPanel.Children.OfType<StackPanel>().ElementAt(i);

                // Get the options panel (the second child of the question panel)
                var optionsPanel = questionPanel.Children.OfType<StackPanel>().Last();

                // Find the selected RadioButton
                var selectedRadioButton = optionsPanel.Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked == true);

                // Retrieve the text of the selected option
                var selectedOption = (selectedRadioButton?.Content as StackPanel)?
                    .Children.OfType<TextBlock>().FirstOrDefault()?.Text;

                if (selectedOption == questions[i].CorrectOption)
                {
                    score++;
                }
            }

            MessageBox.Show($"You scored {score} out of {questions.Count}!", "Quiz Result", MessageBoxButton.OK, MessageBoxImage.Information);
            StudentWindow dashboard = new StudentWindow();
            dashboard.Show();
            this.Close();
        }
    }

    public class Question
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectOption { get; set; }
    }
}
