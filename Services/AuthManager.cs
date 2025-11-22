using System;
using System.Linq;
using Test1.Data;
using Test1.Models;
using Test1.Services;

namespace Test1
{
    /// <summary>
    /// Authentication manager to track user login state and manage database operations
    /// </summary>
    public static class AuthManager
    {
        private static bool _isLoggedIn = false;
        private static int? _currentUserId = null;
        private static string? _currentUser = null;
        private static string? _fullName = null;
        private static string? _email = null;
        private static string? _phone = null;

        public static bool IsLoggedIn => _isLoggedIn;
        public static int? CurrentUserId => _currentUserId;
        public static string CurrentUser => _currentUser ?? string.Empty;
        public static string FullName => _fullName ?? string.Empty;
        public static string Email => _email ?? string.Empty;
        public static string Phone => _phone ?? string.Empty;

        // Для обратной совместимости
        public static string FirstName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullName)) return string.Empty;
                var parts = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : string.Empty;
            }
        }

        public static string LastName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullName)) return string.Empty;
                var parts = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
            }
        }

        /// <summary>
        /// Авторизация пользователя по имени пользователя и паролю
        /// </summary>
        public static bool Login(string username, string password)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.CpUsers.FirstOrDefault(u => u.Username == username);
                    
                    if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
                    {
                        return false;
                    }

                    _isLoggedIn = true;
                    _currentUserId = user.UserId;
                    _currentUser = user.Username;
                    _fullName = user.FullName ?? string.Empty;
                    _email = user.Email;
                    _phone = user.Phone ?? string.Empty;
                    
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public static bool Register(string username, string email, string password, string? fullName = null, string? phone = null)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Проверяем, существует ли пользователь с таким именем или email
                    if (context.CpUsers.Any(u => u.Username == username || u.Email == email))
                    {
                        return false;
                    }

                    var newUser = new CpUser
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = PasswordHasher.HashPassword(password),
                        FullName = fullName,
                        Phone = phone,
                        Role = "customer"
                    };

                    context.CpUsers.Add(newUser);
                    context.SaveChanges();

                    // Автоматически входим после регистрации
                    _isLoggedIn = true;
                    _currentUserId = newUser.UserId;
                    _currentUser = newUser.Username;
                    _fullName = newUser.FullName ?? string.Empty;
                    _email = newUser.Email;
                    _phone = newUser.Phone ?? string.Empty;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        public static bool UpdateUser(string? fullName, string email, string? phone)
        {
            if (!_isLoggedIn || !_currentUserId.HasValue)
                return false;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.CpUsers.FirstOrDefault(u => u.UserId == _currentUserId.Value);
                    if (user == null)
                        return false;

                    // Проверяем, не занят ли email другим пользователем
                    if (context.CpUsers.Any(u => u.Email == email && u.UserId != _currentUserId.Value))
                        return false;

                    user.FullName = fullName;
                    user.Email = email;
                    user.Phone = phone;

                    context.SaveChanges();

                    // Обновляем локальные данные
                    _fullName = fullName;
                    _email = email;
                    _phone = phone;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Загрузка данных пользователя из БД
        /// </summary>
        public static void LoadUserData()
        {
            if (!_isLoggedIn || !_currentUserId.HasValue)
                return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.CpUsers.FirstOrDefault(u => u.UserId == _currentUserId.Value);
                    if (user != null)
                    {
                        _currentUser = user.Username;
                        _fullName = user.FullName ?? string.Empty;
                        _email = user.Email;
                        _phone = user.Phone ?? string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки при загрузке
            }
        }

        public static void Logout()
        {
            _isLoggedIn = false;
            _currentUserId = null;
            _currentUser = null;
            _fullName = null;
            _email = null;
            _phone = null;
        }
    }
}

