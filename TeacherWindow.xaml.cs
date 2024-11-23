using Microsoft.Win32;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using ProjectQMSWpf.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for TeacherWindow.xaml
    /// </summary>
    public partial class TeacherWindow : Window
    {
        public TeacherViewModal ViewModel { get; private set; }
        public string UserEmail { get; private set; }
        private User CurrentUser;
        public TeacherWindow(string useremail)
        {
            InitializeComponent();
            ViewModel = new TeacherViewModal();
            DataContext = ViewModel;
            UserEmail = useremail;
            LoadQuizResults();
            //LoadCategories();
            LoadUserDetails();
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
                        username.Text = $"{CurrentUser.FirstName} {CurrentUser.LastName}";
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


        private void LoadQuizResults()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Fetch data by joining Submission, User, and Category tables
                    var quizResults = context.Submissions
                        .Select(s => new
                        {
                            UserName = s.User.FirstName + " " + s.User.LastName, // Combine FirstName and LastName
                            CategoryName = s.Category.Name, // Get Category name
                            QuizID = s.QuizID,
                            MarksObtained = s.MarksObtained,
                            TotalMarks = s.TotalMarks
                        })
                        .ToList();

                    // Display data (assumes a DataGrid named QuizResultsDataGrid exists)
                    QuizRecordsDataGrid.ItemsSource = quizResults;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quiz results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private bool IsMaximize = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    WindowState = WindowState.Normal;
                    Width = 1000;
                    Height = 650;
                    IsMaximize = false;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    IsMaximize = true;
                }
            }
        }

        private void CreatePdf(string filePath, List<QuestionAnswer> answeredQuestions)
        {
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fontTitle = new XFont("Verdana", 14, XFontStyleEx.Bold);
                var fontRegular = new XFont("Verdana", 12, XFontStyleEx.Regular);

                double yPos = 40;

                gfx.DrawString("Quiz Results", fontTitle, XBrushes.Black, new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                yPos += 60;

                foreach (var obj in answeredQuestions)
                {
                    using (var _context = new AppDbContext())
                    {
                        var question = _context.Questions.FirstOrDefault(q => q.QuestionID == obj.QuestionID);

                        if (question != null)
                        {
                            gfx.DrawString($"Question: {question.QuestionText}", fontTitle, XBrushes.Black, new XPoint(40, yPos));
                            yPos += 20;

                            List<OptionText> optionTexts = JsonConvert.DeserializeObject<List<OptionText>>(question.Options);
                            foreach (var option in optionTexts)
                            {
                                gfx.DrawString($"{option.Option}. {option.Text}", fontRegular, XBrushes.Black, new XPoint(60, yPos));
                                yPos += 15;
                            }

                            var correctOption = optionTexts.FirstOrDefault(ot => ot.Option == question.CorrectAnswer);
                            gfx.DrawString($"Selected Answer: {obj.SelectedAnswer}", fontRegular, XBrushes.Black, new XPoint(40, yPos));
                            yPos += 15;
                            gfx.DrawString($"Correct Answer: {correctOption.Text}", fontRegular, XBrushes.Green, new XPoint(40, yPos));
                            yPos += 30;
                        }
                    }
                }

                try
                {
                    document.Save(filePath);
                    MessageBox.Show("PDF saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DownloadPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int quizId)
            {
                try
                {
                    using (var context = new AppDbContext())
                    {
                        var submission = context.Submissions.FirstOrDefault(s => s.QuizID == quizId);
                        if (submission == null)
                        {
                            MessageBox.Show("Submission not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var answeredQuestions = JsonConvert.DeserializeObject<List<QuestionAnswer>>(submission.AnsweredQuestions);

                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            Filter = "PDF Files (*.pdf)|*.pdf",
                            DefaultExt = "pdf",
                            AddExtension = true
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filePath = saveFileDialog.FileName;
                            CreatePdf(filePath, answeredQuestions);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching submission details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        internal class QuestionAnswer
        {
            public int QuestionID { get; set; }
            public string SelectedAnswer { get; set; }
        }

        internal class OptionText
        {
            public string Option { get; set; }
            public string Text { get; set; }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow window = new LoginWindow();
            window.Show();
            this.Close();
        }

        private void AddQuizzes_Click(object sender, RoutedEventArgs e)
        {
            AddQuizzes window = new AddQuizzes(CurrentUser.Email);
            window.Show();
            this.Close();
        }
    }
}
