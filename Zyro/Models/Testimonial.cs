using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Testimonial
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Remarks { get; set; }

    public string? Email { get; set; }

    public string? Action { get; set; }
}
