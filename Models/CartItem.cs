namespace Test1.Models
{
    /// <summary>
    /// Модель товара в корзине
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public CartItem(int id, string name, decimal price, int quantity = 1, string description = "")
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            Description = description;
        }

        public decimal TotalPrice => Price * Quantity;
    }
}

