# Настройка базы данных

## Инструкция по подключению к базе данных

### 1. Настройка строки подключения

Откройте файл `Config/DatabaseConfig.cs` и измените строку подключения в соответствии с вашей конфигурацией:

```csharp
public static string ConnectionString { get; set; } = 
    "Server=localhost;Database=ispp2109;Integrated Security=True;TrustServerCertificate=True;";
```

### Варианты строк подключения:

#### Windows Authentication (встроенная аутентификация):
```
Server=localhost;Database=ispp2109;Integrated Security=True;TrustServerCertificate=True;
```

#### SQL Server Authentication (с логином и паролем):
```
Server=localhost;Database=ispp2109;User Id=your_username;Password=your_password;TrustServerCertificate=True;
```

#### Удаленный сервер:
```
Server=your_server_name;Database=ispp2109;Integrated Security=True;TrustServerCertificate=True;
```

### 2. Создание базы данных

Убедитесь, что база данных `ispp2109` создана на вашем SQL Server. Если база данных еще не создана:

1. Откройте SQL Server Management Studio (SSMS)
2. Подключитесь к вашему серверу
3. Выполните команду:
   ```sql
   CREATE DATABASE ispp2109;
   ```

### 3. Создание таблиц

Выполните SQL скрипт из файла `script (3).sql` в вашей базе данных. Этот скрипт создаст все необходимые таблицы:
- `CP_Users` - пользователи
- `CP_Products` - товары
- `CP_Categories` - категории
- `CP_Orders` - заказы
- `CP_OrderItems` - элементы заказов

### 4. Проверка подключения

После настройки строки подключения запустите приложение. Если возникнут ошибки подключения, проверьте:
- Правильность имени сервера
- Правильность имени базы данных
- Наличие прав доступа к базе данных
- Запущен ли SQL Server

## Структура базы данных

### Таблица CP_Users
- `user_id` (int, PK, Identity) - идентификатор пользователя
- `username` (nvarchar(50), UNIQUE) - имя пользователя
- `email` (nvarchar(100), UNIQUE) - email
- `password_hash` (nvarchar(255)) - хеш пароля
- `full_name` (nvarchar(100)) - полное имя
- `phone` (nvarchar(20)) - телефон
- `role` (nvarchar(20), default: 'customer') - роль

### Таблица CP_Products
- `product_id` (int, PK, Identity) - идентификатор товара
- `name` (nvarchar(255)) - название
- `description` (nvarchar(max)) - описание
- `price` (decimal(10,2)) - цена
- `category_id` (int, FK) - категория
- `stock_quantity` (int, default: 0) - количество на складе
- `image_url` (nvarchar(512)) - URL изображения
- `rating` (decimal(2,1), default: 0.0) - рейтинг
- `is_active` (bit, default: 1) - активен ли товар

### Таблица CP_Categories
- `category_id` (int, PK, Identity) - идентификатор категории
- `name` (nvarchar(50)) - название
- `description` (nvarchar(255)) - описание
- `parent_category_id` (int, FK) - родительская категория
- `is_active` (bit, default: 1) - активна ли категория
- `created_at` (datetime2(7)) - дата создания

### Таблица CP_Orders
- `order_id` (int, PK, Identity) - идентификатор заказа
- `user_id` (int, FK) - пользователь
- `shipping_address` (nvarchar(255)) - адрес доставки
- `total_amount` (decimal(10,2)) - общая сумма
- `status` (nvarchar(20), default: 'pending') - статус
- `created_at` (datetime2(7)) - дата создания
- `updated_at` (datetime2(7)) - дата обновления

### Таблица CP_OrderItems
- `item_id` (int, PK, Identity) - идентификатор элемента
- `order_id` (int, FK) - заказ
- `product_id` (int, FK) - товар
- `quantity` (int, default: 1) - количество
- `unit_price` (decimal(10,2)) - цена за единицу

## Примечания

- Пароли хранятся в виде SHA256 хешей
- Все таблицы используют префикс `CP_` (вероятно, Customer Portal)
- При удалении пользователя удаляются все его заказы (CASCADE)
- При удалении заказа удаляются все его элементы (CASCADE)
- При удалении категории у товаров category_id устанавливается в NULL (SET NULL)

