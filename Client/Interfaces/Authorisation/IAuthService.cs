using Client.Dtos;

namespace Client.Interfaces.Authorisation
{
    public interface IAuthService
    {
        /// <summary>
        /// Login with username and password
        /// </summary>
        Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request);

        /// <summary>
        /// Logout the current user
        /// </summary>
        Task<ApiResponse<object>?> LogoutAsync();

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        Task<ApiResponse<LoginResponse>?> GetCurrentUserAsync();

        /// <summary>
        /// Get the current token status including expiration info
        /// </summary>
        Task<ApiResponse<TokenStatusResponse>?> GetTokenStatusAsync();

        /// <summary>
        /// Refresh the authentication token (extends session)
        /// </summary>
        Task<ApiResponse<TokenStatusResponse>?> RefreshTokenAsync();
    }
}

