using System;
using System.Collections.Generic;

namespace DemoExamSolution.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int OrderNumber { get; set; }

    public int IdProduct { get; set; }

    public DateOnly OrderDate { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int IdOrderDeliveryPlace { get; set; }

    public int IdClient { get; set; }

    public int Code { get; set; }

    public int IdOrderStatus { get; set; }

    public virtual User IdClientNavigation { get; set; } = null!;

    public virtual OrderDeliveryPlace IdOrderDeliveryPlaceNavigation { get; set; } = null!;

    public virtual OrderStatus IdOrderStatusNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;
}
