using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Project { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public string? Date { get; set; }

    public virtual ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
}
