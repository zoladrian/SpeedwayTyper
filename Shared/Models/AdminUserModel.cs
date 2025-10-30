using System.Collections.Generic;
using System.Linq;

namespace SpeedwayTyperApp.Shared.Models
{
    public class AdminUserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsPendingApproval { get; set; }
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
