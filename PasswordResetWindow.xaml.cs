using System;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace ProjectQMSWpf
{
    public partial class PasswordResetWindow : Window
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public PasswordResetWindow()
        {
            InitializeComponent();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string verificationCode = txtVerificationCode.Text;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsVerificationCodeValid(verificationCode))
            {
                ResetUserPassword(newPassword);
                MessageBox.Show("Password reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoginWindow window = new LoginWindow();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid or expired verification code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsVerificationCodeValid(string enteredCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ResetToken, TokenExpiry FROM Users WHERE ResetToken = @ResetToken";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ResetToken", enteredCode);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedCode = reader["ResetToken"].ToString();
                        DateTime tokenExpiry = Convert.ToDateTime(reader["TokenExpiry"]);

                        // Check if the code matches and is not expired
                        return storedCode == enteredCode && DateTime.Now <= tokenExpiry;
                    }
                }
            }
            return false;
        }

        private void ResetUserPassword(string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string hashedPassword = newPassword;
                string updateQuery = "UPDATE Users SET password = @Password, ResetToken = NULL, TokenExpiry = NULL WHERE ResetToken = @ResetToken";
                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Password", hashedPassword);
                updateCommand.Parameters.AddWithValue("@ResetToken", txtVerificationCode.Text);

                connection.Open();
                updateCommand.ExecuteNonQuery();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
