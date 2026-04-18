using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class RegisterationUser
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Age { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? Action { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
}
