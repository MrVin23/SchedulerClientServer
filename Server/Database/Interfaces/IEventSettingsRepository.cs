using Server.Models.Events;

namespace Server.Database.Interfaces
{
    public interface IEventSettingsRepository : IGenericRepository<EventSettingsModel>
    {
        Task<EventSettingsModel?> GetByUserIdAsync(int userId);
        Task<bool> UserHasSettingsAsync(int userId);
    }
}
