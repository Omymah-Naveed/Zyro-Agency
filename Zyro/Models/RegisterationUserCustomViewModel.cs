using Zyro.Models;

using System.Data;
namespace Zyro.Models
{
    public class RegisterationUserCustomViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Age { get; set; }

        public string? City { get; set; }

        public string? Phone { get; set; }

        public string? Action { get; set; }

        public string? ImagePath { get; set; }
        public string? Status { get; set; }
        public List<RegisterationUser> user { get; set; } = new List<RegisterationUser>();
        public List<RegisterationUser> userList { get; set; }
        public RegisterationUser registerationFormData { get; set; }
    }
}
