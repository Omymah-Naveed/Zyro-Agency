using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Meeting
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly MeetingDate { get; set; }

    public TimeOnly MeetingTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<MeetingMessage> MeetingMessages { get; set; } = new List<MeetingMessage>();

    public virtual Order Order { get; set; } = null!;
}
