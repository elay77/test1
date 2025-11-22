using System.Security.Cryptography;
using System.Text;

namespace Test1.Services
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Хеширует пароль с использованием SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Проверяет, соответствует ли пароль хешу
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}

