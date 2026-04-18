namespace Zyro.Models
{
    public class BlogDetailsViewModel
    {
        public Blog Blog { get; set; }
        public List<BlogCommentViewModel> Comments { get; set; }
        public List<BlogComment> Com { get; set; } 
        public string? Name { get; set; }
        public string? Email { get; set; }

    }

}
