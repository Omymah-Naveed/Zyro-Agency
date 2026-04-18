using Zyro.Models;

namespace Zyro.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? Age { get; set; }
        public string? Password { get; set; }

        public RegisterationUser? User { get; set; }
        public List<Blog> Blogs { get; set; } = new();
        public List<RegisterationUser> RegisterationUsers { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Testimonial> Tes { get; set; } = new();
        public List<Contact> Con { get; set; } = new();
    }
}
