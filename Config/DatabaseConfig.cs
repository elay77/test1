namespace Test1.Config
{
    /// <summary>
    /// Класс для настройки подключения к базе данных
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Строка подключения к базе данных SQL Server
        /// База данных содержит таблицы: CP_Users, CP_Products, CP_Categories, CP_Orders, CP_OrderItems
        /// </summary>
        public static string ConnectionString { get; set; } = 
            "Server=localhost;Database=work;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

        // Примеры других вариантов подключения:
        // 
        // Windows Authentication (встроенная аутентификация):
        // "Server=localhost;Database=work;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;"
        //
        // SQL Server Authentication (с логином и паролем):
        // "Server=localhost;Database=work;User Id=your_username;Password=your_password;TrustServerCertificate=True;Connection Timeout=30;"
        //
        // Удаленный сервер:
        // "Server=your_server_name\\instance_name;Database=work;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;"
        //
        // С указанием порта:
        // "Server=localhost,1433;Database=work;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;"
    }
}

