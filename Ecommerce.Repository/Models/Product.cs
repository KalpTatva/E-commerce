using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public int Stocks { get; set; }

    public int SellerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
}
