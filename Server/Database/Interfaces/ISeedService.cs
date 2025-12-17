using Server.Database.Interfaces;

namespace Server.Database.Services
{
    public interface ISeedService
    {
        Task SeedAsync();
        Task SeedRolesAsync();
        Task SeedPermissionsAsync();
        Task SeedUsersAsync();
        Task AssignPermissionsToRolesAsync();
    }
}
