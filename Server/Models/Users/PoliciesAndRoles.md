# Dynamic Role-Based Authorization with Policy-Based Authentication

## Overview
This implementation combines role-based and policy-based authentication to create a flexible, database-driven authorization system where admins can manage user roles and permissions without requiring code changes.

## Architecture Benefits
- **Flexibility**: Admins can change permissions without code changes
- **Scalability**: Easy to add new roles and permissions
- **Security**: Centralized permission management
- **Maintainability**: Clear separation between roles and permissions
- **Performance**: Database queries are optimized and can be cached

## Database Structure

### 1. User Model (Already exists)
```csharp
public class User : ModelBase
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}
```

### 2. Role Model
```csharp
public class Role : ModelBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```

### 3. Permission Model
```csharp
public class Permission : ModelBase
{
    public string Name { get; set; } // e.g., "CanAccessAdminPanel", "CanEditPosts"
    public string Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```

### 4. UserRole (Many-to-many relationship)
```csharp
public class UserRole : ModelBase
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
}
```

### 5. RolePermission (Many-to-many relationship)
```csharp
public class RolePermission : ModelBase
{
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; }
}
```

## Authorization Implementation

### 1. Permission Requirement
```csharp
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }
}
```

### 2. Custom Authorization Handler
```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var userId = context.User.GetUserId(); // Extension method to get user ID
        var permissionName = requirement.PermissionName;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();

        // Check if user has any role that has the required permission
        var hasPermission = await dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.Permission.Name == permissionName);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
```

### 3. User ID Extension Method
```csharp
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
```

## Configuration

### 1. Policy Registration (Program.cs)
```csharp
builder.Services.AddAuthorization(options =>
{
    // Dynamic policies will be created based on database permissions
    options.AddPolicy("CanAccessAdminPanel", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanAccessAdminPanel")));
    
    options.AddPolicy("CanEditPosts", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanEditPosts")));
    
    options.AddPolicy("CanDeleteUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement("CanDeleteUsers")));
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

### 2. Database Context Configuration
```csharp
public class YourDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationships
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);
    }
}
```

## Usage in Controllers

### 1. Controller Authorization
```csharp
[Authorize(Policy = "CanAccessAdminPanel")]
public class AdminController : Controller
{
    // Only users with roles that have "CanAccessAdminPanel" permission can access
}

[Authorize(Policy = "CanEditPosts")]
public class PostController : Controller
{
    // Only users with roles that have "CanEditPosts" permission can access
}
```

### 2. Action-Level Authorization
```csharp
public class UserController : Controller
{
    [Authorize(Policy = "CanDeleteUsers")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Only users with roles that have "CanDeleteUsers" permission can access
    }
}
```

## Admin Management Operations

### 1. Assign Role to User
```csharp
public async Task AssignRoleToUser(int userId, int roleId)
{
    var userRole = new UserRole
    {
        UserId = userId,
        RoleId = roleId
    };
    
    _context.UserRoles.Add(userRole);
    await _context.SaveChangesAsync();
}
```

### 2. Remove Role from User
```csharp
public async Task RemoveRoleFromUser(int userId, int roleId)
{
    var userRole = await _context.UserRoles
        .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    
    if (userRole != null)
    {
        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
    }
}
```

### 3. Update Role Permissions
```csharp
public async Task UpdateRolePermissions(int roleId, List<int> permissionIds)
{
    var role = await _context.Roles.FindAsync(roleId);
    
    // Remove existing permissions
    role.RolePermissions.Clear();
    
    // Add new permissions
    foreach (var permissionId in permissionIds)
    {
        role.RolePermissions.Add(new RolePermission 
        { 
            RoleId = roleId, 
            PermissionId = permissionId 
        });
    }
    
    await _context.SaveChangesAsync();
}
```

### 4. Create New Permission
```csharp
public async Task<Permission> CreatePermission(string name, string description)
{
    var permission = new Permission
    {
        Name = name,
        Description = description
    };
    
    _context.Permissions.Add(permission);
    await _context.SaveChangesAsync();
    
    return permission;
}
```

## Implementation Steps

### Phase 1: Database Models
1. Create the Role, Permission, UserRole, and RolePermission models
2. Update the User model to include UserRoles collection
3. Configure the DbContext with proper relationships

### Phase 2: Authorization Infrastructure
1. Create the PermissionRequirement class
2. Implement the PermissionAuthorizationHandler
3. Add the User ID extension method
4. Register policies and handlers in Program.cs

### Phase 3: Controller Implementation
1. Add authorization attributes to controllers
2. Test the authorization system
3. Create admin management endpoints

### Phase 4: Admin Interface
1. Create admin controllers for managing roles and permissions
2. Implement user role assignment functionality
3. Add permission management features

## Testing the System

### 1. Seed Initial Data
```csharp
public async Task SeedInitialData()
{
    // Create roles
    var adminRole = new Role { Name = "Admin", Description = "Administrator" };
    var userRole = new Role { Name = "User", Description = "Regular User" };
    
    // Create permissions
    var adminPanelPermission = new Permission { Name = "CanAccessAdminPanel", Description = "Access admin panel" };
    var editPostsPermission = new Permission { Name = "CanEditPosts", Description = "Edit posts" };
    
    // Assign permissions to roles
    adminRole.RolePermissions.Add(new RolePermission { Permission = adminPanelPermission });
    adminRole.RolePermissions.Add(new RolePermission { Permission = editPostsPermission });
    userRole.RolePermissions.Add(new RolePermission { Permission = editPostsPermission });
    
    _context.Roles.AddRange(adminRole, userRole);
    await _context.SaveChangesAsync();
}
```

### 2. Test Authorization
1. Create test users with different roles
2. Assign roles to users
3. Test accessing protected endpoints
4. Verify permission changes take effect immediately

## Performance Considerations

### 1. Caching
Consider implementing caching for frequently accessed permissions:
```csharp
public class CachedPermissionService
{
    private readonly IMemoryCache _cache;
    private readonly YourDbContext _context;
    
