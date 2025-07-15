using System;
using System.Collections.Generic;

namespace Ecommerce.Repository.Models;

public partial class UserNotificationMapping
{
    public int MappingId { get; set; }

    public int UserId { get; set; }

    public int NotificationId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
