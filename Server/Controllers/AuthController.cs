using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using Server.Dtos;
using Server.Interfaces;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>User information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequestResponse("Username and password are required");
            }

            var user = await _userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                return StatusCode(401, new ApiError("Invalid username or password", "INVALID_CREDENTIALS", TraceId));
            }

            // Get user roles
            var roleNames = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            // Create claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username ?? string.Empty),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Add role claims
            foreach (var role in roleNames)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
            }

            // Create claims principal
            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // VP: Todo make cookie expiration time configurable or match industry standards
            // For testing: Use AddMinutes(2) to test expiration quickly
            // For production: Use configuration-based expiration
            var tokenExpiration = TimeSpan.FromHours(1); // Change to AddMinutes(2) for testing
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(tokenExpiration),
                // Store expiration time in properties for client-side access
                Items = { { "TokenExpiration", DateTimeOffset.UtcNow.Add(tokenExpiration).ToString("o") } }
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Create detailed role information
            var roleDetails = user.UserRoles?.Where(ur => ur.Role != null)
                .Select(ur => new RoleResponse
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name ?? string.Empty,
                    Description = ur.Role.Description ?? string.Empty
                })
                .ToList() ?? new List<RoleResponse>();

            // Create login response
            var loginResponse = new LoginResponse
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roleNames,
                RoleDetails = roleDetails
            };

            return SuccessResponse(loginResponse, "Login successful");
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        /// <returns>Logout result</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return SuccessResponse<object?>(null, "Logout successful");
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return StatusCode(401, new ApiError("Not authenticated", "UNAUTHORIZED", TraceId));
            }

            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return StatusCode(401, new ApiError("User identity not found", "UNAUTHORIZED", TraceId));
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return StatusCode(401, new ApiError("User not found", "UNAUTHORIZED", TraceId));
            }

            var roleNames = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            // Create detailed role information
            var roleDetails = user.UserRoles?.Where(ur => ur.Role != null)
                .Select(ur => new RoleResponse
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name ?? string.Empty,
                    Description = ur.Role.Description ?? string.Empty
                })
                .ToList() ?? new List<RoleResponse>();

            var loginResponse = new LoginResponse
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roleNames,
                RoleDetails = roleDetails
            };

            return SuccessResponse(loginResponse, "User retrieved successfully");
        }

        /// <summary>
        /// Check authentication token status and get expiration info
        /// Use this endpoint to determine if the token needs refresh
        /// </summary>
        /// <returns>Token status including expiration time</returns>
        [HttpGet("token-status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<TokenStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetTokenStatus()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return StatusCode(401, new ApiError("Not authenticated", "UNAUTHORIZED", TraceId));
            }

            // Get authentication properties from the current cookie
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!authenticateResult.Succeeded || authenticateResult.Properties == null)
            {
                return StatusCode(401, new ApiError("Authentication failed", "UNAUTHORIZED", TraceId));
            }

            var expiresUtc = authenticateResult.Properties.ExpiresUtc;
            var issuedUtc = authenticateResult.Properties.IssuedUtc;
            
            var response = new TokenStatusResponse
            {
                IsAuthenticated = true,
                Username = User.Identity.Name ?? string.Empty,
                ExpiresUtc = expiresUtc,
                IssuedUtc = issuedUtc,
                TimeRemaining = expiresUtc.HasValue 
                    ? expiresUtc.Value - DateTimeOffset.UtcNow 
                    : null,
                IsExpiringSoon = expiresUtc.HasValue && 
                    (expiresUtc.Value - DateTimeOffset.UtcNow).TotalMinutes < 10 // Warning if < 10 minutes left
            };

            return SuccessResponse(response, "Token status retrieved");
        }

        /// <summary>
        /// Refresh the authentication token (extends session)
        /// Call this before the token expires to maintain the session
        /// </summary>
        /// <returns>New token expiration information</returns>
        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<TokenStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RefreshToken()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return StatusCode(401, new ApiError("Not authenticated", "UNAUTHORIZED", TraceId));
            }

            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return StatusCode(401, new ApiError("User identity not found", "UNAUTHORIZED", TraceId));
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return StatusCode(401, new ApiError("User not found", "UNAUTHORIZED", TraceId));
            }

            // Re-authenticate with fresh expiration
            var roleNames = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username ?? string.Empty),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in roleNames)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
            }

            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var tokenExpiration = TimeSpan.FromHours(1); // Same as login
            var newExpiresUtc = DateTimeOffset.UtcNow.Add(tokenExpiration);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = newExpiresUtc,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                authProperties);

            var response = new TokenStatusResponse
            {
                IsAuthenticated = true,
                Username = user.Username ?? string.Empty,
                ExpiresUtc = newExpiresUtc,
                IssuedUtc = DateTimeOffset.UtcNow,
                TimeRemaining = tokenExpiration,
                IsExpiringSoon = false
            };

            return SuccessResponse(response, "Token refreshed successfully");
        }
    }
}

