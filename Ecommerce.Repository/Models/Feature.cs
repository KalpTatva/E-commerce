using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Feature
{
    public int FeatureId { get; set; }

    public string FeatureName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime? DeleteAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
