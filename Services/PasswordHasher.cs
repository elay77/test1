using System.Security.Cryptography;
using System.Text;

namespace Test1.Services
{
    /// <summary>
    /// Класс для хеширования и проверки паролей
    /// Использует алгоритм SHA256 для создания хешей паролей
    /// Пароли никогда не хранятся в открытом виде - только их хеши
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Хеширует пароль с использованием алгоритма SHA256
        /// Преобразует пароль в байты, вычисляет хеш и возвращает его в формате Base64
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <returns>Хеш пароля в формате Base64</returns>
        public static string HashPassword(string password)
        {
            // Создаем экземпляр SHA256 для хеширования
            using (SHA256 sha256 = SHA256.Create())
            {
                // Преобразуем пароль в байты в кодировке UTF-8
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                // Вычисляем хеш пароля
                byte[] hash = sha256.ComputeHash(bytes);
                // Преобразуем хеш в строку Base64 для хранения
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Проверяет, соответствует ли введенный пароль сохраненному хешу
        /// Хеширует введенный пароль и сравнивает с сохраненным хешем
        /// </summary>
        /// <param name="password">Пароль, введенный пользователем</param>
        /// <param name="hash">Хеш пароля, сохраненный в базе данных</param>
        /// <returns>true, если пароль соответствует хешу, иначе false</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            // Хешируем введенный пароль
            string passwordHash = HashPassword(password);
            // Сравниваем полученный хеш с сохраненным
            return passwordHash == hash;
        }
    }
}

