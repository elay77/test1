using System;
using System.Windows;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Окно регистрации нового пользователя
    /// Позволяет создать новый аккаунт с валидацией данных
    /// </summary>
    public partial class RegisterWindow : Window
    {
        /// <summary>
        /// Конструктор окна регистрации
        /// Инициализирует компоненты интерфейса
        /// </summary>
        public RegisterWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Зарегистрироваться"
        /// Выполняет валидацию данных и регистрацию нового пользователя
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем введенные данные
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string phone = PhoneTextBox.Text.Trim();

            // Валидация: проверяем, что все обязательные поля заполнены
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

            // Проверяем, что пароли совпадают
            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            // Проверяем минимальную длину пароля
            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return;
            }

            // Выполняем регистрацию через базу данных
            bool success = AuthManager.Register(username, email, password, 
                null, // fullName не используется в текущей версии
                string.IsNullOrEmpty(phone) ? null : phone); // Телефон опционален

            if (success)
            {
                // Если регистрация успешна, показываем сообщение и закрываем окно
                MessageBox.Show("Регистрация прошла успешно!", "Успешно", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true; // Указываем, что регистрация прошла успешно
                Close();
            }
            else
            {
                // Если регистрация не удалась, показываем ошибку
                ShowError("Ошибка регистрации. Возможно, имя пользователя или email уже используются.");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Назад"
        /// Закрывает окно регистрации без создания аккаунта
        /// </summary>
        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно регистрации (оно модальное, поэтому просто закрываем)
            this.DialogResult = false; // Указываем, что регистрация не была выполнена
            this.Close();
        }

        /// <summary>
        /// Показывает сообщение об ошибке в интерфейсе
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке</param>
        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

