namespace Zyro.Models
{
    public class BlogViewModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Adminpick { get; set; }

        public string? Action { get; set; }
        public List<Blog> BlogList { get; set; }
        public List<BlogCommentViewModel> Comments { get; set; }

    }
}
