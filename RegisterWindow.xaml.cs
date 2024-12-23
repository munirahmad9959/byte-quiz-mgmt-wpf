using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectQMSWpf.Models; // Ensure this namespace is correct for your User model

namespace ProjectQMSWpf
{
    public partial class RegisterWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private string selectedRole = string.Empty;

        public RegisterWindow()
        {
            InitializeComponent();
            // Initialize the AppDbContext
            _dbContext = new AppDbContext();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordTextBox.Password.Trim();
            string confirmPassword = ConfirmPasswordTextBox.Password.Trim();

            // Basic validation
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(selectedRole))
            {
                MessageBox.Show("Please fill all the fields!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Check if the email already exists using LINQ
                var existingUser = _dbContext.Users.FirstOrDefault(u => u.Email == email);

                if (existingUser != null)
                {
                    MessageBox.Show("The email is already registered.", "Duplicate Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create a new user object
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password, // Note: For security, hash the password before saving in production
                    Role = selectedRole
                };

                // Add the new user to the Users table
                _dbContext.Users.Add(newUser);

                // Save changes to the database
                _dbContext.SaveChanges();

                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigate to the login window
                LoginWindow window = new LoginWindow();
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                // Extract inner exception details if available
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
                MessageBox.Show($"An error occurred: {ex.Message}\nInner Exception: {innerExceptionMessage}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void TeacherRadioButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRole = "Teacher";
        }

        private void StudentRadioButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRole = "Student";
        }

        private void Placeholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Focus the corresponding TextBox or PasswordBox when clicking on the placeholder
            if (sender is TextBlock placeholder)
            {
                switch (placeholder.Name)
                {
                    case "FirstNamePlaceholder":
                        FirstNameTextBox.Focus();
                        break;
                    case "LastNamePlaceholder":
                        LastNameTextBox.Focus();
                        break;
                    case "EmailPlaceholder":
                        EmailTextBox.Focus();
                        break;
                    case "PasswordPlaceholder":
                        PasswordTextBox.Focus();
                        break;
                    case "ConfirmPasswordPlaceholder":
                        ConfirmPasswordTextBox.Focus();
                        break;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Hide placeholder when typing starts
            if (sender is TextBox textBox)
            {
                switch (textBox.Name)
                {
                    case "FirstNameTextBox":
                        FirstNamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                            ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "LastNameTextBox":
                        LastNamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                            ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "EmailTextBox":
                        EmailPlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                            ? Visibility.Visible : Visibility.Collapsed;
                        break;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Hide placeholder when typing starts in PasswordBox
            if (sender is PasswordBox passwordBox)
            {
                switch (passwordBox.Name)
                {
                    case "PasswordTextBox":
                        PasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                            ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "ConfirmPasswordTextBox":
                        ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                            ? Visibility.Visible : Visibility.Collapsed;
                        break;
                }
            }
        }

        private void SignIn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginWindow window = new LoginWindow();
            window.Show();
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
