using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Authorization;
using Server.Dtos;
using Server.Utils;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : BaseController
    {
        private readonly IAuthorizationService _authorizationService;

        public TestController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Test if current user has a specific permission
        /// </summary>
        /// <param name="permissionName">Name of the permission to check</param>
        /// <returns>True if user has access, false otherwise</returns>
        [HttpGet("permission/{permissionName}")]
        [Authorize] // Requires authentication
        [ProducesResponseType(typeof(ApiResponse<PermissionTestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> TestPermission(string permissionName)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return StatusCode(401, new ApiError("Not authenticated", "UNAUTHORIZED", TraceId));
            }

            var requirement = new PermissionRequirement(permissionName);
            var result = await _authorizationService.AuthorizeAsync(User, null, requirement);

            var response = new PermissionTestResponse
            {
                HasAccess = result.Succeeded,
                PermissionName = permissionName,
                Message = result.Succeeded 
                    ? $"User has access to permission: {permissionName}" 
                    : $"User does not have access to permission: {permissionName}",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            };

            return SuccessResponse(response, "Permission test completed");
        }

        /// <summary>
        /// Test endpoint protected by Admin permission (Administrator role)
        /// </summary>
        /// <returns>True if user has Admin permission</returns>
        [HttpGet("admin")]
        [Authorize(Policy = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult TestAdminAccess()
        {
            var response = new
            {
                HasAccess = true,
                Message = "You have Admin permission (Administrator role)",
                Role = "Administrator",
                Permission = "Admin",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            };

            return SuccessResponse(response, "Administrator role access granted");
        }

        /// <summary>
        /// Test endpoint protected by Viewer permission (User role)
        /// </summary>
        /// <returns>True if user has Viewer permission</returns>
        [HttpGet("viewer")]
        [Authorize(Policy = "Viewer")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult TestViewerAccess()
        {
            var response = new
            {
                HasAccess = true,
                Message = "You have Viewer permission (User role)",
                Role = "User",
                Permission = "Viewer",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            };

            return SuccessResponse(response, "User role access granted");
        }

        /// <summary>
        /// Test endpoint protected by ActiveUser permission (VerifiedUser role)
        /// </summary>
        /// <returns>True if user has ActiveUser permission</returns>
        [HttpGet("active-user")]
        [Authorize(Policy = "ActiveUser")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult TestActiveUserAccess()
        {
            var response = new
            {
                HasAccess = true,
                Message = "You have ActiveUser permission (VerifiedUser role)",
                Role = "VerifiedUser",
                Permission = "ActiveUser",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            };

            return SuccessResponse(response, "VerifiedUser role access granted");
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        /// <returns>List of all permissions the user has through their roles</returns>
        [HttpGet("my-permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public ActionResult GetMyPermissions()
        {
            var userId = User.GetUserId();
            if (userId == 0)
            {
                return StatusCode(401, new ApiError("User ID not found", "UNAUTHORIZED", TraceId));
            }

            // This would ideally come from a service, but for now we'll return what we can from claims
            var permissions = new List<string>();
            
            // Note: This is a simplified version. In production, you'd want to fetch from database
            // based on the user's roles and their permissions

            return SuccessResponse(permissions, "User permissions retrieved");
        }
    }

    public class PermissionTestResponse
    {
        public bool HasAccess { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
