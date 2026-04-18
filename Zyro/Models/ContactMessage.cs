using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class ContactMessage
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public string SenderType { get; set; } = null!;

    public string? SenderEmail { get; set; }

    public string MessageContent { get; set; } = null!;

    public DateTime? MessageDate { get; set; }

    public virtual Contact Contact { get; set; } = null!;
}
