using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Database.Services;
using Server.Database.Interfaces;
using Server.Database.Repositories;

namespace Server.Database.Services
{
    /// <summary>
    /// Standalone seeding utility for developers
    /// Usage: dotnet run --project Server -- --seed-standalone
    /// </summary>
    public class StandaloneSeeder
    {
        public static async Task SeedDatabaseAsync(string[] args)
        {
            if (!args.Contains("--seed-standalone"))
            {
                Console.WriteLine("Usage: dotnet run --project Server -- --seed-standalone");
                return;
            }

            // Create a simple host to run seeding
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add database context
                    services.AddDbContext<DatabaseContext>(options =>
                        options.UseNpgsql("Host=localhost;Database=template-db;Username=template-user;Password=Tb2024cP"));

                    // Add repositories
                    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IRoleRepository, RoleRepository>();
                    services.AddScoped<IPermissionRepository, PermissionRepository>();
                    services.AddScoped<IUserRoleRepository, UserRoleRepository>();
                    services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

                    // Add seed service
                    services.AddScoped<ISeedService, SeedService>();
                })
                .Build();

            try
            {
                Console.WriteLine("Starting database seeding...");
                
                using var scope = host.Services.CreateScope();
                var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
                
                await seedService.SeedAsync();
                
                Console.WriteLine("Database seeding completed successfully!");
                Console.WriteLine("Seeded data:");
                Console.WriteLine("- 5 Roles (SuperAdmin, Admin, Moderator, User, Guest)");
                Console.WriteLine("- 25 Permissions (User management, Role management, etc.)");
                Console.WriteLine("- 4 Test Users with assigned roles");
                Console.WriteLine("- Complete role-permission mappings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
