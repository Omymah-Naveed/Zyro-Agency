using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class Blog
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Adminpick { get; set; }

    public string? Action { get; set; }

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
}
