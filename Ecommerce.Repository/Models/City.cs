using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class City
{
    public int CityId { get; set; }

    public string City1 { get; set; } = null!;

    public int? StateId { get; set; }

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();

    public virtual State? State { get; set; }
}
