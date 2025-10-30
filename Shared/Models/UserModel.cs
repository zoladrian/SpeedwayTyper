using Microsoft.AspNetCore.Identity;

namespace SpeedwayTyperApp.Shared.Models
{
    public class UserModel : IdentityUser
    {
        public int TotalPoints { get; set; }
        public int AccurateMatchResults { get; set; }
        public bool IsPendingApproval { get; set; }
    }
}
