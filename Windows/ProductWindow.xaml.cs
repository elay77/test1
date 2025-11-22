using System;
using System.Linq;
using System.Windows;
using Test1.Services;
// Временно закомментировано до генерации моделей через Scaffold-DbContext
// using Test1.Models;

namespace Test1
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        // private Product? _product; // Временно закомментировано

        // Конструктор для обратной совместимости (если где-то используется старый способ)
        public ProductWindow(string productName, string price, string description)
        {
            InitializeComponent();
            
            ProductNameText.Text = productName;
            PriceText.Text = price;
            DescriptionText.Text = description;
        }

        // Новый конструктор с объектом Product - временно отключен до Scaffold-DbContext
        /*
        public ProductWindow(Product product)
        {
            InitializeComponent();
            
            _product = product;
            
            // Устанавливаем информацию о товаре
            ProductNameText.Text = product.Name;
            PriceText.Text = $"{product.Price:N0} ₽";
            DescriptionText.Text = product.Description ?? "Описание отсутствует";
        }
        */

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is logged in
            if (!AuthManager.IsLoggedIn)
            {
                // Show message and open profile window for login
                MessageBox.Show(
                    "Для добавления товара в корзину необходимо войти в аккаунт.",
                    "Требуется вход",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Open profile window for login
                Profile profileWindow = new Profile();
                profileWindow.Show();
            }
            else
            {
                // Получаем данные о товаре
                string productName = ProductNameText.Text;
                string priceText = PriceText.Text.Replace(" ₽", "").Replace(" ", "");
                
                // Пытаемся распарсить цену
                if (decimal.TryParse(priceText, out decimal price))
                {
                    // Получаем количество из ComboBox
                    int quantity = 1;
                    if (QuantityComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                    {
                        if (int.TryParse(selectedItem.Content?.ToString(), out int parsedQuantity))
                        {
                            quantity = parsedQuantity;
                        }
                    }

                    // Получаем описание
                    string description = DescriptionText.Text;

                    // Добавляем товар в корзину
                    CartManager.AddToCart(productName, price, quantity, description);

                    MessageBox.Show(
                        $"Товар \"{productName}\" добавлен в корзину!",
                        "Успешно",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Не удалось определить цену товара.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
