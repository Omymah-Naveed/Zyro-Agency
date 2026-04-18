using System.Collections.Generic;

namespace Zyro.Models
{
    public class DashViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? ImagePath { get; set; }
        public List<Contact> Con { get; set; } = new List<Contact>();
        public List<AdminRegisteration> reg { get; set; } = new List<AdminRegisteration>();
        public List<Testimonial> Tes { get; set; } = new List<Testimonial>();
        public List<Taskk> Task { get; set; } = new List<Taskk>();

        // Add this property for input only
        public string NewTask { get; set; }
    }
}