using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Models.UserPermissions;

namespace Server.Database.Services
{
    public class SeedService : ISeedService
    {
        private readonly DatabaseContext _context;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public SeedService(
            DatabaseContext context,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRolePermissionRepository rolePermissionRepository)
        {
            _context = context;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.Roles.AnyAsync())
            {
                return; // Data already seeded
            }

            await SeedRolesAsync();
            await SeedPermissionsAsync();
            await AssignPermissionsToRolesAsync();
            await SeedUsersAsync();
        }

        public async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "SuperAdmin",
                    Description = "Super Administrator with full system access"
                },
                new Role
                {
                    Name = "Admin",
                    Description = "Administrator with administrative privileges"
                },
                new Role
                {
                    Name = "Moderator",
                    Description = "Moderator with content management privileges"
                },
                new Role
                {
                    Name = "User",
                    Description = "Regular user with basic privileges"
                },
                new Role
                {
                    Name = "Guest",
                    Description = "Guest user with limited access"
                }
            };

            foreach (var role in roles)
            {
                if (!await _roleRepository.RoleNameExistsAsync(role.Name))
                {
                    await _roleRepository.AddAsync(role);
                }
            }
        }

        public async Task SeedPermissionsAsync()
        {
            var permissions = new List<Permission>
            {
                // User Management Permissions
                new Permission
                {
                    Name = "CanViewUsers",
                    Description = "Can view user list and details"
                },
                new Permission
                {
                    Name = "CanCreateUsers",
                    Description = "Can create new users"
                },
                new Permission
                {
                    Name = "CanEditUsers",
                    Description = "Can edit user information"
                },
                new Permission
                {
                    Name = "CanDeleteUsers",
                    Description = "Can delete users"
                },
                new Permission
                {
                    Name = "CanAssignRoles",
                    Description = "Can assign roles to users"
                },

                // Role Management Permissions
                new Permission
                {
                    Name = "CanViewRoles",
                    Description = "Can view role list and details"
                },
                new Permission
                {
                    Name = "CanCreateRoles",
                    Description = "Can create new roles"
                },
                new Permission
                {
                    Name = "CanEditRoles",
                    Description = "Can edit role information"
                },
                new Permission
                {
                    Name = "CanDeleteRoles",
                    Description = "Can delete roles"
                },

                // Permission Management Permissions
                new Permission
                {
                    Name = "CanViewPermissions",
                    Description = "Can view permission list and details"
                },
                new Permission
                {
                    Name = "CanCreatePermissions",
                    Description = "Can create new permissions"
                },
                new Permission
                {
                    Name = "CanEditPermissions",
                    Description = "Can edit permission information"
                },
                new Permission
                {
                    Name = "CanDeletePermissions",
                    Description = "Can delete permissions"
                },
                new Permission
                {
                    Name = "CanManageRolePermissions",
                    Description = "Can assign/remove permissions from roles"
                },

                // Admin Panel Permissions
                new Permission
                {
                    Name = "CanAccessAdminPanel",
                    Description = "Can access the admin panel"
                },
                new Permission
                {
                    Name = "CanViewSystemSettings",
                    Description = "Can view system settings"
                },
                new Permission
                {
                    Name = "CanEditSystemSettings",
                    Description = "Can edit system settings"
                },

                // Content Management Permissions
                new Permission
                {
                    Name = "CanViewPosts",
                    Description = "Can view posts"
                },
                new Permission
                {
                    Name = "CanCreatePosts",
                    Description = "Can create posts"
                },
                new Permission
                {
                    Name = "CanEditPosts",
                    Description = "Can edit posts"
                },
                new Permission
                {
                    Name = "CanDeletePosts",
                    Description = "Can delete posts"
                },
                new Permission
                {
                    Name = "CanModeratePosts",
                    Description = "Can moderate posts (approve/reject)"
                },

                // Profile Permissions
                new Permission
                {
                    Name = "CanViewOwnProfile",
                    Description = "Can view own profile"
                },
                new Permission
                {
                    Name = "CanEditOwnProfile",
                    Description = "Can edit own profile"
                }
            };

            foreach (var permission in permissions)
            {
                if (!await _permissionRepository.PermissionNameExistsAsync(permission.Name))
                {
                    await _permissionRepository.AddAsync(permission);
                }
            }
        }

        public async Task AssignPermissionsToRolesAsync()
        {
            // Get all roles and permissions
            var roles = await _roleRepository.GetAllAsync();
            var permissions = await _permissionRepository.GetAllAsync();

            var roleDict = roles.ToDictionary(r => r.Name, r => r);
            var permissionDict = permissions.ToDictionary(p => p.Name, p => p);

            // SuperAdmin - All permissions
            var superAdminRole = roleDict["SuperAdmin"];
            var allPermissionIds = permissions.Select(p => p.Id).ToList();
            await _rolePermissionRepository.UpdateRolePermissionsAsync(superAdminRole.Id, allPermissionIds);

            // Admin - Most permissions except super admin stuff
            var adminRole = roleDict["Admin"];
            var adminPermissionIds = permissions
                .Where(p => !p.Name.Contains("SystemSettings") && p.Name != "CanDeleteUsers")
                .Select(p => p.Id)
                .ToList();
            await _rolePermissionRepository.UpdateRolePermissionsAsync(adminRole.Id, adminPermissionIds);

            // Moderator - Content and user management permissions
            var moderatorRole = roleDict["Moderator"];
            var moderatorPermissionIds = permissions
                .Where(p => p.Name.Contains("Posts") || 
                           p.Name.Contains("ViewUsers") || 
                           p.Name.Contains("ViewRoles") ||
                           p.Name.Contains("ViewPermissions") ||
                           p.Name.Contains("OwnProfile"))
                .Select(p => p.Id)
                .ToList();
            await _rolePermissionRepository.UpdateRolePermissionsAsync(moderatorRole.Id, moderatorPermissionIds);

            // User - Basic permissions
            var userRole = roleDict["User"];
            var userPermissionIds = permissions
                .Where(p => p.Name.Contains("Posts") && !p.Name.Contains("Moderate") ||
                           p.Name.Contains("OwnProfile"))
                .Select(p => p.Id)
                .ToList();
            await _rolePermissionRepository.UpdateRolePermissionsAsync(userRole.Id, userPermissionIds);

            // Guest - Very limited permissions
            var guestRole = roleDict["Guest"];
            var guestPermissionIds = permissions
                .Where(p => p.Name == "CanViewPosts" || p.Name == "CanViewOwnProfile")
                .Select(p => p.Id)
                .ToList();
            await _rolePermissionRepository.UpdateRolePermissionsAsync(guestRole.Id, guestPermissionIds);
        }

        public async Task SeedUsersAsync()
        {
            var users = new List<Server.Models.Users.User>
            {
                new Server.Models.Users.User
                {
                    Username = "superadmin",
                    Email = "superadmin@example.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    Password = "SuperAdmin123!" // In production, this should be hashed
                },
                new Server.Models.Users.User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Password = "Admin123!" // In production, this should be hashed
                },
                new Server.Models.Users.User
                {
                    Username = "moderator",
                    Email = "moderator@example.com",
                    FirstName = "Moderator",
                    LastName = "User",
                    Password = "Moderator123!" // In production, this should be hashed
                },
                new Server.Models.Users.User
                {
                    Username = "testuser",
                    Email = "user@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Password = "User123!" // In production, this should be hashed
                }
            };

            foreach (var user in users)
            {
                if (!await _userRepository.UsernameExistsAsync(user.Username!))
                {
                    await _userRepository.AddAsync(user);
                }
            }

            // Assign roles to users
            await AssignRolesToUsersAsync();
        }

        private async Task AssignRolesToUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var roles = await _roleRepository.GetAllAsync();

            var userDict = users.ToDictionary(u => u.Username!, u => u);
            var roleDict = roles.ToDictionary(r => r.Name, r => r);

            // Assign SuperAdmin role to superadmin user
            var superAdminUser = userDict["superadmin"];
            var superAdminRole = roleDict["SuperAdmin"];
            if (!await _userRoleRepository.UserHasRoleAsync(superAdminUser.Id, superAdminRole.Id))
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = superAdminUser.Id,
                    RoleId = superAdminRole.Id
                });
            }

            // Assign Admin role to admin user
            var adminUser = userDict["admin"];
            var adminRole = roleDict["Admin"];
            if (!await _userRoleRepository.UserHasRoleAsync(adminUser.Id, adminRole.Id))
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
            }

            // Assign Moderator role to moderator user
            var moderatorUser = userDict["moderator"];
            var moderatorRole = roleDict["Moderator"];
            if (!await _userRoleRepository.UserHasRoleAsync(moderatorUser.Id, moderatorRole.Id))
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = moderatorUser.Id,
                    RoleId = moderatorRole.Id
                });
            }

            // Assign User role to testuser
            var testUser = userDict["testuser"];
            var userRole = roleDict["User"];
            if (!await _userRoleRepository.UserHasRoleAsync(testUser.Id, userRole.Id))
            {
                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = testUser.Id,
                    RoleId = userRole.Id
                });
            }
        }
    }
}
