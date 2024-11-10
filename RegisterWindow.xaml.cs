using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace ProjectQMSWpf
{
    public partial class RegisterWindow : Window
    {
        private string selectedRole = string.Empty;

        public RegisterWindow()
        {
            InitializeComponent();
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
                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if email already exists
                    string checkEmailQuery = "SELECT COUNT(*) FROM users WHERE Email = @Email";
                    using (SqlCommand checkEmailCommand = new SqlCommand(checkEmailQuery, connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", email);
                        int emailCount = (int)checkEmailCommand.ExecuteScalar();

                        if (emailCount > 0)
                        {
                            MessageBox.Show("The email is already registered.", "Duplicate Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Insert new user
                    string query = "INSERT INTO users (FirstName, LastName, Password, Email, Role) VALUES (@FirstName, @LastName, @Password, @Email, @Role)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Role", selectedRole);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Registration failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoginWindow window = new LoginWindow();
            window.Show();
            this.Close();
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
        }
    }
}
