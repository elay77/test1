using System.Collections.Generic;
using System.Linq;
using Test1.Models;

namespace Test1.Services
{
    /// <summary>
    /// Менеджер для управления корзиной покупок
    /// </summary>
    public static class CartManager
    {
        private static List<CartItem> _cartItems = new List<CartItem>();
        private static int _nextId = 1;

        /// <summary>
        /// Получить все товары в корзине
        /// </summary>
        public static List<CartItem> GetCartItems()
        {
            return _cartItems.ToList();
        }

        /// <summary>
        /// Добавить товар в корзину
        /// </summary>
        public static void AddToCart(string name, decimal price, int quantity = 1, string description = "")
        {
            // Проверяем, есть ли уже такой товар в корзине
            var existingItem = _cartItems.FirstOrDefault(item => item.Name == name && item.Price == price);
            
            if (existingItem != null)
            {
                // Если товар уже есть, увеличиваем количество
                existingItem.Quantity += quantity;
            }
            else
            {
                // Если товара нет, добавляем новый
                var newItem = new CartItem(_nextId++, name, price, quantity, description);
                _cartItems.Add(newItem);
            }
        }

        /// <summary>
        /// Удалить товар из корзины
        /// </summary>
        public static void RemoveFromCart(int id)
        {
            var item = _cartItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _cartItems.Remove(item);
            }
        }

        /// <summary>
        /// Обновить количество товара
        /// </summary>
        public static void UpdateQuantity(int id, int quantity)
        {
            var item = _cartItems.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(id);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
        }

        /// <summary>
        /// Очистить корзину
        /// </summary>
        public static void ClearCart()
        {
            _cartItems.Clear();
        }

        /// <summary>
        /// Получить общую стоимость корзины
        /// </summary>
        public static decimal GetTotalPrice()
        {
            return _cartItems.Sum(item => item.TotalPrice);
        }

        /// <summary>
        /// Получить количество товаров в корзине
        /// </summary>
        public static int GetItemCount()
        {
            return _cartItems.Sum(item => item.Quantity);
        }
    }
}

