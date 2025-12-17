using Server.Models;

namespace Server.Models.UserPermissions
{
    public class RolePermission : ModelBase
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
