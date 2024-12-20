using System;
using System.Data;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace CurrencyConventer.Pages
{
    public partial class LoginWindow : Window
    {
        public static string DbPath { get; } = "CurrencyConverter.db"; // Путь к базе данных
        public static string CurrentUser { get; private set; } // Текущий пользователь

        public LoginWindow()
        {
            InitializeComponent();
        }

        // Обработка нажатия на кнопку "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (AuthenticateUser(username, password))
                {
                    MessageBox.Show("Вход выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    var mainwindow = new MainWindow();
                    mainwindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неправильный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод проверки учетных данных
        private bool AuthenticateUser(string username, string password)
        {
            string connectionString = $"Data Source={DbPath}";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    var result = command.ExecuteScalar();
                    if (Convert.ToInt32(result) > 0)
                    {
                        CurrentUser = username; // Сохранение имени текущего пользователя
                        return true;
                    }
                    return false;
                }
            }
        }

        // Обработка нажатия на кнопку "Назад"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var startupWindow = new StartUpWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}