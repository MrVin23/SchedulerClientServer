using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Client.Dtos;
using Client.Interfaces;

namespace Client.Interfaces.Authorisation
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    // Server uses HTTP-only cookies for authentication
                    // Cookie is automatically set and will be sent with subsequent requests
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                    return apiResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Invalid credentials",
                        Data = null
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Bad request",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public async Task<ApiResponse<object>?> LogoutAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                }
                else
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Logout failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        public async Task<ApiResponse<LoginResponse>?> GetCurrentUserAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Not authenticated",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get the current token status including expiration info
        /// </summary>
        public async Task<ApiResponse<TokenStatusResponse>?> GetTokenStatusAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/token-status");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<TokenStatusResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = "Not authenticated",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenStatusResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Refresh the authentication token (extends session)
        /// </summary>
        public async Task<ApiResponse<TokenStatusResponse>?> RefreshTokenAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<TokenStatusResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = "Not authenticated - please log in again",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = $"Token refresh failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenStatusResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}

