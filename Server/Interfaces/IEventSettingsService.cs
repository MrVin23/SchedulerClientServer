using Server.Dtos;
using Server.Models.Events;

namespace Server.Interfaces
{
    public interface IEventSettingsService
    {
        Task<EventSettingsModel?> GetByUserIdAsync(int userId);
        Task<EventSettingsModel> CreateEventSettingsAsync(CreateEventSettingsRequest request, int userId);
        Task<EventSettingsModel> UpdateEventSettingsAsync(int userId, UpdateEventSettingsRequest request);
        Task<bool> DeleteEventSettingsAsync(int userId);
        Task<bool> UserHasSettingsAsync(int userId);
    }
}
