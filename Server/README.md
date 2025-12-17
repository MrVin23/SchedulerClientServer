# Dotnet Authorization Server

A comprehensive .NET 9 Web API server implementing a dynamic, database-driven authorization system with policy-based authentication. This system allows administrators to manage user roles and permissions without requiring code changes.

## 🚀 Features

- **Dynamic Authorization**: Database-driven roles and permissions
- **Policy-Based Authentication**: Flexible authorization policies
- **Admin Management**: Change permissions without code deployment
- **Multiple User Roles**: Users can have multiple roles simultaneously
- **PostgreSQL Integration**: Robust database backend
- **Swagger Documentation**: Interactive API documentation
- **Docker Support**: Easy containerized deployment

## 🏗️ Architecture

### Database-Driven Authorization Flow

```
User Request → Controller Action
    │
    ▼
[Authorize(Policy = "CanEditPosts")]
    │
    ▼
PermissionAuthorizationHandler
    │
    ▼
Database Query: Users → UserRoles → Roles → RolePermissions → Permissions
    │
    ▼
Decision: Access Granted/Denied
```

### Key Components

- **Users**: System users with authentication credentials
- **Roles**: User roles (SuperAdmin, Admin, Moderator, User, Guest)
- **Permissions**: Specific actions/access rights
- **UserRoles**: Many-to-many relationship between Users and Roles
- **RolePermissions**: Many-to-many relationship between Roles and Permissions

## 📋 Prerequisites

- .NET 9 SDK
- Docker Desktop (for PostgreSQL)
- PostgreSQL client (optional, for direct database access)

## 🛠️ Setup Instructions

### 1. Clone and Navigate
```bash
git clone <your-repo-url>
cd Dotnet-Authorization/Server
```

### 2. Start PostgreSQL Database
```bash
docker-compose up template_database -d
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Create Database Migration
```bash
dotnet ef migrations add InitialCreate
```

### 5. Apply Migration
```bash
dotnet ef database update
```

### 6. Seed Initial Data
```bash
dotnet run --seed
```

### 7. Start Application
```bash
dotnet run
```

## 🗄️ Database Schema

### Tables Created

| Table | Description |
|-------|-------------|
| `Users` | User accounts with authentication info |
| `Roles` | User roles (SuperAdmin, Admin, etc.) |
| `Permissions` | Specific permissions (CanEditPosts, etc.) |
| `UserRoles` | Many-to-many: Users ↔ Roles |
| `RolePermissions` | Many-to-many: Roles ↔ Permissions |

### Seeded Data

#### Roles (5)
- **SuperAdmin**: Full system access
- **Admin**: Administrative privileges  
- **Moderator**: Content management
- **User**: Basic privileges
- **Guest**: Limited access

#### Permissions (25)
- **User Management**: CanViewUsers, CanCreateUsers, CanEditUsers, CanDeleteUsers, CanAssignRoles
- **Role Management**: CanViewRoles, CanCreateRoles, CanEditRoles, CanDeleteRoles
- **Permission Management**: CanViewPermissions, CanCreatePermissions, CanEditPermissions, CanDeletePermissions, CanManageRolePermissions
- **Admin Panel**: CanAccessAdminPanel, CanViewSystemSettings, CanEditSystemSettings
- **Content Management**: CanViewPosts, CanCreatePosts, CanEditPosts, CanDeletePosts, CanModeratePosts
- **Profile Management**: CanViewOwnProfile, CanEditOwnProfile

#### Test Users (4)
- **superadmin** / `SuperAdmin123!` → SuperAdmin role
- **admin** / `Admin123!` → Admin role
- **moderator** / `Moderator123!` → Moderator role
- **testuser** / `User123!` → User role

## 🔧 API Endpoints

### User Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users` | Get all users |
| `GET` | `/api/users/{id}` | Get user by ID |
| `GET` | `/api/users/username/{username}` | Get user by username |
| `GET` | `/api/users/email/{email}` | Get user by email |
| `GET` | `/api/users/role/{roleId}` | Get users by role |
| `POST` | `/api/users` | Create new user |
| `PUT` | `/api/users/{id}` | Update user |
| `DELETE` | `/api/users/{id}` | Delete user |
| `GET` | `/api/users/exists/username/{username}` | Check username exists |
| `GET` | `/api/users/exists/email/{email}` | Check email exists |

