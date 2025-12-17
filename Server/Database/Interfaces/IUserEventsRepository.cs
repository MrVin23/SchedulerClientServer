using Server.Models.Events;

namespace Server.Database.Interfaces
{
    public interface IUserEventsRepository : IGenericRepository<UserEventsModel>
    {
        Task<IEnumerable<UserEventsModel>> GetUserEventsByUserIdAsync(int userId);
        Task<IEnumerable<UserEventsModel>> GetUserEventsByEventIdAsync(int eventId);
        Task<UserEventsModel?> GetUserEventAsync(int userId, int eventId);
        Task<bool> UserHasEventAsync(int userId, int eventId);
        Task RemoveUserEventAsync(int userId, int eventId);
    }
}

