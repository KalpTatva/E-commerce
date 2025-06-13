using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Session
{
    public string SessionId { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    public string? Jwttoken { get; set; }
}
