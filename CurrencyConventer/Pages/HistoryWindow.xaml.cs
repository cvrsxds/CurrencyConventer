using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace CurrencyConventer.Pages
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            string connectionString = $"Data Source={LoginWindow.DbPath}";
            var conversionHistory = new List<string>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Получаем ID текущего пользователя
                string getUserIdQuery = "SELECT Id FROM Users WHERE Username = @Username";
                using (var getUserIdCommand = new SqliteCommand(getUserIdQuery, connection))
                {
                    getUserIdCommand.Parameters.AddWithValue("@Username", LoginWindow.CurrentUser);
                    int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());

                    // Загружаем записи из таблицы Conversions
                    string getHistoryQuery = @"
                        SELECT FromCurrency, ToCurrency, Amount, ConvertedAmount, ConversionDate
                        FROM Conversions
                        WHERE UserId = @UserId";
                    using (var getHistoryCommand = new SqliteCommand(getHistoryQuery, connection))
                    {
                        getHistoryCommand.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = getHistoryCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fromCurrency = reader.GetString(0);
                                string toCurrency = reader.GetString(1);
                                double amount = reader.GetDouble(2);
                                double convertedAmount = reader.GetDouble(3);
                                string date = reader.GetString(4);

                                conversionHistory.Add(
                                    $"{amount} {fromCurrency} -> {convertedAmount:F2} {toCurrency} ({date})");
                            }
                        }
                    }
                }
            }

            // Отображаем записи в ListBox
            HistoryListBox.ItemsSource = conversionHistory;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}