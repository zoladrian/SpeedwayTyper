using Microsoft.AspNetCore.Identity;

namespace SpeedwayTyperApp.Shared.Models
{
    public class User : IdentityUser
    {
        public int TotalPoints { get; set; }
        public int AccurateMatchResults { get; set; }
    }
}
