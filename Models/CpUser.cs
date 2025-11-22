using System;
using System.Collections.Generic;

namespace Test1.Models;

public partial class CpUser
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<CpOrder> CpOrders { get; set; } = new List<CpOrder>();
}