### Example API Usage

#### Get All Users
```bash
curl http://localhost:5097/api/users
```

#### Create New User
```bash
curl -X POST http://localhost:5097/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "firstName": "New",
    "lastName": "User",
    "password": "NewUser123!"
  }'
```

#### Get User by Username
```bash
curl http://localhost:5097/api/users/username/superadmin
```

## 🔐 Authorization System

### How It Works

1. **Policy Definition**: Policies are defined in code (e.g., `"CanEditPosts"`)
2. **Database Storage**: Permission assignments are stored in the database
3. **Dynamic Evaluation**: Authorization handler queries database on each request
4. **Admin Control**: Admins can change permissions without code changes

### Permission Hierarchy

```
SuperAdmin (25 permissions)
├── All system permissions
└── Full access

Admin (22 permissions)
├── User management
├── Role management
├── Permission management
├── Admin panel access
└── Content management

Moderator (8 permissions)
├── Content management
├── User viewing
└── Profile management

User (6 permissions)
├── Basic content access
└── Profile management

Guest (2 permissions)
└── View-only access
```

## 🎯 Usage Examples

### Testing in Swagger UI

1. Navigate to `http://localhost:5097` (Swagger UI)
2. Explore available endpoints
3. Test user CRUD operations
4. Verify role-based access

### Admin Workflow Example

**Scenario**: Admin wants to remove "CanEditPosts" permission from "User" role

1. **Current State**: Users with "User" role can edit posts
2. **Admin Action**: Remove "CanEditPosts" from "User" role via admin interface
3. **Immediate Effect**: Users with "User" role can no longer edit posts
4. **No Code Changes**: Permission change happens through database only

## 🏃‍♂️ Development Commands

### Application Commands
```bash
# Start application (no seeding)
dotnet run

# Seed database only (exits after seeding)
dotnet run --seed

# Standalone seeding with detailed output
dotnet run --project Server -- --seed-standalone

# Start with specific URL
dotnet run --urls "http://localhost:5097"
```

### Database Commands
```bash
# Create new migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Docker Commands
```bash
# Start PostgreSQL
docker-compose up template_database -d

# Start pgAdmin (optional)
docker-compose up pgadmin -d

# Stop all services
docker-compose down
```

## 🔧 Configuration

### Connection String
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=template-db;Username=template-user;Password=Tb2024cP"
  }
}
```

### Docker Configuration
Located in `docker-compose.yml`:
- **PostgreSQL**: Port 5432
- **pgAdmin**: Port 5052
- **Database**: template-db
- **Username**: template-user
- **Password**: Tb2024cP

## 📁 Project Structure

```
Server/
├── Controllers/
│   ├── Interfaces/          # Service interfaces
│   ├── Services/           # Business logic services
│   └── UsersController.cs  # User API endpoints
├── Database/
│   ├── Interfaces/         # Repository interfaces
│   ├── Repositories/       # Data access layer
│   └── Services/          # Database context & seeding
├── Models/
│   ├── Users/             # User models
│   ├── UserPermissions/   # Authorization models
│   └── PaginationModel.cs # Pagination support
├── Program.cs             # Application startup
└── appsettings.json       # Configuration
```

## 🚀 Next Steps

### Planned Features
- [ ] JWT Authentication
- [ ] Password Hashing
- [ ] Role Management API
- [ ] Permission Management API
- [ ] Admin Dashboard
- [ ] Audit Logging

### Implementation Phases
1. **Phase 1**: ✅ Database models and relationships
2. **Phase 2**: ✅ Authorization infrastructure
3. **Phase 3**: ✅ Controller implementation
4. **Phase 4**: 🔄 Admin interface (in progress)

## 🐛 Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Kill process using port 5097
netstat -ano | findstr :5097
taskkill /PID <PID> /F
```

#### Database Connection Failed
- Ensure PostgreSQL container is running: `docker-compose ps`
- Check connection string in `appsettings.json`
- Verify database exists: `docker-compose exec template_database psql -U template-user -d template-db`

#### Migration Errors
```bash
# Reset migrations (WARNING: This will delete all data)
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run --seed
```

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📞 Support

For questions or issues, please create an issue in the repository or contact the development team.
