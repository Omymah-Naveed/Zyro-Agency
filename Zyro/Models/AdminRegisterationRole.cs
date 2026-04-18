using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class AdminRegisterationRole
{
    public int Id { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<AdminRegisteration> AdminRegisterations { get; set; } = new List<AdminRegisteration>();
}
