using System;
using System.Text.RegularExpressions;
using System.Windows;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Окно сброса пароля
    /// Позволяет пользователю сбросить пароль по имени пользователя или email
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        /// <summary>
        /// Конструктор окна сброса пароля
        /// Инициализирует компоненты интерфейса
        /// </summary>
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сбросить пароль"
        /// Выполняет валидацию данных и сброс пароля пользователя
        /// </summary>
        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем введенные данные
            string usernameOrEmail = UsernameOrEmailTextBox.Text.Trim();
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Скрываем предыдущие сообщения перед новой проверкой
            ErrorMessage.Visibility = Visibility.Collapsed;
            SuccessMessage.Visibility = Visibility.Collapsed;

            // Валидация: проверяем, что все поля заполнены
            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                ShowError("Пожалуйста, введите имя пользователя или email");
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowError("Пожалуйста, введите новый пароль");
                return;
            }

            // Проверяем минимальную длину пароля
            if (newPassword.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return;
            }

            // Проверяем, что пароли совпадают
            if (newPassword != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            // Определяем, это email или имя пользователя
            bool isEmail = IsValidEmail(usernameOrEmail);
            bool success = false;

            if (isEmail)
            {
                // Если введен email, выполняем сброс по email
                success = PasswordResetHelper.ResetPasswordByEmail(usernameOrEmail, newPassword);
            }
            else
            {
                // Если введено имя пользователя, выполняем сброс по имени
                success = PasswordResetHelper.ResetPassword(usernameOrEmail, newPassword);
            }

            if (success)
            {
                // Если сброс успешен, показываем сообщение об успехе
                ShowSuccess("Пароль успешно изменен! Теперь вы можете войти с новым паролем.");
                
                // Очищаем поля ввода
                UsernameOrEmailTextBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();
                
                // Закрываем окно автоматически через 2 секунды после успешного сброса
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    this.DialogResult = true; // Указываем, что операция выполнена успешно
                    this.Close();
                };
                timer.Start();
            }
            else
            {
                // Если сброс не удался, показываем ошибку
                ShowError("Пользователь с таким именем или email не найден");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Отмена"
        /// Закрывает окно без выполнения сброса пароля
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Указываем, что операция не была выполнена
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
            SuccessMessage.Visibility = Visibility.Collapsed; // Скрываем сообщение об успехе
        }

        /// <summary>
        /// Показывает сообщение об успехе в интерфейсе
        /// </summary>
        /// <param name="message">Текст сообщения об успехе</param>
        private void ShowSuccess(string message)
        {
            SuccessMessage.Text = message;
            SuccessMessage.Visibility = Visibility.Visible;
            ErrorMessage.Visibility = Visibility.Collapsed; // Скрываем сообщение об ошибке
        }

        /// <summary>
        /// Проверяет, является ли строка валидным email адресом
        /// Использует простое регулярное выражение для проверки формата
        /// </summary>
        /// <param name="email">Строка для проверки</param>
        /// <returns>true, если строка является валидным email, иначе false</returns>
        private bool IsValidEmail(string email)
        {
            // Проверяем, что строка не пустая
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Простая проверка email через регулярное выражение
                // Проверяет наличие символов до @, после @ и точки с доменом
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                // В случае ошибки регулярного выражения возвращаем false
                return false;
            }
        }
    }
}

