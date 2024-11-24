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
                var fontTitle = new XFont("Verdana", 14, XFontStyleEx.Bold);
                var fontRegular = new XFont("Verdana", 12, XFontStyleEx.Regular);
                var fontGreen = new XFont("Verdana", 12, XFontStyleEx.Regular); // Same font but will use a green brush

                double pageWidthMargin = 40; // Left and right margins
                double pageWidth = 595 - 2 * pageWidthMargin; // A4 width minus margins
                double pageHeightThreshold = 750; // A4 height minus bottom margin
                double margin = 40;
                double yPos = margin;

                void AddNewPage(ref PdfPage page, ref XGraphics gfx, ref double y)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                PdfPage page = null;
                XGraphics gfx = null;
                AddNewPage(ref page, ref gfx, ref yPos); // Create the first page

                // Title for the document
                gfx.DrawString("Quiz Results", fontTitle, XBrushes.Black, new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                yPos += 60;

                foreach (var obj in answeredQuestions)
                {
                    using (var _context = new AppDbContext())
                    {
                        var question = _context.Questions.FirstOrDefault(q => q.QuestionID == obj.QuestionID);

                        if (question != null)
                        {
                            // Check for page overflow before drawing question
                            if (yPos + 100 > pageHeightThreshold)
                            {
                                AddNewPage(ref page, ref gfx, ref yPos);
                            }

                            // Draw the question text with word wrapping
                            yPos = DrawTextWithWrapping(gfx, $"Question: {question.QuestionText}", fontTitle, margin, yPos, pageWidth);

                            // Draw question options with word wrapping
                            List<OptionText> optionTexts = JsonConvert.DeserializeObject<List<OptionText>>(question.Options);
                            foreach (var option in optionTexts)
                            {
                                yPos = DrawTextWithWrapping(gfx, $"{option.Option}. {option.Text}", fontRegular, margin + 20, yPos, pageWidth - 20);

                                // Check for page overflow during option drawing
                                if (yPos > pageHeightThreshold)
                                {
                                    AddNewPage(ref page, ref gfx, ref yPos);
                                }
                            }

                            // Draw selected answer
                            yPos = DrawTextWithWrapping(gfx, $"Selected Answer: {obj.SelectedAnswer}", fontRegular, margin, yPos, pageWidth);

                            // Draw correct answer in green
                            var correctOption = optionTexts.FirstOrDefault(ot => ot.Option == question.CorrectAnswer);
                            yPos = DrawTextWithWrapping(gfx, $"Correct Answer: {correctOption.Text}", fontGreen, margin, yPos, pageWidth, XBrushes.Green);

                            // Check for page overflow after answers
                            if (yPos > pageHeightThreshold)
                            {
                                AddNewPage(ref page, ref gfx, ref yPos);
                            }
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

        private double DrawTextWithWrapping(XGraphics gfx, string text, XFont font, double x, double y, double maxWidth, XBrush brush = null)
        {
            if (brush == null)
            {
                brush = XBrushes.Black; // Default brush
            }

            var words = text.Split(' ');
            string line = "";
            double lineHeight = gfx.MeasureString("A", font).Height;

            foreach (var word in words)
            {
                string testLine = line + (line.Length > 0 ? " " : "") + word;
                double testWidth = gfx.MeasureString(testLine, font).Width;

                if (testWidth > maxWidth)
                {
                    // Draw the current line and move to the next line
                    gfx.DrawString(line, font, brush, new XPoint(x, y));
                    line = word; // Start a new line with the current word
                    y += lineHeight;
                }
                else
                {
                    line = testLine; // Continue adding words to the current line
                }
            }

            // Draw the last line
            if (line.Length > 0)
            {
                gfx.DrawString(line, font, brush, new XPoint(x, y));
                y += lineHeight;
            }

            return y;
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

        private void CloseBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
