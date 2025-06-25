using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Contactu
{
    public int ContactId { get; set; }

    public string SenderEmail { get; set; } = null!;

    public string ReciverEmail { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
