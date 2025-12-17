using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Client.Dtos;
using Client.Interfaces;

namespace Client.Services.HttpServices
{
    public class EventSettingsService : IEventSettingsService
    {
        private readonly HttpClient _httpClient;

        public EventSettingsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get event settings for the current user
        /// </summary>
        public async Task<ApiResponse<EventSettingsResponse>?> GetMySettingsAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, "api/eventsettings");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<EventSettingsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventSettingsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Get event settings for a specific user (admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        public async Task<ApiResponse<EventSettingsResponse>?> GetSettingsByUserIdAsync(int userId)
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, $"api/eventsettings/{userId}");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<EventSettingsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventSettingsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Create event settings for the current user
        /// </summary>
        /// <param name="request">Event settings creation request</param>
        public async Task<ApiResponse<EventSettingsResponse>?> CreateMySettingsAsync(CreateEventSettingsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Post, "api/eventsettings", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventSettingsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventSettingsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Update event settings for the current user
        /// </summary>
        /// <param name="request">Event settings update request</param>
        public async Task<ApiResponse<EventSettingsResponse>?> UpdateMySettingsAsync(UpdateEventSettingsRequest request)
        {
            try
            {
                var httpRequest = CreateRequestWithCredentials(HttpMethod.Put, "api/eventsettings", JsonContent.Create(request));
                var response = await _httpClient.SendAsync(httpRequest);
                return await HandleResponseAsync<EventSettingsResponse>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<EventSettingsResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Delete event settings for the current user
        /// </summary>
        public async Task<ApiResponse<bool>?> DeleteMySettingsAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Delete, "api/eventsettings");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex.Message);
            }
        }

        /// <summary>
        /// Check if current user has event settings
        /// </summary>
        public async Task<ApiResponse<bool>?> HasSettingsAsync()
        {
            try
            {
                var request = CreateRequestWithCredentials(HttpMethod.Get, "api/eventsettings/exists");
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex.Message);
            }
        }

        #region Helper Methods

        private HttpRequestMessage CreateRequestWithCredentials(HttpMethod method, string uri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (content != null)
            {
                request.Content = content;
            }
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return request;
        }

        private async Task<ApiResponse<T>?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>();
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = error?.Message ?? $"Request failed with status: {response.StatusCode}",
                    Data = default
                };
            }
        }

        private ApiResponse<T> CreateErrorResponse<T>(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"An error occurred: {message}",
                Data = default
            };
        }

        #endregion
    }
}