    public async Task<bool> HasPermission(int userId, string permissionName)
    {
        var cacheKey = $"user_{userId}_permission_{permissionName}";
        
        if (_cache.TryGetValue(cacheKey, out bool hasPermission))
        {
            return hasPermission;
        }
        
        // Database query logic here
        var result = await CheckPermissionInDatabase(userId, permissionName);
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }
}
```

### 2. Database Indexing
Add indexes for better performance:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Add indexes for better performance
    modelBuilder.Entity<UserRole>()
        .HasIndex(ur => ur.UserId);
    
    modelBuilder.Entity<RolePermission>()
        .HasIndex(rp => rp.RoleId);
    
    modelBuilder.Entity<Permission>()
        .HasIndex(p => p.Name);
}
```

This implementation provides a robust, flexible authorization system that gives admins complete control over user permissions without requiring code changes.

## Authorization Flow Logic Tree

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              AUTHORIZATION FLOW                                 │
└─────────────────────────────────────────────────────────────────────────────────┘

User Request → Controller Action
    │
    ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           [Authorize(Policy = "CanEditPosts")]                  │
└─────────────────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        PermissionAuthorizationHandler                            │
│  ┌─────────────────────────────────────────────────────────────────────────────┐ │
│  │ 1. Check if user is authenticated                                          │ │
│  │ 2. Extract UserId from ClaimsPrincipal                                     │ │
│  │ 3. Query database for user's roles and permissions                         │ │
│  └─────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              DATABASE QUERY                                     │
│                                                                                 │
│  Users → UserRoles → Roles → RolePermissions → Permissions                      │
│    │        │         │           │              │                            │
│    │        │         │           │              ▼                            │
│    │        │         │           │    ┌─────────────────────┐                │
│    │        │         │           │    │ Permission.Name     │                │
│    │        │         │           │    │ = "CanEditPosts"    │                │
│    │        │         │           │    └─────────────────────┘                │
│    │        │         │           │                                           │
│    │        │         │           ▼                                           │
│    │        │         │    ┌─────────────────────┐                            │
│    │        │         │    │ RolePermission      │                            │
│    │        │         │    │ (Many-to-Many)      │                            │
│    │        │         │    └─────────────────────┘                            │
│    │        │         │                                           │
│    │        │         ▼                                           │
│    │        │    ┌─────────────────────┐                         │
│    │        │    │ Role                │                         │
│    │        │    │ (Admin, User, etc.)  │                         │
│    │        │    └─────────────────────┘                         │
│    │        │                                           │
│    │        ▼                                           │
│    │    ┌─────────────────────┐                         │
│    │    │ UserRole            │                         │
│    │    │ (Many-to-Many)      │                         │
│    │    └─────────────────────┘                         │
│    │                                           │
│    ▼                                           │
│ ┌─────────────────────┐                       │
│ │ User                │                       │
│ │ (Authenticated)     │                       │
│ └─────────────────────┘                       │
└─────────────────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              DECISION LOGIC                                     │
│                                                                                 │
│  IF (user has ANY role with required permission)                              │
│  ┌─────────────────────────────────────────────────────────────────────────────┐ │
│  │  context.Succeed(requirement)  →  ACCESS GRANTED                            │ │
│  └─────────────────────────────────────────────────────────────────────────────┘ │
│  ELSE                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────────────┐ │
│  │  ACCESS DENIED  →  401/403 Response                                        │ │
│  └─────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              ADMIN MANAGEMENT                                   │
└─────────────────────────────────────────────────────────────────────────────────┘

Admin Interface → Database Changes → Immediate Effect
    │                    │                    │
    ▼                    ▼                    ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ Assign Role │    │ Update      │    │ Next Request│
│ to User     │    │ Permissions │    │ Uses New    │
│             │    │             │    │ Permissions │
└─────────────┘    └─────────────┘    └─────────────┘
    │                    │                    │
    ▼                    ▼                    ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ UserRole    │    │ RolePermission│   │ Authorization│
│ Table       │    │ Table         │   │ Handler      │
│ Updated     │    │ Updated       │   │ Queries      │
└─────────────┘    └─────────────┘    └─────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              KEY BENEFITS                                       │
└─────────────────────────────────────────────────────────────────────────────────┘

✅ Dynamic: No code changes needed for permission updates
✅ Flexible: Users can have multiple roles
✅ Scalable: Easy to add new roles and permissions
✅ Secure: Centralized permission management
✅ Real-time: Changes take effect immediately
✅ Admin-friendly: Full control through application interface
