using Client.Dtos;

namespace Client.Interfaces
{
    public interface IEventSettingsService
    {
        /// <summary>
        /// Get event settings for the current user
        /// </summary>
        Task<ApiResponse<EventSettingsResponse>?> GetMySettingsAsync();

        /// <summary>
        /// Get event settings for a specific user (admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        Task<ApiResponse<EventSettingsResponse>?> GetSettingsByUserIdAsync(int userId);

        /// <summary>
        /// Create event settings for the current user
        /// </summary>
        /// <param name="request">Event settings creation request</param>
        Task<ApiResponse<EventSettingsResponse>?> CreateMySettingsAsync(CreateEventSettingsRequest request);

        /// <summary>
        /// Update event settings for the current user
        /// </summary>
        /// <param name="request">Event settings update request</param>
        Task<ApiResponse<EventSettingsResponse>?> UpdateMySettingsAsync(UpdateEventSettingsRequest request);

        /// <summary>
        /// Delete event settings for the current user
        /// </summary>
        Task<ApiResponse<bool>?> DeleteMySettingsAsync();

        /// <summary>
        /// Check if current user has event settings
        /// </summary>
        Task<ApiResponse<bool>?> HasSettingsAsync();
    }
}

