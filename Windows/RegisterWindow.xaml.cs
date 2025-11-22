using System;
using System.Windows;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string phone = PhoneTextBox.Text.Trim();

            // Валидация
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Пожалуйста, введите имя пользователя");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Пожалуйста, введите email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите пароль");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return;
            }

            // Регистрация через БД
            bool success = AuthManager.Register(username, email, password, 
                null, // fullName больше не используется
                string.IsNullOrEmpty(phone) ? null : phone);

            if (success)
            {
                MessageBox.Show("Регистрация прошла успешно!", "Успешно", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Ошибка регистрации. Возможно, имя пользователя или email уже используются.");
            }
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            // Находим или создаем главное окно
            MainWindow? mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            
            if (mainWindow != null)
            {
                // Если главное окно уже открыто, активируем его
                mainWindow.Activate();
                mainWindow.Focus();
            }
            else
            {
                // Если главного окна нет, создаем новое
                mainWindow = new MainWindow();
                mainWindow.Show();
            }
            
            Close();
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

