namespace Zyro.Models
{
    public class IndexViewModel
    {
        public List<Service> ServiceList { get; set; } = new();
        public List<Faq> FaqList { get; set; } = new();
        public List<Testimonial> TestimonialList { get; set; } = new();
        public List<AboutU> AboutusList { get; set; } = new();
    }
}
