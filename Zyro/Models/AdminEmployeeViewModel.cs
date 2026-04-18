namespace Zyro.Models
{
    public class AdminEmployeeViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? RoleName { get; set; }
        public string? Phone { get; set; }

        public string? Action { get; set; }

        public string? Age { get; set; }

        public string? City { get; set; }

        public string? ImagePath { get; set; }

        public List<AdminRegisteration> user { get; set; } = new List<AdminRegisteration>();
        public List<AdminRegisteration> UserList { get; set; }
    }
}
