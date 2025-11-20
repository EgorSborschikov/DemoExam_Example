using System;
using System.Collections.Generic;

namespace DemoExamSolution.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Articul { get; set; } = null!;

    public int IdProductType { get; set; }

    public string UnitOfMeasurement { get; set; } = null!;

    public decimal Price { get; set; }

    public int IdSupplier { get; set; }

    public int IdManufacturer { get; set; }

    public int IdCategory { get; set; }

    public int Discount { get; set; }

    public int QuantityInStock { get; set; }

    public string Description { get; set; } = null!;

    public string? PhotoPath { get; set; }

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual Manufacturer IdManufacturerNavigation { get; set; } = null!;

    public virtual ProductType IdProductTypeNavigation { get; set; } = null!;

    public virtual Supplier IdSupplierNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
