using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class MeetingMessage
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public string SenderType { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;
}
