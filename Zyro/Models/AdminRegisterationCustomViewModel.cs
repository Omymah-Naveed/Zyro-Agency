using System.Data;
using Zyro.Models;

namespace Zyro.Models
{
    public class AdminRegisterationCustomViewModel
    {
        public IEnumerable<AdminRegisterationRole>? RoleList { get; set; }
        public AdminRegisteration registrationFormData { get; set; }
    }
}
