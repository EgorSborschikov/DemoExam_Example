using System;
using System.Collections.Generic;

namespace DemoExamSolution.Entities;

public partial class Manufacturer
{
    public int Id { get; set; }

    public string ManufacturerName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
