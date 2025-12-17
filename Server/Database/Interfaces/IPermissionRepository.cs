using Server.Models.UserPermissions;

namespace Server.Database.Interfaces
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId);
        Task<bool> PermissionNameExistsAsync(string name);
    }
}
