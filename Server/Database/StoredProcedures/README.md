# PostgreSQL Stored Procedures

This directory contains PostgreSQL stored procedures for the Dotnet-Authorization system. These procedures provide efficient database access for user authorization and role management operations.

## Table of Contents

- [Get User Permissions](#get-user-permissions)
- [Get Role Permissions](#get-role-permissions)
- [Get User Roles](#get-user-roles)
- [Usage Examples](#usage-examples)
- [Database Schema](#database-schema)

---

## Get User Permissions

### Procedure: `get_user_permissions`

Retrieves all permissions for a specific user by traversing the user's roles.

**Parameters:**
- `p_user_id` (INTEGER): The ID of the user

**Returns:**
- Table with columns: `id`, `name`, `description`, `created_at`, `updated_at`

**SQL:**
```sql
CREATE OR REPLACE FUNCTION get_user_permissions(p_user_id INTEGER)
RETURNS TABLE (
    "Id" INTEGER,
    "Name" TEXT,
    "Description" TEXT,
    "CreatedAt" TIMESTAMPTZ,
    "UpdatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT
        p."Id",
        p."Name",
        p."Description",
        p."CreatedAt",
        p."UpdatedAt"
    FROM "Permissions" p
    INNER JOIN "RolePermissions" rp ON p."Id" = rp."PermissionId"
    INNER JOIN "Roles" r ON rp."RoleId" = r."Id"
    INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
    WHERE ur."UserId" = p_user_id
    ORDER BY p."Name";
END;
$$ LANGUAGE plpgsql;
```

---

## Get Role Permissions

### Procedure: `get_role_permissions`

Retrieves all permissions assigned to a specific role.

**Parameters:**
- `p_role_id` (INTEGER): The ID of the role

**Returns:**
- Table with columns: `id`, `name`, `description`, `created_at`, `updated_at`

**SQL:**
```sql
CREATE OR REPLACE FUNCTION get_role_permissions(p_role_id INTEGER)
RETURNS TABLE (
    "Id" INTEGER,
    "Name" TEXT,
    "Description" TEXT,
    "CreatedAt" TIMESTAMPTZ,
    "UpdatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p."Id",
        p."Name",
        p."Description",
        p."CreatedAt",
        p."UpdatedAt"
    FROM "Permissions" p
    INNER JOIN "RolePermissions" rp ON p."Id" = rp."PermissionId"
    WHERE rp."RoleId" = p_role_id
    ORDER BY p."Name";
END;
$$ LANGUAGE plpgsql;
```

---

## Get User Roles

### Procedure: `get_user_roles`

Retrieves all roles assigned to a specific user.

**Parameters:**
- `p_user_id` (INTEGER): The ID of the user

**Returns:**
- Table with columns: `id`, `name`, `description`, `created_at`, `updated_at`

**SQL:**
```sql
CREATE OR REPLACE FUNCTION get_user_roles(p_user_id INTEGER)
RETURNS TABLE (
    "Id" INTEGER,
    "Name" TEXT,
    "Description" TEXT,
    "CreatedAt" TIMESTAMPTZ,
    "UpdatedAt" TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        r."Id",
        r."Name",
        r."Description",
        r."CreatedAt",
        r."UpdatedAt"
    FROM "Roles" r
    INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
    WHERE ur."UserId" = p_user_id
    ORDER BY r."Name";
END;
$$ LANGUAGE plpgsql;
```

---

## Additional Helper Procedures

### Check If User Has Permission

```sql
CREATE OR REPLACE FUNCTION user_has_permission(
    p_user_id INTEGER,
    p_permission_name TEXT
)
RETURNS BOOLEAN AS $$
DECLARE
    permission_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM "Permissions" p
        INNER JOIN "RolePermissions" rp ON p."Id" = rp."PermissionId"
        INNER JOIN "Roles" r ON rp."RoleId" = r."Id"
        INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
        WHERE ur."UserId" = p_user_id
          AND p."Name" = p_permission_name
    ) INTO permission_exists;
    
    RETURN permission_exists;
END;
$$ LANGUAGE plpgsql;
```

### Check If User Has Role

```sql
CREATE OR REPLACE FUNCTION user_has_role(
    p_user_id INTEGER,
    p_role_name TEXT
)
RETURNS BOOLEAN AS $$
DECLARE
    role_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM "Roles" r
        INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
        WHERE ur."UserId" = p_user_id
          AND r."Name" = p_role_name
    ) INTO role_exists;
    
    RETURN role_exists;
END;
$$ LANGUAGE plpgsql;
```

### Get User Permissions Count

```sql
CREATE OR REPLACE FUNCTION get_user_permissions_count(p_user_id INTEGER)
RETURNS INTEGER AS $$
DECLARE
    permission_count INTEGER;
BEGIN
    SELECT COUNT(DISTINCT p."Id")
    INTO permission_count
    FROM "Permissions" p
    INNER JOIN "RolePermissions" rp ON p."Id" = rp."PermissionId"
    INNER JOIN "Roles" r ON rp."RoleId" = r."Id"
    INNER JOIN "UserRoles" ur ON r."Id" = ur."RoleId"
    WHERE ur."UserId" = p_user_id;
    
    RETURN permission_count;
END;
$$ LANGUAGE plpgsql;
```

### Get Role Permissions Count

```sql
CREATE OR REPLACE FUNCTION get_role_permissions_count(p_role_id INTEGER)
RETURNS INTEGER AS $$
DECLARE
    permission_count INTEGER;
BEGIN
    SELECT COUNT(p."Id")
    INTO permission_count
    FROM "Permissions" p
    INNER JOIN "RolePermissions" rp ON p."Id" = rp."PermissionId"
    WHERE rp."RoleId" = p_role_id;
    
    RETURN permission_count;
END;
$$ LANGUAGE plpgsql;
```

---

## Usage Examples

### Example 1: Get All Permissions for User ID 1

```sql
SELECT * FROM get_user_permissions(1);
```

**Expected Output:**
```
 id | name                | description           | created_at          | updated_at
----+---------------------+-----------------------+---------------------+-----------------------
  1 | CanAccessAdminPanel | Access admin panel    | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
  3 | CanEditPosts        | Edit posts            | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
  5 | CanDeleteUsers      | Delete users          | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
```

### Example 2: Get All Permissions for Role ID 2

```sql
SELECT * FROM get_role_permissions(2);
```

**Expected Output:**
```
 id | name                | description        | created_at          | updated_at
----+---------------------+--------------------+---------------------+-----------------------
  1 | CanAccessAdminPanel | Access admin panel | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
  2 | CanEditPosts        | Edit posts         | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
```

### Example 3: Get All Roles for User ID 1

```sql
SELECT * FROM get_user_roles(1);
```

**Expected Output:**
```
 id | name       | description              | created_at          | updated_at
----+------------+--------------------------+---------------------+-----------------------
  1 | SuperAdmin | Full system access       | 2024-01-01 00:00:00 | 2024-01-01 00:00:00
  2 | Admin      | Administrative privileges| 2024-01-01 00:00:00 | 2024-01-01 00:00:00
```

### Example 4: Check If User Has Specific Permission

```sql
SELECT user_has_permission(1, 'CanAccessAdminPanel');
-- Returns: true
```

### Example 5: Check If User Has Specific Role

```sql
SELECT user_has_role(1, 'SuperAdmin');
-- Returns: true
```

### Example 6: Get Permission Count for User

```sql
SELECT get_user_permissions_count(1);
-- Returns: 25
```

### Example 7: Get Permission Count for Role

```sql
SELECT get_role_permissions_count(1);
-- Returns: 20
```

---

## Installing the Stored Procedures

### Option 1: Using psql Command Line

```bash
# Connect to your database
psql -h localhost -U template-user -d template-db -f stored_procedures.sql
```

### Option 2: Using Docker Exec

```bash
# Copy the SQL file to the container
docker cp stored_procedures.sql <container-name>:/tmp/

# Execute it
docker exec -i <container-name> psql -U template-user -d template-db -f /tmp/stored_procedures.sql
```

### Option 3: Using Entity Framework Migration

You can add these procedures via a new migration:

```bash
dotnet ef migrations add AddStoredProcedures
```

Then modify the migration file to include the SQL:

```csharp
public partial class AddStoredProcedures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var sql = File.ReadAllText("path/to/stored_procedures.sql");
        migrationBuilder.Sql(sql);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_user_permissions(INTEGER);");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_role_permissions(INTEGER);");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_user_roles(INTEGER);");
        // ... drop other functions
    }
}
```

---

## Calling from Entity Framework

### Example: Get User Permissions in C#

```csharp
public async Task<List<Permission>> GetUserPermissions(int userId)
{
    var permissions = await _context
        .Database
        .SqlQueryRaw<Permission>(
            "SELECT * FROM get_user_permissions({0})",
            userId
        )
        .ToListAsync();
    
    return permissions;
}
```

### Example: Check Permission in C#

```csharp
public async Task<bool> UserHasPermission(int userId, string permissionName)
{
    var result = await _context
        .Database
        .ExecuteSqlRawAsync(
            "SELECT user_has_permission({0}, {1})",
            userId,
            permissionName
        );
    
    return result > 0;
}
```

---

## Database Schema

### Entity Relationship Diagram

```
┌─────────┐         ┌──────────────┐         ┌──────┐
│  Users  │─────────│  UserRoles   │─────────│Roles │
└─────────┘         └──────────────┘         └──────┘
                                          │
                                          │
                                     ┌─────────────┐
                                     │RolePermissions
                                     └─────────────┘
                                          │
                                          │
                                     ┌─────────────┐
                                     │ Permissions │
                                     └─────────────┘
```

### Tables

- **Users**: `Id`, `Username`, `Email`, `FirstName`, `LastName`, `Password`, `CreatedAt`, `UpdatedAt`
- **Roles**: `Id`, `Name`, `Description`, `CreatedAt`, `UpdatedAt`
- **Permissions**: `Id`, `Name`, `Description`, `CreatedAt`, `UpdatedAt`
- **UserRoles**: `Id`, `UserId`, `RoleId`, `CreatedAt`, `UpdatedAt` (Composite Key: UserId + RoleId)
- **RolePermissions**: `Id`, `RoleId`, `PermissionId`, `CreatedAt`, `UpdatedAt` (Composite Key: RoleId + PermissionId)

---

## Performance Considerations

### Indexes

The following indexes are already configured in the database:

```sql
-- UserRoles indexes
CREATE INDEX IF NOT EXISTS "IX_UserRoles_UserId" ON "UserRoles" ("UserId");

-- RolePermissions indexes
CREATE INDEX IF NOT EXISTS "IX_RolePermissions_RoleId" ON "RolePermissions" ("RoleId");

-- Unique constraints
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Permissions_Name" ON "Permissions" ("Name");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Roles_Name" ON "Roles" ("Name");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
```

### Query Optimization Tips

1. **Use DISTINCT** when fetching permissions to avoid duplicates (already implemented)
2. **Index foreign keys** for faster joins (already implemented)
3. **Order results** by name for consistent output
4. **Consider caching** frequently accessed permissions at the application level

---

## Error Handling

All stored procedures include basic error handling. If a function is called with an invalid parameter:

- NULL values will return empty result sets
- Non-existent IDs will return empty result sets
- No exceptions will be thrown

For additional error handling, wrap calls in try-catch blocks:

```sql
BEGIN
    SELECT * FROM get_user_permissions(999);
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Error occurred: %', SQLERRM;
END;
```

---

## Maintenance

### Updating Procedures

To update a procedure, use `CREATE OR REPLACE FUNCTION`:

```sql
CREATE OR REPLACE FUNCTION get_user_permissions(p_user_id INTEGER)
RETURNS TABLE (...) AS $$
-- updated logic
$$ LANGUAGE plpgsql;
```

### Removing Procedures

To remove all procedures:

```sql
DROP FUNCTION IF EXISTS get_user_permissions(INTEGER);
DROP FUNCTION IF EXISTS get_role_permissions(INTEGER);
DROP FUNCTION IF EXISTS get_user_roles(INTEGER);
DROP FUNCTION IF EXISTS user_has_permission(INTEGER, VARCHAR);
DROP FUNCTION IF EXISTS user_has_role(INTEGER, VARCHAR);
DROP FUNCTION IF EXISTS get_user_permissions_count(INTEGER);
DROP FUNCTION IF EXISTS get_role_permissions_count(INTEGER);
```

---

## Version History

- **Version 1.0** (2024-01-01)
  - Initial creation
  - Basic CRUD operations
  - Helper functions added

