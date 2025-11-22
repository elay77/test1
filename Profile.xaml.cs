using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Test1
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public Profile()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (AuthManager.IsLoggedIn)
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                
                // Загружаем актуальные данные из БД
                AuthManager.LoadUserData();
                
                // Обновляем данные пользователя
                UserNameText.Text = AuthManager.FullName;
                UserEmailText.Text = AuthManager.Email;
                
                // В БД у нас full_name, но для обратной совместимости используем FirstName и LastName
                FirstNameTextBox.Text = AuthManager.FirstName;
                LastNameTextBox.Text = AuthManager.LastName;
                EmailTextBox.Text = AuthManager.Email;
                PhoneTextBox.Text = AuthManager.Phone;
                
                // Устанавливаем "Мои данные" как выбранный пункт по умолчанию
                SelectMenuItem(MyDataMenuItem);
            }
            else
            {
                LoginPanel.Visibility = Visibility.Visible;
            }
        }

        private void SelectMenuItem(Border menuItem)
        {
            // Сбрасываем все пункты меню
            MyDataMenuItem.Background = Brushes.Transparent;
            MyDataMenuItem.BorderBrush = Brushes.Transparent;
            MyDataMenuItem.BorderThickness = new Thickness(0);
            MyDataText.FontWeight = FontWeights.Normal;
            MyDataText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            
            MyOrdersMenuItem.Background = Brushes.Transparent;
            MyOrdersMenuItem.BorderBrush = Brushes.Transparent;
            MyOrdersMenuItem.BorderThickness = new Thickness(0);
            MyOrdersText.FontWeight = FontWeights.Normal;
            MyOrdersText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            
            // Выделяем выбранный пункт
            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
            menuItem.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            menuItem.BorderThickness = new Thickness(3, 0, 0, 0);
            
            // Обновляем стиль текста выбранного пункта
            if (menuItem == MyDataMenuItem)
            {
                MyDataText.FontWeight = FontWeights.SemiBold;
                MyDataText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
            else if (menuItem == MyOrdersMenuItem)
            {
                MyOrdersText.FontWeight = FontWeights.SemiBold;
                MyOrdersText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Пожалуйста, введите имя пользователя");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите пароль");
                return;
            }

            // Авторизация через БД
            bool success = AuthManager.Login(username, password);
            
            if (success)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
                UpdateUI();
            }
            else
            {
                ShowError("Неверное имя пользователя или пароль");
            }
        }

        private void LogoutMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AuthManager.Logout();
            UsernameTextBox.Clear();
            PasswordBox.Clear();
            UpdateUI();
        }

        private void MyDataMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenuItem(MyDataMenuItem);
        }

        private void MyOrdersMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenuItem(MyOrdersMenuItem);
            // Здесь можно добавить логику для отображения заказов
            MessageBox.Show("Раздел 'Мои заказы' будет реализован позже", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            // Валидация
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Объединяем имя и фамилию в full_name
            string? fullName = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                fullName = null;
            }

            // Обновляем данные в БД
            bool success = AuthManager.UpdateUser(fullName, email, string.IsNullOrEmpty(phone) ? null : phone);

            if (success)
            {
                MessageBox.Show("Данные успешно сохранены", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateUI();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении данных. Возможно, email уже используется другим пользователем.", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно регистрации
            RegisterWindow registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                // После успешной регистрации обновляем UI
                UpdateUI();
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
