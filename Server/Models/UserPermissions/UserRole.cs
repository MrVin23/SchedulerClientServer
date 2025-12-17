using Server.Models;
using Server.Models.Users;

namespace Server.Models.UserPermissions
{
    public class UserRole : ModelBase
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
