using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Test1.Models;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Interaction logic for CartWindow.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        public CartWindow()
        {
            InitializeComponent();
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            CartItemsPanel.Children.Clear();

            var cartItems = CartManager.GetCartItems();

            if (cartItems.Count == 0)
            {
                // Показываем сообщение, если корзина пуста
                var emptyMessage = new TextBlock
                {
                    Text = "Ваша корзина пуста",
                    FontSize = 18,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                CartItemsPanel.Children.Add(emptyMessage);
            }
            else
            {
                // Отображаем товары
                foreach (var item in cartItems)
                {
                    var cartItemCard = CreateCartItemCard(item);
                    CartItemsPanel.Children.Add(cartItemCard);
                }
            }

            UpdateTotal();
        }

        private Border CreateCartItemCard(CartItem item)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                Margin = new Thickness(0, 0, 0, 16),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                BorderThickness = new Thickness(1)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Название товара
            var nameText = new TextBlock
            {
                Text = item.Name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            // Цена за единицу
            var priceText = new TextBlock
            {
                Text = $"{item.Price:N0} ₽",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 16, 0)
            };
            Grid.SetColumn(priceText, 1);
            grid.Children.Add(priceText);

            // Количество
            var quantityPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 16, 0)
            };

            var decreaseButton = new Button
            {
                Content = "-",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0),
                Tag = item.Id
            };
            decreaseButton.Click += DecreaseQuantityButton_Click;

            var quantityText = new TextBlock
            {
                Text = item.Quantity.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Width = 40,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var increaseButton = new Button
            {
                Content = "+",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(8, 0, 0, 0),
                Tag = item.Id
            };
            increaseButton.Click += IncreaseQuantityButton_Click;

            quantityPanel.Children.Add(decreaseButton);
            quantityPanel.Children.Add(quantityText);
            quantityPanel.Children.Add(increaseButton);
            quantityPanel.Tag = quantityText; // Сохраняем ссылку на TextBlock для обновления

            Grid.SetColumn(quantityPanel, 2);
            grid.Children.Add(quantityPanel);

            // Общая стоимость товара
            var totalText = new TextBlock
            {
                Text = $"{item.TotalPrice:N0} ₽",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 100,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(totalText, 3);
            grid.Children.Add(totalText);

            // Кнопка удаления
            var removeButton = new Button
            {
                Content = "✕",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(16, 0, 0, 0),
                Tag = item.Id
            };
            removeButton.Click += RemoveItemButton_Click;

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            mainGrid.Children.Add(grid);
            Grid.SetColumn(removeButton, 1);
            mainGrid.Children.Add(removeButton);

            border.Child = mainGrid;
            border.Tag = new { Item = item, QuantityText = quantityText, TotalText = totalText };

            return border;
        }

        private void DecreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                var item = CartManager.GetCartItems().FirstOrDefault(i => i.Id == itemId);
                if (item != null && item.Quantity > 1)
                {
                    CartManager.UpdateQuantity(itemId, item.Quantity - 1);
                    LoadCartItems();
                }
            }
        }

        private void IncreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                var item = CartManager.GetCartItems().FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    CartManager.UpdateQuantity(itemId, item.Quantity + 1);
                    LoadCartItems();
                }
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этот товар из корзины?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CartManager.RemoveFromCart(itemId);
                    LoadCartItems();
                }
            }
        }

        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите очистить корзину?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CartManager.ClearCart();
                LoadCartItems();
            }
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            var cartItems = CartManager.GetCartItems();
            
            if (cartItems.Count == 0)
            {
                MessageBox.Show(
                    "Корзина пуста. Добавьте товары перед оформлением заказа.",
                    "Корзина пуста",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!AuthManager.IsLoggedIn)
            {
                MessageBox.Show(
                    "Для оформления заказа необходимо войти в аккаунт.",
                    "Требуется вход",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Profile profileWindow = new Profile();
                profileWindow.Show();
                return;
            }

            // TODO: Здесь будет логика оформления заказа
            MessageBox.Show(
                $"Заказ на сумму {CartManager.GetTotalPrice():N0} ₽ успешно оформлен!",
                "Заказ оформлен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            CartManager.ClearCart();
            LoadCartItems();
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateTotal()
        {
            var totalPrice = CartManager.GetTotalPrice();
            var itemCount = CartManager.GetItemCount();

            TotalPriceText.Text = $"Итого: {totalPrice:N0} ₽";
            TotalItemsText.Text = $"Товаров: {itemCount}";
        }
    }
}

