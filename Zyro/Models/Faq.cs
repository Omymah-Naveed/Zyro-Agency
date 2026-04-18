using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Faq
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Action { get; set; }
}
