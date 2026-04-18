using Zyro.Models;

namespace Zyro.Models
{
    public class ServiceViewModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Action { get; set; }

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public string? Sdescription { get; set; }

        public IFormFile Image { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Service { get; set; }

        public string? Date { get; set; }

        public string? Status { get; set; }
        public List<Service> servi { get; set; } = new List<Service>();
        public List<Service> ServiceList { get; set; }
        public List<Faq> FaqList { get; set; } = new();

        public List<Order> Order { get; set; } = new List<Order>();


    }

}
