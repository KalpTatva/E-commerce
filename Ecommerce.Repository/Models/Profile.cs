using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Profile
{
    public int ProfileId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Pincode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedById { get; set; }

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual State? State { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
