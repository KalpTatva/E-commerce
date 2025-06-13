using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class PasswordResetRequest
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public DateTime? Createdate { get; set; }

    public DateTime? Closedate { get; set; }

    public string Guidtoken { get; set; } = null!;
}
