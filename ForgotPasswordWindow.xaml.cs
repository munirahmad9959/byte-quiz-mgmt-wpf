using Microsoft.Data.SqlClient;
using System;
using System.Net.Mail;
using System.Net;
using System.Windows;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        // Click event to send verification code
        private void SendVerificationCode_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmailVerification.Text;
            string verificationCode = GenerateVerificationCode();
            DateTime codeExpiry = DateTime.Now.AddMinutes(15);

            SaveVerificationCode(email, verificationCode, codeExpiry);

            // Send verification code email
            SendVerificationCodeEmail(email, verificationCode);
            StatusText.Text = "Verification code sent to your email.";
        }

        // Method to generate a random 6-digit verification code
        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a 6-digit code
        }

        // Save verification code and expiry in the database
        private void SaveVerificationCode(string email, string verificationCode, DateTime codeExpiry)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {
                string query = "UPDATE users SET ResetToken = @VerificationCode, TokenExpiry = @CodeExpiry WHERE Email = @Email";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@VerificationCode", verificationCode);
                command.Parameters.AddWithValue("@CodeExpiry", codeExpiry);
                command.Parameters.AddWithValue("@Email", email);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Send the verification code to the user's email
        private void SendVerificationCodeEmail(string email, string verificationCode)
        {
            try
            {
                // Configure the SMTP client
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("mughalmunir6224@gmail.com", "qlvnbrwnilssjwnj"), // Using your App Password
                    EnableSsl = true,
                };

                // Create the email message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("mughalmunir6224@gmail.com"),
                    Subject = "Password Reset Verification Code",
                    Body = $"Your password reset verification code is: {verificationCode}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);

                // Send the email
                smtpClient.Send(mailMessage);

                // Update status text on success
                StatusText.Text = "Verification code sent to your email.";

            }
            catch (Exception ex)
            {
                // Handle any exceptions and update the status text
                StatusText.Text = $"Failed to send email. Error: {ex.Message}";
                MessageBox.Show($"Error sending email: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
