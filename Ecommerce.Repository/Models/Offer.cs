using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Offer
{
    public int OfferId { get; set; }

    public int ProductId { get; set; }

    public int OfferType { get; set; }

    public decimal? DiscountRate { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
