using System;
using Microsoft.Data.Sqlite;

public class DataBaseInitializer
{
    public static void InitializeDatabase()
    {
        string dbPath = "CurrencyConverter.db"; // Исправлено имя файла

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // SQL для создания таблицы пользователей
            string createUserTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                );";

            // SQL для создания таблицы конверсий
            string createConversionsTable = @"
                CREATE TABLE IF NOT EXISTS Conversions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, -- Исправлено 'PRIMATY' на 'PRIMARY'
                    UserId INTEGER NOT NULL,
                    FromCurrency TEXT NOT NULL,
                    ToCurrency TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    ConvertedAmount REAL NOT NULL,
                    ConversionDate DATETIME NOT NULL, -- Исправлено 'ConversasionDate'
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );";

            // Создание таблицы Users
            using (var command = new SqliteCommand(createUserTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // Создание таблицы Conversions
            using (var command = new SqliteCommand(createConversionsTable, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}