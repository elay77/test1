using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Test1.Services;
// Временно закомментировано до генерации моделей
// using Test1.Data;
// using Test1.Models;

namespace Test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Временно отключено до генерации моделей через Scaffold-DbContext
            // LoadProducts();
        }

        private void LoadProducts()
        {
            // TODO: Раскомментировать после выполнения Scaffold-DbContext
            /*
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Загружаем только активные товары
                    var products = context.Products
                        .Where(p => p.IsActive == true)
                        .OrderBy(p => p.Name)
                        .ToList();

                    ProductsPanel.Children.Clear();

                    foreach (var product in products)
                    {
                        var productCard = CreateProductCard(product);
                        ProductsPanel.Children.Add(productCard);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке товаров: {ex.Message}\n\nПроверьте строку подключения к базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            */
        }

        private Border CreateProductCard(object product) // Временно object вместо Product
        {
            var border = new Border
            {
                Width = 200,
                Height = 280,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                Margin = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                Cursor = Cursors.Hand,
                Tag = product // Сохраняем объект Product в Tag
            };

            var stackPanel = new StackPanel
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Margin = new Thickness(12)
            };

            // TODO: Раскомментировать после Scaffold-DbContext
            /*
            var priceText = new TextBlock
            {
                Text = $"{product.Price:N0} ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var nameText = new TextBlock
            {
                Text = product.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                TextWrapping = TextWrapping.Wrap
            };
            */
            
            var priceText = new TextBlock
            {
                Text = "0 ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var nameText = new TextBlock
            {
                Text = "Товар",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(priceText);
            stackPanel.Children.Add(nameText);
            border.Child = stackPanel;

            border.MouseDown += ProductCard_MouseDown;

            return border;
        }

        private void ProductCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: Раскомментировать после выполнения Scaffold-DbContext
            /*
            if (sender is Border border && border.Tag is Product product)
            {
                // Открываем окно товара с информацией из базы данных
                ProductWindow productWindow = new ProductWindow(product);
                productWindow.Show();
            }
            */
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the profile window
            Profile profileWindow = new Profile();
            profileWindow.Show();
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем подключение к БД
            var result = TestConnection.TestDatabaseConnection();
            
            string message;
            MessageBoxImage icon;
            
            if (result)
            {
                message = "✓ Подключение к базе данных успешно!\n\n" +
                         "Все необходимые таблицы найдены.\n\n" +
                         "Теперь можно выполнить Scaffold-DbContext для генерации моделей.";
                icon = MessageBoxImage.Information;
            }
            else
            {
                message = "✗ Не удалось подключиться к базе данных или не найдены необходимые таблицы.\n\n" +
                         "Проверьте:\n" +
                         "1. Строку подключения в Config/DatabaseConfig.cs\n" +
                         "2. Что база данных work существует\n" +
                         "3. Что все таблицы созданы (CP_Users, CP_Products, CP_Categories, CP_Orders, CP_OrderItems)";
                icon = MessageBoxImage.Warning;
            }
            
            MessageBox.Show(message, "Проверка подключения к БД", MessageBoxButton.OK, icon);
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно корзины
            CartWindow cartWindow = new CartWindow();
            cartWindow.Show();
        }

        private void BrandFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем фильтр по бренду
            if (ProductFilter.GetCurrentFilter() == FilterType.Brand)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по бренду сброшен");
            }
            else
            {
                ProductFilter.SetFilter(FilterType.Brand);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Бренд\n\nПоказываются товары популярных брендов");
            }
            // TODO: После подключения БД здесь будет вызов LoadProducts() с применением фильтра
        }

        private void PriceFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем фильтр по цене
            if (ProductFilter.GetCurrentFilter() == FilterType.Price)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по цене сброшен");
            }
            else
            {
                ProductFilter.SetFilter(FilterType.Price);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Цена\n\nТовары отсортированы по возрастанию цены");
            }
            // TODO: После подключения БД здесь будет вызов LoadProducts() с применением фильтра
        }

        private void SaleFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем фильтр по акциям
            if (ProductFilter.GetCurrentFilter() == FilterType.Sale)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по акциям сброшен");
            }
            else
            {
                ProductFilter.SetFilter(FilterType.Sale);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Акции\n\nПоказываются только товары со скидками");
            }
            // TODO: После подключения БД здесь будет вызов LoadProducts() с применением фильтра
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ProductFilter.ClearFilter();
            UpdateFilterButtons();
            ShowFilterMessage("Все фильтры сброшены");
            // TODO: После подключения БД здесь будет вызов LoadProducts() без фильтров
        }

        private void UpdateFilterButtons()
        {
            var currentFilter = ProductFilter.GetCurrentFilter();
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            var activeTextColor = Brushes.White;
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
            var inactiveTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));

            // Сбрасываем все кнопки
            BrandFilterButton.Background = inactiveColor;
            BrandFilterButton.Foreground = inactiveTextColor;

            PriceFilterButton.Background = inactiveColor;
            PriceFilterButton.Foreground = inactiveTextColor;

            SaleFilterButton.Background = inactiveColor;
            SaleFilterButton.Foreground = inactiveTextColor;

            // Устанавливаем активную кнопку
            switch (currentFilter)
            {
                case FilterType.Brand:
                    BrandFilterButton.Background = activeColor;
                    BrandFilterButton.Foreground = activeTextColor;
                    break;
                case FilterType.Price:
                    PriceFilterButton.Background = activeColor;
                    PriceFilterButton.Foreground = activeTextColor;
                    break;
                case FilterType.Sale:
                    SaleFilterButton.Background = activeColor;
                    SaleFilterButton.Foreground = activeTextColor;
                    break;
            }

            // Показываем/скрываем кнопку сброса фильтров
            ClearFilterButton.Visibility = ProductFilter.IsFilterActive() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowFilterMessage(string message)
        {
            // Показываем сообщение о применении фильтра
            // В будущем можно заменить на более красивый уведомление
            // MessageBox.Show(message, "Фильтр", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Можно добавить поиск в реальном времени, но пока оставим только по кнопке/Enter
        }

        private void PerformSearch()
        {
            string searchQuery = SearchTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                ProductFilter.ClearSearchQuery();
                // TODO: После подключения БД здесь будет вызов LoadProducts() без поиска
                return;
            }

            ProductFilter.SetSearchQuery(searchQuery);
            
            // TODO: После подключения БД здесь будет вызов LoadProducts() с поисковым запросом
            // Пример:
            // var products = context.Products
            //     .Where(p => p.IsActive == true && p.Name.Contains(searchQuery))
            //     .ToList();
            
            // Временное сообщение для демонстрации
            MessageBox.Show(
                $"Поиск по запросу: \"{searchQuery}\"\n\nПосле подключения БД здесь будут отображаться найденные товары.",
                "Поиск",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}