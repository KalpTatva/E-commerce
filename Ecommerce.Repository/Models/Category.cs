using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<GrantOfferPermission> GrantOfferPermissions { get; set; } = new List<GrantOfferPermission>();
}
