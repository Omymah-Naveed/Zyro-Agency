namespace Zyro.Models
{
    public class BlogListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }   // Author name
        public string? Email { get; set; }   // Author name
        public string? Action { get; set; }   // Author name
        public string? Adminpick { get; set; }   // Author name
        public DateTime? CreatedDate { get; set; }
        public int CommentCount { get; set; }
        public DateTime? CommentDate { get; set; }
        public List<BlogCommentViewModel> Replies { get; set; } = new List<BlogCommentViewModel>();

    }
}
