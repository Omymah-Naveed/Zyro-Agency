using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Order
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Service { get; set; }

    public string? OrderDate { get; set; }

    public string? Status { get; set; }

    public string? Time { get; set; }

    public string? Date { get; set; }

    public string? MeetingText { get; set; }
}
