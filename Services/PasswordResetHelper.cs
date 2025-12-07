using System;
using System.Linq;
using Test1.Data;
using Test1.Models;

namespace Test1.Services
{
    /// <summary>
    /// Утилита для сброса паролей пользователей (для тестирования)
    /// </summary>
    public static class PasswordResetHelper
    {
        /// <summary>
        /// Устанавливает новый пароль для пользователя по имени пользователя
        /// </summary>
        public static bool ResetPassword(string username, string newPassword)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.CpUsers.FirstOrDefault(u => u.Username == username);
                    
                    if (user == null)
                    {
                        return false;
                    }

                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    context.SaveChanges();
                    
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Устанавливает новый пароль для пользователя по email
        /// </summary>
        public static bool ResetPasswordByEmail(string email, string newPassword)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var user = context.CpUsers.FirstOrDefault(u => u.Email == email);
                    
                    if (user == null)
                    {
                        return false;
                    }

                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    context.SaveChanges();
                    
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Устанавливает пароль "123456" для всех пользователей (только для тестирования!)
        /// </summary>
        public static void SetDefaultPasswordForAll(string defaultPassword = "123456")
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var users = context.CpUsers.ToList();
                    string hashedPassword = PasswordHasher.HashPassword(defaultPassword);
                    
                    foreach (var user in users)
                    {
                        user.PasswordHash = hashedPassword;
                    }
                    
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}

