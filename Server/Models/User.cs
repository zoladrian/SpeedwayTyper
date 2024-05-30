using Microsoft.AspNetCore.Identity;

namespace SpeedwayTyperApp.Server.Models
{
    public class User : IdentityUser
    {
        public int TotalPoints { get; set; }
        public int AccurateMatchResults { get; set; }
    }
}
