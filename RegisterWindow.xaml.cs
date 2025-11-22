using System;
using System.Windows;

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

