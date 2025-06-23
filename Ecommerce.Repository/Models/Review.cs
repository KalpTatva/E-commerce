using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int BuyerId { get; set; }

    public int ProductId { get; set; }

    public int OrderProductId { get; set; }

    public decimal? Ratings { get; set; }

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual OrderProduct OrderProduct { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
