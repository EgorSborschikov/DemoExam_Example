using System;
using System.Collections.Generic;

namespace DemoExamSolution.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
