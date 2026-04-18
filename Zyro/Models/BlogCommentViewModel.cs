namespace Zyro.Models
{
    public class BlogCommentViewModel
    {
        public BlogComment Blog { get; set; }

        public int Id { get; set; }
        public int BlogId { get; set; }
        public string UserName { get; set; }
        public string CommentText { get; set; }
        public DateTime? CommentDate { get; set; }
        public int? ParentCommentId { get; set; }
        public List<BlogCommentViewModel> Replies { get; set; } = new List<BlogCommentViewModel>();

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Adminpick { get; set; }

        public string? Action { get; set; }

    }

}
