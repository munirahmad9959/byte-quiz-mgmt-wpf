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

        private void SendVerificationCode_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmailVerification.Text;

            try
            {
                using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    connection.Open();

                    // Check if the email exists in the database
                    string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email"; // Replace 'Users' with your table name
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        int userCount = (int)command.ExecuteScalar();

                        if (userCount == 0)
                        {
                            StatusText.Text = "This email does not exist in our database!";
                            return;
                        }
                        else
                        {
                            string verificationCode = GenerateVerificationCode();
                            DateTime codeExpiry = DateTime.Now.AddMinutes(15);

                            SaveVerificationCode(email, verificationCode, codeExpiry);

                            SendVerificationCodeEmail(email, verificationCode);

                            StatusText.Text = "Verification code sent to your email.";
                            PasswordResetWindow passwordResetWindow = new PasswordResetWindow();
                            passwordResetWindow.Show();
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


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
                string query = "UPDATE Users SET ResetToken = @VerificationCode, TokenExpiry = @CodeExpiry WHERE Email = @Email";
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

                string query = "SELECT FirstName, LastName FROM Users WHERE Email = @Email";
                using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                string firstName = reader.GetString(0);
                                string lastName = reader.GetString(1);

                                var mailMessage = new MailMessage
                                {
                                    From = new MailAddress("mughalmunir6224@gmail.com"),
                                    Subject = "Password Reset Verification Code",
                                    Body = $"Hi {firstName + " "+ lastName},\r\n\r\nWe received a request to reset your password. Please use the verification code below to proceed:\r\n\r\nYour password reset verification code is:\r\n\r\n{verificationCode}\r\nThis code is only valid for 15 minutes.\r\n\r\nIf you did not request this change, you can safely ignore this email.\r\n\r\nThank you,\r\nByteSlashers.inc Team",
                                    IsBodyHtml = false,
                                };
                                mailMessage.To.Add(email);

                                // Send the email
                                smtpClient.Send(mailMessage);

                                // Update status text on success
                                StatusText.Text = "Verification code sent to your email.";


                            }
                            else
                            {
                                Console.WriteLine("No user found with the specified email.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
