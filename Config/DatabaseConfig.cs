namespace Test1.Config
{
    /// <summary>
    /// Класс для настройки подключения к базе данных
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Строка подключения к базе данных SQL Server
        /// Измените параметры подключения в соответствии с вашей конфигурацией
        /// </summary>
        public static string ConnectionString { get; set; } = 
            "Server=localhost;Database=work;Integrated Security=True;TrustServerCertificate=True;";

        // Примеры других вариантов подключения:
        // "Server=localhost;Database=work;User Id=your_username;Password=your_password;TrustServerCertificate=True;"
        // "Server=your_server_name;Database=work;Integrated Security=True;TrustServerCertificate=True;"
    }
}

