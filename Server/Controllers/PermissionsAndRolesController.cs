using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Dtos;
using Server.Models;
using Server.Models.UserPermissions;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class PermissionsAndRolesController : BaseController
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public PermissionsAndRolesController(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUserRoleRepository userRoleRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userRoleRepository = userRoleRepository;
        }

        /// <summary>
        /// Get a paginated list of all role permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of role permissions</returns>
        [HttpGet("role-permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RolePermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetRolePermissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Use queryable with includes to load navigation properties
            var query = _rolePermissionRepository.GetQueryable()
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission);

            var result = await query.ToPagedResponseAsync(parameters);
            
            var rolePermissionResponses = result.Items.Cast<RolePermission>()
                .Select(rp => new RolePermissionResponse
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    RoleName = rp.Role?.Name ?? string.Empty,
                    PermissionId = rp.PermissionId,
                    PermissionName = rp.Permission?.Name ?? string.Empty,
                    CreatedAt = rp.CreatedAt,
                    UpdatedAt = rp.UpdatedAt
                });

            return PaginatedResponse(
                rolePermissionResponses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Role permissions retrieved successfully"
            );
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of permissions for the role</returns>
        [HttpGet("roles/{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPermissionsByRole(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            var permissions = await _permissionRepository.GetPermissionsByRoleAsync(roleId);
            var permissionResponses = permissions.Select(p => new PermissionResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return SuccessResponse(permissionResponses, $"Permissions for role '{role.Name}' retrieved successfully");
        }

        /// <summary>
        /// Set permissions to a role (replaces existing permissions)
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="request">Permission IDs to set</param>
        /// <returns>Updated role permissions</returns>
        [HttpPut("roles/{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetRolePermissions(int roleId, [FromBody] SetPermissionsRequest request)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            // Validate that all permission IDs exist
            foreach (var permissionId in request.PermissionIds)
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    return BadRequestResponse($"Permission with ID {permissionId} not found");
                }
            }

            // Update role permissions
            await _rolePermissionRepository.UpdateRolePermissionsAsync(roleId, request.PermissionIds);

            // Retrieve updated permissions
            var updatedPermissions = await _permissionRepository.GetPermissionsByRoleAsync(roleId);
            var permissionResponses = updatedPermissions.Select(p => new PermissionResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return SuccessResponse(permissionResponses, $"Permissions updated for role '{role.Name}'");
        }

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return NotFoundResponse($"Permission with ID {permissionId} not found");
            }

            var hasPermission = await _rolePermissionRepository.RoleHasPermissionAsync(roleId, permissionId);
            if (!hasPermission)
            {
                return BadRequestResponse($"Role '{role.Name}' does not have permission '{permission.Name}'");
            }

            await _rolePermissionRepository.RemoveRolePermissionAsync(roleId, permissionId);
            return NoContent();
        }

        /// <summary>
        /// Get a paginated list of all roles with their associated permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of roles with permissions</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleWithPermissionsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _roleRepository.GetPagedAsync(parameters);
            
            var rolesWithPermissions = new List<RoleWithPermissionsResponse>();
            foreach (var role in result.Items.Cast<Role>())
            {
                var permissions = await _permissionRepository.GetPermissionsByRoleAsync(role.Id);
                rolesWithPermissions.Add(new RoleWithPermissionsResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt,
                    Permissions = permissions.Select(p => new PermissionResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList()
                });
            }

            return PaginatedResponse(
                rolesWithPermissions,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Roles retrieved successfully"
            );
        }

        /// <summary>
        /// Get users with their roles and permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of users with roles and permissions</returns>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserWithRolesResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUsersWithRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Use queryable with includes to load navigation properties
            var allUserRoles = await _userRoleRepository.GetQueryable()
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();
            
            var userRoles = allUserRoles.Cast<UserRole>().ToList();

            // Group by user
            var groupedUserRoles = userRoles.GroupBy(ur => ur.UserId).ToList();
            
            var usersWithRoles = new List<UserWithRolesResponse>();
            
            foreach (var group in groupedUserRoles.Skip((pageNumber - 1) * pageSize).Take(pageSize))
            {
                var userRolesForUser = group.ToList();
                var firstUserRole = userRolesForUser.First();
                
                var rolesWithPermissions = new List<RoleWithPermissionsResponse>();
                
                foreach (var ur in userRolesForUser)
                {
                    if (ur.Role == null) continue;
                    
                    // Fetch permissions for this role
                    var permissions = await _permissionRepository.GetPermissionsByRoleAsync(ur.Role.Id);
                    
                    rolesWithPermissions.Add(new RoleWithPermissionsResponse
                    {
                        Id = ur.Role.Id,
                        Name = ur.Role.Name ?? string.Empty,
                        Description = ur.Role.Description ?? string.Empty,
                        CreatedAt = ur.Role.CreatedAt,
                        UpdatedAt = ur.Role.UpdatedAt,
                        Permissions = permissions.Select(p => new PermissionResponse
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt
                        }).ToList()
                    });
                }
                
                usersWithRoles.Add(new UserWithRolesResponse
                {
                    UserId = firstUserRole.UserId,
                    Username = firstUserRole.User?.Username ?? string.Empty,
                    Email = firstUserRole.User?.Email ?? string.Empty,
                    FirstName = firstUserRole.User?.FirstName ?? string.Empty,
                    LastName = firstUserRole.User?.LastName ?? string.Empty,
                    Roles = rolesWithPermissions
                });
            }

            var totalCount = groupedUserRoles.Count;

            return PaginatedResponse(
                usersWithRoles,
                pageNumber,
                pageSize,
                totalCount,
                "Users with roles retrieved successfully"
            );
        }

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveUserRole(int userId, int roleId)
        {
            var userRole = await _userRoleRepository.GetUserRoleAsync(userId, roleId);
            if (userRole == null)
            {
                return NotFoundResponse($"User with ID {userId} does not have role with ID {roleId}");
            }

            await _userRoleRepository.RemoveUserRoleAsync(userId, roleId);
            return NoContent();
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Success result</returns>
        [HttpPost("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            // Check if user exists by getting repositories from the same context
            // For now, we'll check if the relationship already exists
            var hasRole = await _userRoleRepository.UserHasRoleAsync(userId, roleId);
            if (hasRole)
            {
                return BadRequestResponse($"User with ID {userId} already has role '{role.Name}'");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            var addedUserRole = await _userRoleRepository.AddAsync(userRole);
            
            var response = new
            {
                Id = addedUserRole.Id,
                UserId = addedUserRole.UserId,
                RoleId = addedUserRole.RoleId,
                RoleName = role.Name,
                CreatedAt = addedUserRole.CreatedAt,
                UpdatedAt = addedUserRole.UpdatedAt
            };

            return StatusCode(201, new ApiResponse<object>(response, $"Role '{role.Name}' assigned to user successfully")
            {
                TraceId = TraceId
            });
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="request">Role creation request</param>
        /// <returns>Created role</returns>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(ApiResponse<RoleWithPermissionsResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequestResponse("Role name is required");
            }

            // Check if role name already exists
            var roleExists = await _roleRepository.RoleNameExistsAsync(request.Name);
            if (roleExists)
            {
                return StatusCode(409, new ApiError($"Role with name '{request.Name}' already exists", "DUPLICATE_ROLE", TraceId));
            }

            var role = new Role
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdRole = await _roleRepository.AddAsync(role);

            var response = new RoleWithPermissionsResponse
            {
                Id = createdRole.Id,
                Name = createdRole.Name,
                Description = createdRole.Description,
                CreatedAt = createdRole.CreatedAt,
                UpdatedAt = createdRole.UpdatedAt,
                Permissions = new List<PermissionResponse>()
            };

            var location = Url.Action(nameof(GetPermissionsByRole), new { roleId = createdRole.Id }) ?? string.Empty;
            return CreatedResponse(response, location, "Role created successfully");
        }

        /// <summary>
        /// Create a new permission
        /// </summary>
        /// <param name="request">Permission creation request</param>
        /// <returns>Created permission</returns>
        [HttpPost("permissions")]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequestResponse("Permission name is required");
            }

            // Check if permission name already exists
            var permissionExists = await _permissionRepository.PermissionNameExistsAsync(request.Name);
            if (permissionExists)
            {
                return StatusCode(409, new ApiError($"Permission with name '{request.Name}' already exists", "DUPLICATE_PERMISSION", TraceId));
            }

            var permission = new Permission
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdPermission = await _permissionRepository.AddAsync(permission);

            var response = new PermissionResponse
            {
                Id = createdPermission.Id,
                Name = createdPermission.Name,
                Description = createdPermission.Description,
                CreatedAt = createdPermission.CreatedAt,
                UpdatedAt = createdPermission.UpdatedAt
            };

            return StatusCode(201, new ApiResponse<PermissionResponse>(response, "Permission created successfully")
            {
                TraceId = TraceId
            });
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Created role permission relationship</returns>
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> AssignPermissionToRole(int roleId, int permissionId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return NotFoundResponse($"Permission with ID {permissionId} not found");
            }

            // Check if the relationship already exists
            var exists = await _rolePermissionRepository.RoleHasPermissionAsync(roleId, permissionId);
            if (exists)
            {
                return StatusCode(409, new ApiError($"Role '{role.Name}' already has permission '{permission.Name}'", "DUPLICATE_PERMISSION_ASSIGNMENT", TraceId));
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdRolePermission = await _rolePermissionRepository.AddAsync(rolePermission);

            var response = new RolePermissionResponse
            {
                Id = createdRolePermission.Id,
                RoleId = createdRolePermission.RoleId,
                RoleName = role.Name,
                PermissionId = createdRolePermission.PermissionId,
                PermissionName = permission.Name,
                CreatedAt = createdRolePermission.CreatedAt,
                UpdatedAt = createdRolePermission.UpdatedAt
            };

            return StatusCode(201, new ApiResponse<RolePermissionResponse>(response, $"Permission '{permission.Name}' assigned to role '{role.Name}' successfully")
            {
                TraceId = TraceId
            });
        }
    }
}