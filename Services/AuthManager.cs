using System;
using System.Linq;
using Test1.Data;
using Test1.Models;

namespace Test1.Services
{
    /// <summary>
    /// Менеджер аутентификации пользователей
    /// Отслеживает состояние входа пользователя и управляет операциями с базой данных
    /// Хранит информацию о текущем авторизованном пользователе
    /// </summary>
    public static class AuthManager
    {
        // Приватные поля для хранения состояния авторизации
        private static bool _isLoggedIn = false; // Флаг авторизации
        private static int? _currentUserId = null; // ID текущего пользователя
        private static string? _currentUser = null; // Имя пользователя
        private static string? _fullName = null; // Полное имя пользователя
        private static string? _email = null; // Email пользователя
        private static string? _phone = null; // Телефон пользователя
        private static string? _role = null; // Роль пользователя (admin / customer)

        // Публичные свойства для доступа к данным текущего пользователя
        public static bool IsLoggedIn => _isLoggedIn; // Проверка, авторизован ли пользователь
        public static int? CurrentUserId => _currentUserId; // ID текущего пользователя
        public static string CurrentUser => _currentUser ?? string.Empty; // Имя пользователя
        public static string FullName => _fullName ?? string.Empty; // Полное имя
        public static string Email => _email ?? string.Empty; // Email
        public static string Phone => _phone ?? string.Empty; // Телефон
        public static string Role => _role ?? string.Empty; // Роль пользователя
        public static bool IsAdmin => string.Equals(_role, "admin", StringComparison.OrdinalIgnoreCase);
        public static bool IsManager => string.Equals(_role, "manager", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Получает имя пользователя из полного имени
        /// Используется для обратной совместимости с интерфейсом, который разделяет имя и фамилию
        /// </summary>
        public static string FirstName
        {
            get
            {
                // Если полное имя пустое, возвращаем пустую строку
                if (string.IsNullOrEmpty(_fullName)) return string.Empty;
                // Разделяем полное имя по пробелам и берем первую часть
                var parts = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : string.Empty;
            }
        }

        /// <summary>
        /// Получает фамилию пользователя из полного имени
        /// Используется для обратной совместимости с интерфейсом, который разделяет имя и фамилию
        /// </summary>
        public static string LastName
        {
            get
            {
                // Если полное имя пустое, возвращаем пустую строку
                if (string.IsNullOrEmpty(_fullName)) return string.Empty;
                // Разделяем полное имя по пробелам и берем все части кроме первой (фамилия)
                var parts = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
            }
        }

        /// <summary>
        /// Авторизация пользователя по имени пользователя и паролю
        /// Проверяет учетные данные в базе данных и устанавливает состояние авторизации
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>true, если авторизация успешна, иначе false</returns>
        public static bool Login(string username, string password)
        {
            try
            {
                // Создаем контекст базы данных для работы с пользователями
                using (var context = new ApplicationDbContext())
                {
                    // Ищем пользователя по имени пользователя
                    var user = context.CpUsers.FirstOrDefault(u => u.Username == username);
                    
                    // Проверяем, существует ли пользователь и правильный ли пароль
                    if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
                    {
                        return false; // Пользователь не найден или пароль неверный
                    }

                    // Если все проверки пройдены, устанавливаем состояние авторизации
                    _isLoggedIn = true;
                    _currentUserId = user.UserId;
                    _currentUser = user.Username;
                    _fullName = user.FullName ?? string.Empty;
                    _email = user.Email;
                    _phone = user.Phone ?? string.Empty;
                    _role = string.IsNullOrWhiteSpace(user.Role) ? "customer" : user.Role;
                    
                    return true; // Авторизация успешна
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Регистрация нового пользователя в системе
        /// Создает нового пользователя в базе данных и автоматически выполняет вход
        /// </summary>
        /// <param name="username">Имя пользователя (должно быть уникальным)</param>
        /// <param name="email">Email пользователя (должен быть уникальным)</param>
        /// <param name="password">Пароль пользователя (будет захеширован)</param>
        /// <param name="fullName">Полное имя пользователя (необязательно)</param>
        /// <param name="phone">Телефон пользователя (необязательно)</param>
        /// <returns>true, если регистрация успешна, иначе false</returns>
        public static bool Register(string username, string email, string password, string? fullName = null, string? phone = null)
        {
            try
            {
                // Создаем контекст базы данных для работы с пользователями
                using (var context = new ApplicationDbContext())
                {
                    // Проверяем, не существует ли уже пользователь с таким именем или email
                    if (context.CpUsers.Any(u => u.Username == username || u.Email == email))
                    {
                        return false; // Пользователь с таким именем или email уже существует
                    }

                    // Создаем нового пользователя
                    var newUser = new CpUser
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = PasswordHasher.HashPassword(password), // Хешируем пароль перед сохранением
                        FullName = fullName,
                        Phone = phone,
                        Role = "customer" // По умолчанию роль "покупатель"
                    };

                    // Добавляем пользователя в базу данных
                    context.CpUsers.Add(newUser);
                    context.SaveChanges(); // Сохраняем изменения

                    // Автоматически выполняем вход после успешной регистрации
                    _isLoggedIn = true;
                    _currentUserId = newUser.UserId;
                    _currentUser = newUser.Username;
                    _fullName = newUser.FullName ?? string.Empty;
                    _email = newUser.Email;
                    _phone = newUser.Phone ?? string.Empty;
                    _role = newUser.Role ?? "customer";

                    return true; // Регистрация успешна
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Обновление данных текущего авторизованного пользователя
        /// Изменяет полное имя, email и телефон пользователя в базе данных
        /// </summary>
        /// <param name="fullName">Новое полное имя пользователя</param>
        /// <param name="email">Новый email (должен быть уникальным)</param>
        /// <param name="phone">Новый телефон пользователя</param>
        /// <returns>true, если обновление успешно, иначе false</returns>
        public static bool UpdateUser(string? fullName, string email, string? phone)
        {
            // Проверяем, что пользователь авторизован
            if (!_isLoggedIn || !_currentUserId.HasValue)
                return false;

            try
            {
                // Создаем контекст базы данных
                using (var context = new ApplicationDbContext())
                {
                    // Находим текущего пользователя по ID
                    var user = context.CpUsers.FirstOrDefault(u => u.UserId == _currentUserId.Value);
                    if (user == null)
                        return false; // Пользователь не найден

                    // Проверяем, не занят ли новый email другим пользователем
                    if (context.CpUsers.Any(u => u.Email == email && u.UserId != _currentUserId.Value))
                        return false; // Email уже используется другим пользователем

                    // Обновляем данные пользователя
                    user.FullName = fullName;
                    user.Email = email;
                    user.Phone = phone;

                    // Сохраняем изменения в базе данных
                    context.SaveChanges();

                    // Обновляем локальные данные для немедленного отображения
                    _fullName = fullName;
                    _email = email;
                    _phone = phone;

                    return true; // Обновление успешно
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Загружает актуальные данные пользователя из базы данных
        /// Используется для обновления локальных данных после изменений в БД
        /// </summary>
        public static void LoadUserData()
        {
            // Проверяем, что пользователь авторизован
            if (!_isLoggedIn || !_currentUserId.HasValue)
                return;

            try
            {
                // Создаем контекст базы данных
                using (var context = new ApplicationDbContext())
                {
                    // Находим пользователя по ID и обновляем локальные данные
                    var user = context.CpUsers.FirstOrDefault(u => u.UserId == _currentUserId.Value);
                    if (user != null)
                    {
                        _currentUser = user.Username;
                        _fullName = user.FullName ?? string.Empty;
                        _email = user.Email;
                        _phone = user.Phone ?? string.Empty;
                        _role = string.IsNullOrWhiteSpace(user.Role) ? "customer" : user.Role;
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки при загрузке, чтобы не нарушать работу приложения
            }
        }

        /// <summary>
        /// Выполняет выход пользователя из системы
        /// Очищает все данные о текущем пользователе
        /// </summary>
        public static void Logout()
        {
            // Сбрасываем все данные о текущем пользователе
            _isLoggedIn = false;
            _currentUserId = null;
            _currentUser = null;
            _fullName = null;
            _email = null;
            _phone = null;
            _role = null;
        }

        /// <summary>
        /// Изменение пароля текущего авторизованного пользователя
        /// Требует подтверждения старого пароля перед установкой нового
        /// </summary>
        /// <param name="oldPassword">Текущий пароль пользователя (для подтверждения)</param>
        /// <param name="newPassword">Новый пароль пользователя</param>
        /// <returns>true, если пароль успешно изменен, иначе false</returns>
        public static bool ChangePassword(string oldPassword, string newPassword)
        {
            // Проверяем, что пользователь авторизован
            if (!_isLoggedIn || !_currentUserId.HasValue)
                return false;

            try
            {
                // Создаем контекст базы данных
                using (var context = new ApplicationDbContext())
                {
                    // Находим текущего пользователя по ID
                    var user = context.CpUsers.FirstOrDefault(u => u.UserId == _currentUserId.Value);
                    if (user == null)
                        return false; // Пользователь не найден

                    // Проверяем, что старый пароль введен правильно
                    if (!PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
                    {
                        return false; // Старый пароль неверный
                    }

                    // Устанавливаем новый пароль (хешируем перед сохранением)
                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    context.SaveChanges(); // Сохраняем изменения в базе данных

                    return true; // Пароль успешно изменен
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем false
                return false;
            }
        }
    }
}

