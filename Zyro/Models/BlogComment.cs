using System;
using System.Collections.Generic;

namespace Zyro.Models;

public partial class BlogComment
{
    public int Id { get; set; }

    public int BlogId { get; set; }

    public int UserId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CommentDate { get; set; }

    public int? ParentCommentId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual ICollection<BlogComment> InverseParentComment { get; set; } = new List<BlogComment>();

    public virtual BlogComment? ParentComment { get; set; }

    public virtual RegisterationUser User { get; set; } = null!;
}
