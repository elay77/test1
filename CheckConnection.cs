using System;
using Microsoft.Data.SqlClient;
using Test1.Config;

namespace Test1
{
    /// <summary>
    /// Простой класс для проверки подключения к БД
    /// Запустите этот метод из консоли или добавьте кнопку в UI
    /// </summary>
    public static class CheckConnection
    {
        public static void Test()
        {
            Console.WriteLine("=== Проверка подключения к базе данных ===\n");
            Console.WriteLine($"Строка подключения: {DatabaseConfig.ConnectionString}\n");

            try
            {
                using (var connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    
                    Console.WriteLine("✓ Подключение успешно!");
                    Console.WriteLine($"  Сервер: {connection.DataSource}");
                    Console.WriteLine($"  База данных: {connection.Database}");
                    Console.WriteLine($"  Статус: {connection.State}\n");
                    
                    // Проверяем таблицы
                    var command = new SqlCommand(@"
                        SELECT TABLE_NAME 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' 
                        AND TABLE_NAME LIKE 'CP_%'
                        ORDER BY TABLE_NAME
                    ", connection);
                    
                    Console.WriteLine("Найденные таблицы с префиксом CP_:");
                    var tables = new System.Collections.Generic.List<string>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader["TABLE_NAME"].ToString()!);
                            Console.WriteLine($"  ✓ {reader["TABLE_NAME"]}");
                        }
                    }
                    
                    // Проверяем необходимые таблицы
                    var requiredTables = new[] { "CP_Users", "CP_Products", "CP_Categories", "CP_Orders", "CP_OrderItems" };
                    var missingTables = new System.Collections.Generic.List<string>();
                    
                    foreach (var table in requiredTables)
                    {
                        if (!tables.Contains(table))
                        {
                            missingTables.Add(table);
                        }
                    }
                    
                    Console.WriteLine();
                    if (missingTables.Count == 0)
                    {
                        Console.WriteLine("✓ Все необходимые таблицы найдены!");
                        Console.WriteLine("\nТеперь можно выполнить Scaffold-DbContext для генерации моделей.");
                    }
                    else
                    {
                        Console.WriteLine("⚠ Отсутствуют следующие таблицы:");
                        foreach (var table in missingTables)
                        {
                            Console.WriteLine($"  - {table}");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"✗ Ошибка подключения к SQL Server: {ex.Message}");
                Console.WriteLine("\nПроверьте:");
                Console.WriteLine("  1. Правильность имени сервера");
                Console.WriteLine("  2. Что база данных 'ispp2109' существует");
                Console.WriteLine("  3. Что SQL Server запущен");
                Console.WriteLine("  4. Права доступа к базе данных");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Общая ошибка: {ex.Message}");
            }
        }
    }
}

