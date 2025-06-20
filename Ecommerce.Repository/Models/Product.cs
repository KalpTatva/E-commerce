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

    public int? DiscountType { get; set; }

    public decimal? Discount { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Feature> Features { get; set; } = new List<Feature>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
