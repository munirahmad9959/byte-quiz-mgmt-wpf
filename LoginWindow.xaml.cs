using System.Linq;
using System.Windows;
using System.Windows.Input;
using ProjectQMSWpf.Models; // Ensure this namespace is correct for your User model
using Microsoft.EntityFrameworkCore;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AppDbContext _dbContext;

        public LoginWindow()
        {
            InitializeComponent();
            // Initialize the AppDbContext
            _dbContext = new AppDbContext();
        }

        private void LoginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            // Fetch email and password from input fields
            string enteredEmail = txtEmail.Text.Trim();
            string enteredPassword = txtPassword.Text.Trim();

            // Validate the inputs
            if (string.IsNullOrWhiteSpace(enteredEmail) || string.IsNullOrWhiteSpace(enteredPassword))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Query the database using LINQ with EF Core
                var user = _dbContext.Users
                    .FirstOrDefault(u => u.Email == enteredEmail);

                // Check if user is found
                if (user != null)
                {
                    // Log user data for debugging
                    MessageBox.Show($"User found: {user.Email}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    // If passwords are stored hashed, use a password verification method
                    if (user.Password == enteredPassword) // This is assuming plain-text passwords, update if you're hashing
                    {
                        MessageBox.Show("Logged in successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Proceed to next window or application flow here
                        if (user.Role == "Student")
                        {
                            StudentWindow studentWindow = new StudentWindow(enteredEmail);
                            this.Close();
                            studentWindow.Show();
                            
                        }
                        else if (user.Role == "Teacher")
                        {
                            TeacherWindow teacherWindow = new TeacherWindow();
                            this.Close();
                            teacherWindow.Show();
                        }
                        else
                        {
                            AdminWindow adminWindow = new AdminWindow();
                            this.Close();
                            adminWindow.Show();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for more details
                MessageBox.Show($"An error occurred during login: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignUp_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow newWindow = new RegisterWindow();
            this.Close();
            newWindow.Show();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }
    }
}
