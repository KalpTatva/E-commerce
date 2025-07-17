using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string? Notification1 { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public int ProductId { get; set; }

    public virtual ICollection<UserNotificationMapping> UserNotificationMappings { get; set; } = new List<UserNotificationMapping>();
}
