using System;
using System.Globalization;
using System.Windows;
using Test1.Data;
using Test1.Models;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Окно добавления нового товара (доступно только администратору)
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки выбора изображения
        /// </summary>
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Формируем целевой путь: Images/Products/имя_файла
                string sourcePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(sourcePath);
                
                // Целевая папка (рядом с .exe)
                string targetDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
                
                try
                {
                    System.IO.Directory.CreateDirectory(targetDir);
                    
                    string targetPath = System.IO.Path.Combine(targetDir, fileName);
                    
                    // Копируем файл
                    System.IO.File.Copy(sourcePath, targetPath, true);
                    
                    // В поле записываем относительный путь, который пойдет в БД
                    ImageUrlTextBox.Text = $"Images/Products/{fileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Сохранение нового товара в базе данных
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора на случай прямого вызова окна
            if (!AuthManager.IsLoggedIn || !AuthManager.IsAdmin)
            {
                MessageBox.Show(
                    "Добавление товаров доступно только администраторам.",
                    "Недостаточно прав",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string name = NameTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();
            string ratingText = RatingTextBox.Text.Trim();
            string imageUrl = ImageUrlTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Укажите название товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDecimal(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Укажите корректную цену больше нуля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal? rating = null;
            if (!string.IsNullOrWhiteSpace(ratingText))
            {
                if (TryParseDecimal(ratingText, out decimal parsedRating))
                {
                    if (parsedRating < 0 || parsedRating > 5)
                    {
                        MessageBox.Show("Рейтинг должен быть в диапазоне 0 - 5.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    rating = parsedRating;
                }
                else
                {
                    MessageBox.Show("Укажите корректный рейтинг (число от 0 до 5).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var product = new CpProduct
                    {
                        Name = name,
                        Description = string.IsNullOrWhiteSpace(description) ? null : description,
                        Price = price,
                        StockQuantity = 0, // По умолчанию 0, так как поле скрыто
                        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                        Rating = rating,
                        IsActive = true
                    };

                    context.CpProducts.Add(product);
                    context.SaveChanges();
                }

                MessageBox.Show("Товар успешно добавлен.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Закрывает окно без сохранения
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Парсит число с учетом текущей культуры и инвариантной (поддержка запятой и точки)
        /// </summary>
        private bool TryParseDecimal(string text, out decimal value)
        {
            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value)
                   || decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
}

