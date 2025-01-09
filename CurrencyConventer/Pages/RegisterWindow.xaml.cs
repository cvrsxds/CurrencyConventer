using CurrencyConventer.Pages;
using Microsoft.Data.Sqlite;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;

namespace CurrencyConventer.Pages
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Все поля обязательны.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Введенные пароли не совпадают.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (RegisterUser(username, password))
                {
                    MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    var startupWindow = new StartUpWindow();
                    startupWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Username уже используется. Пожалуйста выберите другой.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool RegisterUser(string username, string password)
        {
            string dbPath = "CurrencyConverter.db";

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE Username = @username", connection);
                checkCommand.Parameters.AddWithValue("@username", username);

                int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (userCount > 0)
                {
                    return false;
                }

                string hashedPassword = HashPassword(password);
                var insertCommand = new SqliteCommand("INSERT INTO Users (Username, Password) VALUES (@username, @password)", connection);
                insertCommand.Parameters.AddWithValue("@username", username);
                insertCommand.Parameters.AddWithValue("@password", hashedPassword);

                insertCommand.ExecuteNonQuery();
            }

            return true;
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var startupWindow = new StartUpWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}
