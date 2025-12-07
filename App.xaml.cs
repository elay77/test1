using System;
using System.Windows;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Главный класс приложения WPF
    /// Управляет жизненным циклом приложения и выполняет инициализацию при запуске
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Обработчик события запуска приложения
        /// Выполняется при старте приложения, до открытия первого окна
        /// Проверяет подключение к базе данных и открывает главное окно
        /// </summary>
        /// <param name="e">Аргументы события запуска</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Открываем главное окно приложения программно
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
