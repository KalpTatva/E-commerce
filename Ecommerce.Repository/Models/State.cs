using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class State
{
    public int StateId { get; set; }

    public string State1 { get; set; } = null!;

    public int? CountryId { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual Country? Country { get; set; }

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();
}
