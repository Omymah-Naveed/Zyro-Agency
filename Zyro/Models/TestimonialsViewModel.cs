namespace Zyro.Models
{
    public class TestimonialsViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Remarks { get; set; }

        public string? Email { get; set; }
        public List<Testimonial> TesList { get; set; }

    }
}
