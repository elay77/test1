using System.Collections.Generic;
using System.Linq;
using Test1.Models;

namespace Test1.Services
{
    /// <summary>
    /// Менеджер для управления корзиной покупок
    /// Управляет добавлением, удалением и изменением товаров в корзине
    /// Хранит корзину в памяти (в будущем можно сохранять в БД или локальное хранилище)
    /// </summary>
    public static class CartManager
    {
        // Список товаров в корзине
        private static List<CartItem> _cartItems = new List<CartItem>();
        // Счетчик для генерации уникальных ID товаров в корзине
        private static int _nextId = 1;

        /// <summary>
        /// Получить все товары в корзине
        /// Возвращает копию списка товаров
        /// </summary>
        /// <returns>Список всех товаров в корзине</returns>
        public static List<CartItem> GetCartItems()
        {
            return _cartItems.ToList(); // Возвращаем копию списка
        }

        /// <summary>
        /// Добавить товар в корзину
        /// Если товар с таким же названием и ценой уже есть, увеличивает его количество
        /// Иначе создает новый элемент корзины
        /// </summary>
        /// <param name="name">Название товара</param>
        /// <param name="price">Цена товара за единицу</param>
        /// <param name="quantity">Количество товара (по умолчанию 1)</param>
        /// <param name="description">Описание товара (необязательно)</param>
        public static void AddToCart(string name, decimal price, int quantity = 1, string description = "")
        {
            // Проверяем, есть ли уже такой товар в корзине (по названию и цене)
            var existingItem = _cartItems.FirstOrDefault(item => item.Name == name && item.Price == price);
            
            if (existingItem != null)
            {
                // Если товар уже есть в корзине, увеличиваем его количество
                existingItem.Quantity += quantity;
            }
            else
            {
                // Если товара нет в корзине, создаем новый элемент и добавляем его
                var newItem = new CartItem(_nextId++, name, price, quantity, description);
                _cartItems.Add(newItem);
            }
        }

        /// <summary>
        /// Удалить товар из корзины по его ID
        /// </summary>
        /// <param name="id">ID товара в корзине</param>
        public static void RemoveFromCart(int id)
        {
            // Находим товар по ID
            var item = _cartItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                // Удаляем товар из корзины
                _cartItems.Remove(item);
            }
        }

        /// <summary>
        /// Обновить количество товара в корзине
        /// Если количество становится 0 или меньше, товар удаляется из корзины
        /// </summary>
        /// <param name="id">ID товара в корзине</param>
        /// <param name="quantity">Новое количество товара</param>
        public static void UpdateQuantity(int id, int quantity)
        {
            // Находим товар по ID
            var item = _cartItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    // Если количество 0 или меньше, удаляем товар из корзины
                    RemoveFromCart(id);
                }
                else
                {
                    // Иначе обновляем количество
                    item.Quantity = quantity;
                }
            }
        }

        /// <summary>
        /// Очистить корзину - удалить все товары
        /// </summary>
        public static void ClearCart()
        {
            _cartItems.Clear();
        }

        /// <summary>
        /// Получить общую стоимость всех товаров в корзине
        /// Суммирует общую стоимость каждого товара (цена * количество)
        /// </summary>
        /// <returns>Общая стоимость корзины в рублях</returns>
        public static decimal GetTotalPrice()
        {
            return _cartItems.Sum(item => item.TotalPrice);
        }

        /// <summary>
        /// Получить общее количество товаров в корзине
        /// Суммирует количество всех товаров (не количество позиций, а общее количество единиц)
        /// </summary>
        /// <returns>Общее количество товаров в корзине</returns>
        public static int GetItemCount()
        {
            return _cartItems.Sum(item => item.Quantity);
        }
    }
}

