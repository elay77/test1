using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.Win32;
using Test1.Services;
using Test1.Data;
using Test1.Models;

namespace Test1
{
    /// <summary>
    /// Главное окно приложения - каталог товаров
    /// Отображает список товаров из базы данных с возможностью поиска и фильтрации
    /// </summary>
    public partial class MainWindow : Window
    {
        private CpProduct? _selectedProduct;
        private List<OrderDisplay> _orderItems = new();
        private TextBlock? _cartEmptyText;

        /// <summary>
        /// Конструктор главного окна
        /// Инициализирует компоненты и загружает товары из базы данных
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            UpdateAdminControls();
            LoadProducts(); // Загружаем товары при открытии окна
        }

        /// <summary>
        /// Загружает товары из базы данных и отображает их в виде карточек
        /// Применяет поисковый запрос и фильтры, если они установлены
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                // Создаем контекст базы данных для работы с товарами
                using (var context = new ApplicationDbContext())
                {
                    // Начинаем с активных товаров (только те, у которых IsActive == true)
                    var productsQuery = context.CpProducts
                        .Where(p => p.IsActive == true);

                    // Применяем поиск, если пользователь ввел текст в поле поиска
                    string searchQuery = ProductFilter.GetSearchQuery();
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        // Ищем товары по названию или описанию
                        productsQuery = productsQuery.Where(p => 
                            p.Name.Contains(searchQuery) || 
                            (p.Description != null && p.Description.Contains(searchQuery)));
                    }

                    // Применяем фильтры в зависимости от выбранного типа
                    var currentFilter = ProductFilter.GetCurrentFilter();
                    switch (currentFilter)
                    {
                        case FilterType.Price:
                            // Сортируем товары по цене по возрастанию
                            productsQuery = productsQuery.OrderBy(p => p.Price);
                            break;
                        case FilterType.Brand:
                            // Фильтр по бренду - сортируем по названию (можно расширить для фильтрации по категории)
                            productsQuery = productsQuery.OrderBy(p => p.Name);
                            break;
                        case FilterType.Sale:
                            // Фильтр по рейтингу - показываем только товары с рейтингом 4.5 и выше
                            productsQuery = productsQuery.Where(p => p.Rating.HasValue && p.Rating.Value >= 4.5m);
                            break;
                        default:
                            // По умолчанию сортируем по названию
                            productsQuery = productsQuery.OrderBy(p => p.Name);
                            break;
                    }

                    // Выполняем запрос и получаем список товаров
                    var products = productsQuery.ToList();

                    // Очищаем панель товаров перед добавлением новых
                    ProductsPanel.Children.Clear();

