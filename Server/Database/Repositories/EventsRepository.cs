using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Models.Events;

namespace Server.Database.Repositories
{
    public class EventsRepository : GenericRepository<EventsModel>, IEventsRepository
    {
        public EventsRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<EventsModel?> GetEventWithTypeAsync(int eventId)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByTypeAsync(int eventTypeId)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Where(e => e.EventTypeId == eventTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Where(e => e.StartDateTime >= startDate && e.StartDateTime <= endDate)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventsModel>> GetUpcomingEventsAsync(DateTime fromDate)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Where(e => e.StartDateTime >= fromDate && !e.IsCompleted)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventsModel>> GetCompletedEventsAsync()
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Where(e => e.IsCompleted)
                .OrderByDescending(e => e.EndDateTime ?? e.StartDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventsModel>> GetEventsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .Where(e => _context.UserEvents.Any(ue => ue.EventId == e.Id && ue.UserId == userId))
                .ToListAsync();
        }

        public override async Task<EventsModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}

