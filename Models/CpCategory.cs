using System;
using System.Collections.Generic;

namespace Test1.Models;

public partial class CpCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CpProduct> CpProducts { get; set; } = new List<CpProduct>();
}
