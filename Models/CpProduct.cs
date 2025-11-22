using System;
using System.Collections.Generic;

namespace Test1.Models;

public partial class CpProduct
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public decimal? Rating { get; set; }

    public bool? IsActive { get; set; }

    public virtual CpCategory? Category { get; set; }

    public virtual ICollection<CpOrderItem> CpOrderItems { get; set; } = new List<CpOrderItem>();
}
