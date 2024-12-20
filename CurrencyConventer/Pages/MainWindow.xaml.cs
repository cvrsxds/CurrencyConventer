using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Globalization;

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
                XDocument doc = XDocument.Parse(xmlData);
                var currencies = from c in doc.Descendants("Valute")
                                 select new
                                 {
                                     CharCode = c.Element("CharCode")?.Value,
                                     Nominal = c.Element("Nominal")?.Value,
                                     Name = c.Element("Name")?.Value,
                                     Value = c.Element("Value")?.Value
                                 };

                foreach (var currency in currencies)
                {
                    _currencyRates[currency.CharCode] = (
                        double.Parse(currency.Nominal, CultureInfo.InvariantCulture),
                        double.Parse(currency.Value.Replace(',', '.'), CultureInfo.InvariantCulture)
                    );
                }

                // Регулярное выражение для извлечения данных о валютах (по желанию, если необходимо использовать regex)
                string pattern = @"<CharCode>(.*?)</CharCode><Nominal>(.*?)</Nominal><Name>(.*?)</Name><Value>(.*?)</Value>";
                Regex regex = new(pattern);

                foreach (Match match in regex.Matches(xmlData))
                {
                    string charCode = match.Groups[1].Value;
                    double nominal = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                    double value = double.Parse(match.Groups[4].Value.Replace(',', '.'), CultureInfo.InvariantCulture);

                    _currencyRates[charCode] = (nominal, value);
                }

                // Обновляем выпадающие списки валют
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка конвертации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("История конверсий пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
