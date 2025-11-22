using System;
using System.Collections.Generic;

namespace Test1.Models;

public partial class CpOrder
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CpOrderItem> CpOrderItems { get; set; } = new List<CpOrderItem>();

    public virtual CpUser User { get; set; } = null!;
}
