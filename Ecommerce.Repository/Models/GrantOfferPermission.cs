using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class GrantOfferPermission
{
    public int GrantId { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