                    if (products.Count == 0)
                    {
                        // Если товары не найдены, показываем сообщение
                        var emptyMessage = new TextBlock
                        {
                            Text = "Товары не найдены",
                            FontSize = 18,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        ProductsPanel.Children.Add(emptyMessage);
                    }
                    else
                    {
                        // Создаем карточку для каждого товара и добавляем в панель
                        foreach (var product in products)
                        {
                            var productCard = CreateProductCard(product);
                            ProductsPanel.Children.Add(productCard);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки показываем сообщение пользователю
                MessageBox.Show(
                    $"Ошибка при загрузке товаров: {ex.Message}\n\nПроверьте строку подключения к базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Создает визуальную карточку товара для отображения в каталоге
        /// Карточка содержит изображение, название, цену и рейтинг товара
        /// </summary>
        /// <param name="product">Объект товара из базы данных</param>
        /// <returns>Border элемент, представляющий карточку товара</returns>
        private Border CreateProductCard(CpProduct product)
        {
            // Создаем основной контейнер карточки
            var border = new Border
            {
                Width = 220, // Чуть шире
                Height = 320, // Чуть выше
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // Белый фон
                Margin = new Thickness(8),
                CornerRadius = new CornerRadius(12), // Большее скругление
                Cursor = Cursors.Hand,
                Tag = product,
                // Тень
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 2,
                    Opacity = 0.1,
                    BlurRadius = 4
                }
            };

            // Основная панель для размещения элементов карточки
            var mainStackPanel = new Grid();
            mainStackPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Картинка фиксированной высоты
            mainStackPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Текст занимает остальное

            // Область для изображения товара
            var imageBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")), // Светло-серый фон под картинкой
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                ClipToBounds = true
            };
            Grid.SetRow(imageBorder, 0);

            // Сетка для изображения и иконки
            var imageGrid = new Grid();
            var image = new Image
            {
                Stretch = Stretch.Uniform, // Картинка вписывается целиком
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var imageIcon = new TextBlock
            {
                Text = "📦",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"))
            };

            var resolvedPath = ResolveImagePath(product);
            if (TryLoadImage(resolvedPath, out var bitmap))
            {
                image.Source = bitmap;
                image.Visibility = Visibility.Visible;
                imageIcon.Visibility = Visibility.Collapsed;
            }

            imageGrid.Children.Add(image);
            imageGrid.Children.Add(imageIcon);
            imageBorder.Child = imageGrid;

            // Область с информацией о товаре
            var infoStackPanel = new StackPanel
            {
                Margin = new Thickness(16),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(infoStackPanel, 1);

            // Цена и Рейтинг в одной строке
            var topInfoGrid = new Grid();
            topInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Цена
            var priceText = new TextBlock
            {
                Text = $"{product.Price:N0} ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827"))
            };
            Grid.SetColumn(priceText, 0);
            topInfoGrid.Children.Add(priceText);

            // Рейтинг
            if (product.Rating.HasValue && product.Rating.Value > 0)
            {
                var ratingPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var ratingText = new TextBlock
                {
                    Text = $"★ {product.Rating.Value:F1}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                ratingPanel.Children.Add(ratingText);
                Grid.SetColumn(ratingPanel, 1);
                topInfoGrid.Children.Add(ratingPanel);
            }

            infoStackPanel.Children.Add(topInfoGrid);

            // Название товара
            var nameText = new TextBlock
            {
                Text = product.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B5563")),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 40,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 8, 0, 0)
            };
            infoStackPanel.Children.Add(nameText);

            // Собираем всё вместе
            mainStackPanel.Children.Add(imageBorder);
            mainStackPanel.Children.Add(infoStackPanel);
            border.Child = mainStackPanel;

            border.MouseDown += ProductCard_MouseDown;

            return border;
        }

        /// <summary>
        /// Обработчик клика на карточку товара
        /// Показывает детальную информацию в текущем окне (без создания нового окна)
        /// </summary>
        private void ProductCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что клик был по карточке товара
            if (sender is Border border && border.Tag is CpProduct product)
            {
                _selectedProduct = product;
                ShowProductDetail(product);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Профиль"
        /// Показывает профиль в рамках основного окна
        /// </summary>
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProfilePanel();
        }


        /// <summary>
        /// Обработчик нажатия кнопки "Корзина"
        /// Открывает окно корзины с товарами, добавленными пользователем
        /// </summary>
        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCart();
        }

        /// <summary>
        /// Показать детальную информацию о товаре в пределах текущего окна
        /// </summary>
        private void ShowProductDetail(CpProduct product)
        {
            // Заполняем данные
            DetailNameText.Text = product.Name;
            DetailPriceText.Text = $"{product.Price:N0} ₽";
            DetailDescriptionText.Text = product.Description ?? "Описание отсутствует";

            // Сброс количества к 1
            DetailQuantityTextBox.Text = "1";

            // Картинка / иконка
            var resolvedPath = ResolveImagePath(product);
            if (TryLoadImage(resolvedPath, out var bitmap))
            {
                DetailImage.Source = bitmap;
                DetailImage.Visibility = Visibility.Visible;
                DetailFallbackIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                DetailImage.Source = null;
                DetailImage.Visibility = Visibility.Collapsed;
                DetailFallbackIcon.Visibility = Visibility.Visible;
            }

            // Показываем панель деталей, скрываем каталог
            CatalogGrid.Visibility = Visibility.Collapsed;
            ProductDetailGrid.Visibility = Visibility.Visible;
            OrdersGrid.Visibility = Visibility.Collapsed;

            // Обновляем админ-элементы
            UpdateAdminControls();
            UpdateDetailAdminControls();
        }

        /// <summary>
        /// Возврат к каталогу без создания нового окна
        /// </summary>
        private void BackToCatalog()
        {
            ProductDetailGrid.Visibility = Visibility.Collapsed;
            OrdersGrid.Visibility = Visibility.Collapsed;
            CartGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Collapsed;
            CatalogGrid.Visibility = Visibility.Visible;
            _selectedProduct = null;
        }

        private void BackToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            BackToCatalog();
        }

        private void BackToCatalogFromProfileButton_Click(object sender, RoutedEventArgs e)
        {
            BackToCatalog();
        }

        /// <summary>
        /// Показывает панель профиля/входа в пределах главного окна
        /// </summary>
        private void ShowProfilePanel()
        {
            CatalogGrid.Visibility = Visibility.Collapsed;
            ProductDetailGrid.Visibility = Visibility.Collapsed;
            OrdersGrid.Visibility = Visibility.Collapsed;
            CartGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Visible;

            UpdateProfileUI();
        }

        private void UpdateProfileUI()
        {
            if (AuthManager.IsLoggedIn)
            {
                LoginPanel.Visibility = Visibility.Collapsed;

                AuthManager.LoadUserData();

                UserNameText.Text = AuthManager.FullName;
                UserEmailText.Text = AuthManager.Email;
                FirstNameTextBox.Text = AuthManager.FirstName;
                LastNameTextBox.Text = AuthManager.LastName;
                EmailTextBox.Text = AuthManager.Email;
                PhoneTextBox.Text = AuthManager.Phone;

                // Показываем кнопку управления заказами, если пользователь - админ или менеджер
                // (в текущей реализации проверяем только на админа, так как роли менеджера пока нет в AuthManager, но в БД роль "manager" может быть)
                // Добавим проверку на роль из AuthManager если бы она там была публичной, но пока проверим через IsAdmin
                // Если нужно расширить на менеджеров, нужно обновить AuthManager
                
                // Проверяем роль напрямую из БД или добавляем свойство в AuthManager
                // Пока используем IsAdmin как заглушку для "сотрудников"
                bool canManageOrders = AuthManager.IsAdmin || AuthManager.IsManager;
                // Если в AuthManager будет свойство Role, можно проверить: AuthManager.Role == "admin" || AuthManager.Role == "manager"
                
                ManageOrdersMenuItem.Visibility = canManageOrders ? Visibility.Visible : Visibility.Collapsed;

                SelectMenuItem(MyDataMenuItem);
            }
            else
            {
                LoginPanel.Visibility = Visibility.Visible;
                ErrorMessage.Visibility = Visibility.Collapsed;
                UsernameTextBox.Clear();
                PasswordBox.Clear();
                ManageOrdersMenuItem.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectMenuItem(Border menuItem)
        {
            // Сброс стилей всех пунктов
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

            ManageOrdersMenuItem.Background = Brushes.Transparent;
            ManageOrdersMenuItem.BorderBrush = Brushes.Transparent;
            ManageOrdersMenuItem.BorderThickness = new Thickness(0);
            ManageOrdersText.FontWeight = FontWeights.Normal;
            ManageOrdersText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

            // Выделение активного пункта
            menuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
            menuItem.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            menuItem.BorderThickness = new Thickness(3, 0, 0, 0);

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
            else if (menuItem == ManageOrdersMenuItem)
            {
                ManageOrdersText.FontWeight = FontWeights.SemiBold;
                ManageOrdersText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
            }
        }

        private void ManageOrdersMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenuItem(ManageOrdersMenuItem);
            ShowManageOrdersPanel();
        }

        private void ShowManageOrdersPanel()
        {
            LoadAllOrders();
            CatalogGrid.Visibility = Visibility.Collapsed;
            ProductDetailGrid.Visibility = Visibility.Collapsed;
            OrdersGrid.Visibility = Visibility.Collapsed;
            CartGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Collapsed;
            ManageOrdersGrid.Visibility = Visibility.Visible;

            // Настройка видимости кнопок в зависимости от роли
            // Менеджеры могут закрывать заказы, но не могут удалять их
            DeleteOrderButton.Visibility = AuthManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BackToProfileFromManageOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            ManageOrdersGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Visible;
            SelectMenuItem(MyDataMenuItem); // Возвращаемся на вкладку "Мои данные"
        }

        private void RefreshManageOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAllOrders();
        }

        private void LoadAllOrders()
        {
            ManageOrdersList.ItemsSource = null;
            ManageOrdersEmptyText.Visibility = Visibility.Collapsed;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Загружаем все заказы, включая информацию о пользователе
                    // Используем Include, но так как это старый EF Core, может потребоваться явная загрузка
                    // В данном проекте используем ленивую загрузку или явную выборку
                    
                    var orders = context.CpOrders
                        .OrderByDescending(o => o.CreatedAt)
                        .ToList(); // Сначала получаем список заказов

                    if (orders.Count == 0)
                    {
                        ManageOrdersEmptyText.Text = "Заказов нет";
                        ManageOrdersEmptyText.Visibility = Visibility.Visible;
                        return;
                    }

                    var orderDisplays = new List<AdminOrderDisplay>();

                    foreach (var order in orders)
                    {
                        // Загружаем пользователя
                        var user = context.CpUsers.FirstOrDefault(u => u.UserId == order.UserId);
                        string clientName = user != null ? (user.FullName ?? user.Username) : "Неизвестный";

                        // Загружаем товары заказа
                        var items = context.CpOrderItems.Where(i => i.OrderId == order.OrderId).ToList();
                        var productNames = new List<string>();
                        
                        foreach (var item in items)
                        {
                            var product = context.CpProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                            string pName = product?.Name ?? $"Товар #{item.ProductId}";
                            productNames.Add($"{pName} (x{item.Quantity})");
                        }

                        string productsSummary = string.Join(", ", productNames);
                        if (productsSummary.Length > 100) 
                            productsSummary = productsSummary.Substring(0, 97) + "...";

                        orderDisplays.Add(new AdminOrderDisplay
                        {
                            OrderId = order.OrderId,
                            ClientName = clientName,
                            CreatedAt = order.CreatedAt,
                            DateText = order.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "-",
                            TotalAmount = order.TotalAmount,
                            TotalAmountText = $"{order.TotalAmount:N0} ₽",
                            Status = order.Status ?? "new",
                            ProductsSummary = productsSummary
                        });
                    }

                    ManageOrdersList.ItemsSource = orderDisplays;
                }
            }
            catch (Exception ex)
            {
                ManageOrdersEmptyText.Text = $"Ошибка: {ex.Message}";
                ManageOrdersEmptyText.Visibility = Visibility.Visible;
            }
        }

        private class AdminOrderDisplay
        {
            public int OrderId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            public string DateText { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public string TotalAmountText { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string ProductsSummary { get; set; } = string.Empty;
        }

        private GridViewColumnHeader? _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void ManageOrdersListHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    string sortBy = header switch
                    {
                        "ID" => "OrderId",
                        "Клиент" => "ClientName",
                        "Дата" => "CreatedAt",
                        "Сумма" => "TotalAmount",
                        "Статус" => "Status",
                        "Товары" => "ProductsSummary",
                        _ => null
                    };

                    if (string.IsNullOrEmpty(sortBy)) return;

                    Sort(sortBy, direction);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ManageOrdersList.ItemsSource);
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        private void CloseOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ManageOrdersList.SelectedItem as AdminOrderDisplay;
            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для изменения статуса.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var order = context.CpOrders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                    if (order != null)
                    {
                        order.Status = "closed";
                        context.SaveChanges();
                        LoadAllOrders();
                        MessageBox.Show($"Заказ #{order.OrderId} закрыт.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ManageOrdersList.SelectedItem as AdminOrderDisplay;
            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для удаления.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите полностью удалить заказ #{selectedOrder.OrderId}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var order = context.CpOrders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                    if (order != null)
                    {
                        var items = context.CpOrderItems.Where(i => i.OrderId == order.OrderId);
                        context.CpOrderItems.RemoveRange(items);
                        
                        context.CpOrders.Remove(order);
                        context.SaveChanges();
                        LoadAllOrders();
                        MessageBox.Show($"Заказ #{order.OrderId} удален.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MyDataMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenuItem(MyDataMenuItem);
        }

        private void MyOrdersMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectMenuItem(MyOrdersMenuItem);
            ShowOrdersPanel();
        }

        private void LogoutMenuItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AuthManager.Logout();
            UsernameTextBox.Clear();
            PasswordBox.Clear();
            UpdateProfileUI();
            UpdateAdminControls();
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

            bool success = AuthManager.Login(username, password);

            if (success)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
                UpdateProfileUI();
                UpdateAdminControls();
                LoadProducts();
            }
            else
            {
                ShowError("Неверное имя пользователя или пароль");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                UpdateProfileUI();
                UpdateAdminControls();
                LoadProducts();
            }
        }

        private void ForgotPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? fullName = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = null;
            }

            bool success = AuthManager.UpdateUser(fullName, email, string.IsNullOrEmpty(phone) ? null : phone);

            if (success)
            {
                MessageBox.Show("Данные успешно сохранены", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateProfileUI();
                UpdateAdminControls();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении данных. Возможно, email уже используется другим пользователем.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenOrdersFromProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrdersPanel();
        }

        private void DetailAddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
                return;

            if (!int.TryParse(DetailQuantityTextBox.Text, out int quantity) || quantity < 1)
            {
                MessageBox.Show("Введите корректное количество (минимум 1).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (quantity > 999)
            {
                MessageBox.Show("Максимальное количество товара - 999.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CartManager.AddToCart(_selectedProduct.Name, _selectedProduct.Price, quantity, _selectedProduct.Description ?? string.Empty);

            MessageBox.Show(
                $"Товар \"{_selectedProduct.Name}\" добавлен в корзину!",
                "Успешно",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            UpdateCartTotalsIfVisible();
        }

        private void DetailDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAdmin || _selectedProduct == null)
                return;

            var confirm = MessageBox.Show(
                $"Удалить товар \"{_selectedProduct.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var dbProduct = context.CpProducts.FirstOrDefault(p => p.ProductId == _selectedProduct.ProductId);
                    if (dbProduct != null)
                    {
                        context.CpProducts.Remove(dbProduct);
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Товар удален.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToCatalog();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить товар" (для администратора)
        /// Открывает окно добавления товара и обновляет каталог после сохранения
        /// </summary>
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора
            if (!AuthManager.IsLoggedIn || !AuthManager.IsAdmin)
            {
                MessageBox.Show(
                    "Добавление товаров доступно только администраторам. Войдите под учетной записью администратора.",
                    "Недостаточно прав",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Показываем панель входа/профиля
                ShowProfilePanel();
                return;
            }

            // Открываем модальное окно добавления товара
            var addProductWindow = new AddProductWindow();
            bool? result = addProductWindow.ShowDialog();

            // Если товар успешно добавлен, обновляем список
            if (result == true)
            {
                LoadProducts();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Бренд"
        /// Переключает фильтр по бренду (включает/выключает)
        /// </summary>
        private void BrandFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Если фильтр уже активен, сбрасываем его
            if (ProductFilter.GetCurrentFilter() == FilterType.Brand)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по бренду сброшен");
            }
            else
            {
                // Иначе включаем фильтр по бренду
                ProductFilter.SetFilter(FilterType.Brand);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Бренд\n\nПоказываются товары популярных брендов");
            }
            // Перезагружаем товары с применением фильтра
            LoadProducts();
        }

        /// <summary>
        /// Показать корзину в пределах главного окна
        /// </summary>
        private void ShowCart()
        {
            LoadCartItems();
            CatalogGrid.Visibility = Visibility.Collapsed;
            ProductDetailGrid.Visibility = Visibility.Collapsed;
            OrdersGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Collapsed;
            CartGrid.Visibility = Visibility.Visible;
        }

        private void BackToCatalogFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            BackToCatalog();
        }

        /// <summary>
        /// Загружает товары из корзины и отображает их
        /// </summary>
        private void LoadCartItems()
        {
            CartItemsPanel.Children.Clear();
            var cartItems = CartManager.GetCartItems();

            if (cartItems.Count == 0)
            {
                _cartEmptyText = new TextBlock
                {
                    Text = "Ваша корзина пуста",
                    FontSize = 18,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                CartItemsPanel.Children.Add(_cartEmptyText);
            }
            else
            {
                foreach (var item in cartItems)
                {
                    var card = CreateCartItemCard(item);
                    CartItemsPanel.Children.Add(card);
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
            quantityPanel.Tag = quantityText;

            Grid.SetColumn(quantityPanel, 2);
            grid.Children.Add(quantityPanel);

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

                ShowProfilePanel();
                return;
            }

            try
            {
                SaveOrder(cartItems);

                MessageBox.Show(
                    $"Заказ на сумму {CartManager.GetTotalPrice():N0} ₽ успешно оформлен!",
                    "Заказ оформлен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CartManager.ClearCart();
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при оформлении заказа: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateTotal()
        {
            var totalPrice = CartManager.GetTotalPrice();
            var itemCount = CartManager.GetItemCount();

            TotalPriceText.Text = $"Итого: {totalPrice:N0} ₽";
            TotalItemsText.Text = $"Товаров: {itemCount}";
        }

        private void UpdateCartTotalsIfVisible()
        {
            if (CartGrid.Visibility == Visibility.Visible)
            {
                UpdateTotal();
            }
        }

        private void SaveOrder(List<CartItem> cartItems)
        {
            if (!AuthManager.IsLoggedIn || !AuthManager.CurrentUserId.HasValue)
                throw new InvalidOperationException("Пользователь не авторизован");

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Корзина пуста");

            using (var context = new ApplicationDbContext())
            {
                var order = new CpOrder
                {
                    UserId = AuthManager.CurrentUserId.Value,
                    ShippingAddress = "Не указан",
                    TotalAmount = CartManager.GetTotalPrice(),
                    Status = "created",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.CpOrders.Add(order);
                context.SaveChanges();

                foreach (var item in cartItems)
                {
                    var product = context.CpProducts.FirstOrDefault(p => p.Name == item.Name);
                    if (product == null)
                        continue;

                    var orderItem = new CpOrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    context.CpOrderItems.Add(orderItem);
                }

                context.SaveChanges();

                LogOrder(order, cartItems);
            }
        }

        private void LogOrder(CpOrder order, List<CartItem> cartItems)
        {
            try
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(logsDir);

                var logPath = Path.Combine(logsDir, "orders.log");

                var sb = new StringBuilder();
                sb.AppendLine("-----");
                sb.AppendLine($"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"OrderId: {order.OrderId}");
                sb.AppendLine($"UserId: {order.UserId}");
                sb.AppendLine($"Итого: {order.TotalAmount:N2} ₽");
                sb.AppendLine("Товары:");
                foreach (var item in cartItems)
                {
                    sb.AppendLine($" - {item.Name} x{item.Quantity} по {item.Price:N2} ₽ = {item.TotalPrice:N2} ₽");
                }
                sb.AppendLine();

                File.AppendAllText(logPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Order logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Цена"
        /// Переключает фильтр по цене (сортирует товары по возрастанию цены)
        /// </summary>
        private void PriceFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Если фильтр уже активен, сбрасываем его
            if (ProductFilter.GetCurrentFilter() == FilterType.Price)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по цене сброшен");
            }
            else
            {
                // Иначе включаем фильтр по цене
                ProductFilter.SetFilter(FilterType.Price);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Цена\n\nТовары отсортированы по возрастанию цены");
            }
            // Перезагружаем товары с применением фильтра
            LoadProducts();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Рейтинг"
        /// Переключает фильтр по рейтингу (показывает товары с рейтингом 4.5 и выше)
        /// </summary>
        private void SaleFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Если фильтр уже активен, сбрасываем его
            if (ProductFilter.GetCurrentFilter() == FilterType.Sale)
            {
                ProductFilter.ClearFilter();
                UpdateFilterButtons();
                ShowFilterMessage("Фильтр по рейтингу сброшен");
            }
            else
            {
                // Иначе включаем фильтр по рейтингу
                ProductFilter.SetFilter(FilterType.Sale);
                UpdateFilterButtons();
                ShowFilterMessage("Применен фильтр: Рейтинг\n\nПоказываются товары с рейтингом 4.5 и выше");
            }
            // Перезагружаем товары с применением фильтра
            LoadProducts();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сбросить фильтры"
        /// Сбрасывает все активные фильтры и показывает все товары
        /// </summary>
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ProductFilter.ClearFilter();
            UpdateFilterButtons();
            ShowFilterMessage("Все фильтры сброшены");
            // Перезагружаем товары без фильтров
            LoadProducts();
        }

        /// <summary>
        /// Обновляет визуальное состояние кнопок фильтров
        /// Выделяет активный фильтр и скрывает/показывает кнопку сброса
        /// </summary>
        private void UpdateFilterButtons()
        {
            var currentFilter = ProductFilter.GetCurrentFilter();
            // Цвета для активной и неактивной кнопок
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            var activeTextColor = Brushes.White;
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6"));
            var inactiveTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));

            // Сбрасываем все кнопки к неактивному состоянию
            BrandFilterButton.Background = inactiveColor;
            BrandFilterButton.Foreground = inactiveTextColor;

            PriceFilterButton.Background = inactiveColor;
            PriceFilterButton.Foreground = inactiveTextColor;

            SaleFilterButton.Background = inactiveColor;
            SaleFilterButton.Foreground = inactiveTextColor;

            // Выделяем активную кнопку в зависимости от текущего фильтра
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

            // Показываем кнопку сброса фильтров только если какой-то фильтр активен
            ClearFilterButton.Visibility = ProductFilter.IsFilterActive() ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Обновляет доступность элементов интерфейса, зависящих от роли пользователя
        /// Например, кнопка добавления товара видна только администратору
        /// </summary>
        private void UpdateAdminControls()
        {
            // Элементы управления товарами (добавление, импорт, экспорт) видны только админу
            AdminProductControls.Visibility = AuthManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            
            // Кнопка добавления внутри панели (для совместимости, если она используется отдельно)
            AddProductButton.Visibility = AuthManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateDetailAdminControls()
        {
            DetailDeleteButton.Visibility = AuthManager.IsAdmin && _selectedProduct != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Обработчик кнопки "Мои заказы" (открывает панель в том же окне)
        /// </summary>
        public void ShowOrdersPanel()
        {
            LoadOrders();
            CatalogGrid.Visibility = Visibility.Collapsed;
            ProductDetailGrid.Visibility = Visibility.Collapsed;
            CartGrid.Visibility = Visibility.Collapsed;
            ProfileGrid.Visibility = Visibility.Collapsed;
            OrdersGrid.Visibility = Visibility.Visible;
        }

        private void BackFromOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            OrdersGrid.Visibility = Visibility.Collapsed;
            CatalogGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Загружает последние заказы текущего пользователя
        /// </summary>
        private void LoadOrders()
        {
            _orderItems.Clear();
            OrdersList.ItemsSource = null;
            OrdersEmptyText.Visibility = Visibility.Collapsed;

            if (!AuthManager.IsLoggedIn || !AuthManager.CurrentUserId.HasValue)
            {
                OrdersEmptyText.Text = "Необходимо войти в аккаунт";
                OrdersEmptyText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var userId = AuthManager.CurrentUserId.Value;
                    var orders = context.CpOrders
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.CreatedAt)
                        .Take(30)
                        .ToList();

                    if (orders.Count == 0)
                    {
                        OrdersEmptyText.Text = "Заказов пока нет";
                        OrdersEmptyText.Visibility = Visibility.Visible;
                        return;
                    }

                    foreach (var order in orders)
                    {
                        var items = context.CpOrderItems.Where(i => i.OrderId == order.OrderId).ToList();
                        if (items.Count == 0)
                        {
                            _orderItems.Add(new OrderDisplay
                            {
                                ProductName = "(пустой заказ)",
                                PriceText = $"{order.TotalAmount:N0} ₽",
                                DateText = order.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "неизвестно"
                            });
                            continue;
                        }

                        foreach (var item in items)
                        {
                            var product = context.CpProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                            _orderItems.Add(new OrderDisplay
                            {
                                ProductName = product?.Name ?? $"Товар #{item.ProductId}",
                                PriceText = $"{item.UnitPrice * item.Quantity:N0} ₽",
                                DateText = order.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "неизвестно"
                            });
                        }
                    }

                    OrdersList.ItemsSource = _orderItems;
                }
            }
            catch (Exception ex)
            {
                OrdersEmptyText.Text = $"Ошибка загрузки заказов: {ex.Message}";
                OrdersEmptyText.Visibility = Visibility.Visible;
            }
        }

        private class OrderDisplay
        {
            public string ProductName { get; set; } = string.Empty;
            public string PriceText { get; set; } = string.Empty;
            public string DateText { get; set; } = string.Empty;
        }

        /// <summary>
        /// Возвращает путь к изображению для товара.
        /// Если ImageUrl пуст, для товара с названием, содержащим "мыло", подставляем наше изображение по умолчанию.
        /// </summary>
        private string? ResolveImagePath(CpProduct product)
        {
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                return product.ImageUrl;

            if (!string.IsNullOrWhiteSpace(product.Name) &&
                product.Name.IndexOf("мыло", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Images/Products/soap.png";
            }

            return null;
        }

        /// <summary>
        /// Пытается загрузить картинку по пути из ImageUrl
        /// Поддерживает относительные и абсолютные пути. Возвращает BitmapImage.
        /// </summary>
        private bool TryLoadImage(string? imageUrl, out BitmapImage bitmap)
        {
            bitmap = null!;
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            try
            {
                // Нормализуем слеши
                imageUrl = imageUrl.Replace("/", Path.DirectorySeparatorChar.ToString())
                                   .Replace("\\", Path.DirectorySeparatorChar.ToString());

                string path = imageUrl;
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(AppContext.BaseDirectory, imageUrl);
                }

                // Логирование для отладки
                Debug.WriteLine($"[ImageLoad] Request: '{imageUrl}' -> FullPath: '{path}'. Exists: {File.Exists(path)}");

                if (!File.Exists(path))
                    return false;

                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(path, UriKind.Absolute);
                img.EndInit();
                img.Freeze(); // не держим файл заблокированным

                bitmap = img;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageLoad] Error loading '{imageUrl}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Показывает сообщение о применении фильтра
        /// В данный момент не используется, но может быть использовано для уведомлений
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        private void ShowFilterMessage(string message)
        {
            // Показываем сообщение о применении фильтра
            // В будущем можно заменить на более красивое уведомление
            // MessageBox.Show(message, "Фильтр", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private System.Windows.Threading.DispatcherTimer? _searchTimer;

        private void InitializeSearchTimer()
        {
            if (_searchTimer == null)
            {
                _searchTimer = new System.Windows.Threading.DispatcherTimer();
                _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
                _searchTimer.Tick += (s, args) =>
                {
                    _searchTimer.Stop();
                    PerformSearch();
                };
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// Реализует поиск в реальном времени с задержкой 500мс для оптимизации запросов к БД
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_searchTimer == null)
            {
                InitializeSearchTimer();
            }

            _searchTimer?.Stop();
            _searchTimer?.Start();
        }

        /// <summary>
        /// Выполняет поиск товаров по введенному запросу
        /// Очищает поиск, если запрос пустой, и перезагружает товары
        /// </summary>
        private void PerformSearch()
        {
            string searchQuery = SearchTextBox.Text.Trim();
            
            // Если поисковый запрос пустой, очищаем фильтр и показываем все товары
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                ProductFilter.ClearSearchQuery();
                LoadProducts();
                return;
            }

            // Устанавливаем поисковый запрос и перезагружаем товары
            ProductFilter.SetSearchQuery(searchQuery);
            LoadProducts();
        }

        // --- Экспорт и Импорт (Excel / CSV) ---

        private void ExportOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV файл (Excel) (*.csv)|*.csv",
                    FileName = $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();
                    // Заголовок CSV (разделитель - точка с запятой, стандарт для Excel в РФ)
                    sb.AppendLine("ID Заказа;Клиент;Дата;Сумма;Статус;Товары");

                    // Получаем данные из списка (предполагаем, что они уже загружены)
                    if (ManageOrdersList.ItemsSource is IEnumerable<AdminOrderDisplay> orders)
                    {
                        foreach (var order in orders)
                        {
                            // Экранируем точки с запятой в данных, чтобы не сломать CSV
                            string products = order.ProductsSummary.Replace(";", ",");
                            string client = order.ClientName.Replace(";", ",");
                            
                            sb.AppendLine($"{order.OrderId};{client};{order.DateText};{order.TotalAmount:F2};{order.Status};{products}");
                        }
                    }

                    // Сохраняем с кодировкой UTF8 + BOM (чтобы Excel правильно читал кириллицу)
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Заказы успешно экспортированы!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportProductsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV файл (Excel) (*.csv)|*.csv",
                    FileName = $"Products_Export_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var products = context.CpProducts.ToList();
                        var sb = new StringBuilder();
                        
                        // Заголовок
                        sb.AppendLine("ID;Название;Цена;Описание;Изображение;Рейтинг;Активен");

                        foreach (var p in products)
                        {
                            string name = p.Name?.Replace(";", ",") ?? "";
                            string desc = p.Description?.Replace(";", ",").Replace("\n", " ").Replace("\r", "") ?? "";
                            string img = p.ImageUrl ?? "";
                            
                            sb.AppendLine($"{p.ProductId};{name};{p.Price:F2};{desc};{img};{p.Rating:F1};{p.IsActive}");
                        }

                        File.WriteAllText(saveFileDialog.FileName, sb.ToString(), new UTF8Encoding(true));
                        MessageBox.Show($"Экспортировано {products.Count} товаров!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthManager.IsAdmin) return;

            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV файл (Excel) (*.csv)|*.csv",
                    Title = "Выберите файл для импорта товаров"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    if (lines.Length < 2)
                    {
                        MessageBox.Show("Файл пуст или содержит только заголовок.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int addedCount = 0;
                    using (var context = new ApplicationDbContext())
                    {
                        // Пропускаем первую строку (заголовок)
                        for (int i = 1; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = line.Split(';');
                            // Ожидаем минимум: Название;Цена (остальное опционально, но формат должен соблюдаться)
                            // Формат ожидаемый: ID;Название;Цена;Описание;Изображение;Рейтинг;Активен
                            // Если ID = 0 или пустой - создаем новый. Если ID есть - обновляем (опционально, здесь сделаем просто добавление новых для безопасности)
                            
                            if (parts.Length < 3) continue;

                            // Индексы из ExportProductsButton_Click:
                            // 0:ID, 1:Name, 2:Price, 3:Desc, 4:Img, 5:Rating, 6:Active
                            
                            string name = parts[1].Trim();
                            if (string.IsNullOrEmpty(name)) continue;

                            if (!decimal.TryParse(parts[2], out decimal price)) continue;
                            
                            string description = parts.Length > 3 ? parts[3] : null;
                            string imageUrl = parts.Length > 4 ? parts[4] : null;
                            
                            decimal? rating = null;
                            if (parts.Length > 5 && decimal.TryParse(parts[5], out decimal r)) rating = r;
                            
                            bool isActive = true;
                            if (parts.Length > 6 && bool.TryParse(parts[6], out bool active)) isActive = active;

                            var newProduct = new CpProduct
                            {
                                Name = name,
                                Price = price,
                                Description = description,
                                ImageUrl = imageUrl,
                                Rating = rating,
                                IsActive = isActive,
                                StockQuantity = 100 // Значение по умолчанию
                            };

                            context.CpProducts.Add(newProduct);
                            addedCount++;
                        }
                        
                        context.SaveChanges();
                    }

                    LoadProducts();
                    MessageBox.Show($"Успешно импортировано {addedCount} товаров!", "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}\nУбедитесь, что формат файла CSV правильный (разделитель ';').", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}