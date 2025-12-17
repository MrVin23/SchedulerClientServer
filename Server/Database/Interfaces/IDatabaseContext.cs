using Microsoft.EntityFrameworkCore;
using Server.Models.Users;
using Server.Models.UserPermissions;
using Server.Models.Events;

namespace Server.Database.Interfaces
{
    public interface IDatabaseContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<Permission> Permissions { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<RolePermission> RolePermissions { get; set; }
        DbSet<EventTypesModel> EventTypes { get; set; }
        DbSet<EventsModel> Events { get; set; }
        DbSet<UserEventsModel> UserEvents { get; set; }
        DbSet<EventSettingsModel> EventSettings { get; set; }
        
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}