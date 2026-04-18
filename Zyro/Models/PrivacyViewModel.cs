namespace Zyro.Models
{
    public class PrivacyViewModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ImagePath { get; set; }
        public string? Action { get; set; }

        public IFormFile Image { get; set; }

        public List<Privacy> Priv { get; set; } = new List<Privacy>();
        public List<Privacy> PrivacyList { get; set; }
    }
}
