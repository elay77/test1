using System;
using System.Collections.Generic;

namespace Test1.Models;

public partial class CpOrderItem
{
    public int ItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual CpOrder Order { get; set; } = null!;

    public virtual CpProduct Product { get; set; } = null!;
}
