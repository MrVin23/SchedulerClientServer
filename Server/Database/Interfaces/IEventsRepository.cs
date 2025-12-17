using Server.Models.Events;

namespace Server.Database.Interfaces
{
    public interface IEventsRepository : IGenericRepository<EventsModel>
    {
        Task<EventsModel?> GetEventWithTypeAsync(int eventId);
        Task<IEnumerable<EventsModel>> GetEventsByTypeAsync(int eventTypeId);
        Task<IEnumerable<EventsModel>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EventsModel>> GetUpcomingEventsAsync(DateTime fromDate);
        Task<IEnumerable<EventsModel>> GetCompletedEventsAsync();
        Task<IEnumerable<EventsModel>> GetEventsByUserAsync(int userId);
    }
}

