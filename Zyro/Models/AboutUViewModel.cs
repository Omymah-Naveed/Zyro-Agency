namespace Zyro.Models
{
    public class AboutUViewModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? TitleTwo { get; set; }

        public string? DescriptionTwo { get; set; }
        public string? ImagePath { get; set; }
        public IFormFile Image { get; set; }

        public List<AboutU> AboutUsList { get; set; }

    }
}
