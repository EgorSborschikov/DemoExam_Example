using System;
using System.Collections.Generic;

namespace DemoExamSolution.Entities;

public partial class OrderStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
