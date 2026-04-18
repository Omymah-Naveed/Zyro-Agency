using Zyro.Models;

namespace Zyro.Models
{
    public class OrderViewModel
    {

            public int Id { get; set; }

            public string? Name { get; set; }

            public string? Email { get; set; }

            public string? Service { get; set; }

            public string? Date { get; set; }
            public string? Time { get; set; }

        public string? Status { get; set; }
            public List<Order> OrdersList { get; set; }
        public string? MeetingText { get; set; }


    }
}
