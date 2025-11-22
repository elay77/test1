using System;
using System.Windows;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Проверяем подключение к базе данных при запуске
            bool connectionOk = Services.TestConnection.TestDatabaseConnection();
            
            if (!connectionOk)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных или не найдены необходимые таблицы.\n\n" +
                    "Проверьте:\n" +
                    "1. Строку подключения в Config/DatabaseConfig.cs\n" +
                    "2. Что база данных work существует\n" +
                    "3. Что все таблицы созданы (CP_Users, CP_Products, CP_Categories, CP_Orders, CP_OrderItems)\n\n" +
                    "Приложение будет работать, но данные не будут загружаться из БД.",
                    "Предупреждение о подключении",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            
            // Открываем главное окно программно
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
