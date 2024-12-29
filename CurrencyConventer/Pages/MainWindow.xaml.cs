using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace CurrencyConventer.Pages
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, (double Nominal, double Value)> _currencyRates = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadCurrencyRatesAsync();
        }

        private async void LoadCurrencyRatesAsync()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                string url = "https://www.cbr.ru/scripts/XML_daily.asp?";

                using HttpClient client = new();
                string xmlData = await client.GetStringAsync(url);

                // Использование XDocument для парсинга XML
                var doc = System.Xml.Linq.XDocument.Parse(xmlData);
                var currencies = from c in doc.Descendants("Valute")
                                 select new
                                 {
                                     CharCode = c.Element("CharCode")?.Value,
                                     Nominal = c.Element("Nominal")?.Value,
                                     Value = c.Element("Value")?.Value
                                 };

                foreach (var currency in currencies)
                {
                    _currencyRates[currency.CharCode] = (
                        double.Parse(currency.Nominal, CultureInfo.InvariantCulture),
                        double.Parse(currency.Value.Replace(',', '.'), CultureInfo.InvariantCulture)
                    );
                }

                FromCurrencyComboBox.ItemsSource = _currencyRates.Keys;
                ToCurrencyComboBox.ItemsSource = _currencyRates.Keys;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fromCurrency = FromCurrencyComboBox.SelectedItem as string;
                string toCurrency = ToCurrencyComboBox.SelectedItem as string;

                if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
                {
                    MessageBox.Show("Пожалуйста, выберите обе валюты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(InputAmountTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double amount))
                {
                    MessageBox.Show("Введите корректное число для конвертации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем данные о выбранных валютах
                var fromRate = _currencyRates[fromCurrency];
                var toRate = _currencyRates[toCurrency];

                // Конвертируем сумму
                double convertedAmount = (amount * fromRate.Value / fromRate.Nominal) / (toRate.Value / toRate.Nominal);
                OutputAmountTextBox.Text = convertedAmount.ToString("F2", CultureInfo.InvariantCulture);

                // Сохраняем конвертацию в базу данных
                SaveConversion(LoginWindow.CurrentUser, fromCurrency, toCurrency, amount, convertedAmount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка конвертации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveConversion(string username, string fromCurrency, string toCurrency, double amount, double convertedAmount)
        {
            string connectionString = $"Data Source={LoginWindow.DbPath}";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string getUserIdQuery = "SELECT Id FROM Users WHERE Username = @Username";
                using (var getUserIdCommand = new SqliteCommand(getUserIdQuery, connection))
                {
                    getUserIdCommand.Parameters.AddWithValue("@Username", username);
                    int userId = Convert.ToInt32(getUserIdCommand.ExecuteScalar());

                    string insertQuery = @"
                        INSERT INTO Conversions (UserId, FromCurrency, ToCurrency, Amount, ConvertedAmount, ConversionDate)
                        VALUES (@UserId, @FromCurrency, @ToCurrency, @Amount, @ConvertedAmount, @ConversionDate)";
                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@UserId", userId);
                        insertCommand.Parameters.AddWithValue("@FromCurrency", fromCurrency);
                        insertCommand.Parameters.AddWithValue("@ToCurrency", toCurrency);
                        insertCommand.Parameters.AddWithValue("@Amount", amount);
                        insertCommand.Parameters.AddWithValue("@ConvertedAmount", convertedAmount);
                        insertCommand.Parameters.AddWithValue("@ConversionDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historywindow = new HistoryWindow();
            historywindow.Show();
            this.Close();
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            var starupwindow = new StartUpWindow();
            starupwindow.Show();
            this.Close();
        }
    }
}