﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace ProjectQMSWpf
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
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
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectQMSWpf;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            string enteredEmail = txtEmail.Text;
            string enteredPassword = txtPassword.Text;

            string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", enteredEmail);
                    command.Parameters.AddWithValue("@Password", enteredPassword);

                    connection.Open();
                    int result = (int)command.ExecuteScalar();

                    if (result == 1)
                    {
                        MessageBox.Show("Logged in successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Invalid email or password.");
                    }
                }
            }
        }

        private void SignUp_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow newWindow = new RegisterWindow();
            newWindow.Show();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }

    }
}