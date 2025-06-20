using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? BuyerId { get; set; }

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public int TotalQuantity { get; set; }

    public decimal TotalDiscount { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
