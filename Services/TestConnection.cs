using System;
using Microsoft.Data.SqlClient;
using Test1.Config;

namespace Test1.Services
{
    /// <summary>
    /// Класс для тестирования подключения к базе данных
    /// </summary>
    public static class TestConnection
    {
        /// <summary>
        /// Проверяет подключение к базе данных
        /// </summary>
        public static bool TestDatabaseConnection()
        {
            try
            {
                using (var connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    
                    // Проверяем, что таблицы существуют
                    var command = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' 
                        AND TABLE_NAME IN ('CP_Users', 'CP_Products', 'CP_Categories', 'CP_Orders', 'CP_OrderItems')
                    ", connection);
                    
                    int tableCount = (int)command.ExecuteScalar();
                    
                    if (tableCount == 5)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Выводит информацию о таблицах в базе данных
        /// </summary>
        public static void ShowDatabaseInfo()
        {
            try
            {
                using (var connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    
                    Console.WriteLine("\n=== Информация о базе данных ===");
                    Console.WriteLine($"Сервер: {connection.DataSource}");
                    Console.WriteLine($"База данных: {connection.Database}");
                    Console.WriteLine($"Статус: {connection.State}");
                    
                    // Проверяем таблицы
                    var tablesCommand = new SqlCommand(@"
                        SELECT TABLE_NAME 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' 
                        AND TABLE_NAME LIKE 'CP_%'
                        ORDER BY TABLE_NAME
                    ", connection);
                    
                    Console.WriteLine("\nНайденные таблицы с префиксом CP_:");
                    using (var reader = tablesCommand.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"  {count}. {reader["TABLE_NAME"]}");
                        }
                        if (count == 0)
                        {
                            Console.WriteLine("  Таблицы не найдены!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}

