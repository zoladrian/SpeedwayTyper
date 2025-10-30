using System.Collections.Generic;
using System.Linq;

namespace SpeedwayTyperApp.Shared.Models
{
    public class AdminUserModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsPendingApproval { get; set; }
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
