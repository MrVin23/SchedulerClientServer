using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Models.Events;

namespace Server.Database.Repositories
{
    public class EventSettingsRepository : GenericRepository<EventSettingsModel>, IEventSettingsRepository
    {
        public EventSettingsRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<EventSettingsModel?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(es => es.User)
                .FirstOrDefaultAsync(es => es.UserId == userId);
        }

        public async Task<bool> UserHasSettingsAsync(int userId)
        {
            return await _dbSet.AnyAsync(es => es.UserId == userId);
        }

        public override async Task<EventSettingsModel> AddAsync(EventSettingsModel entity)
        {
            // Check for duplicate user settings constraint before adding
            if (await UserHasSettingsAsync(entity.UserId))
            {
                throw new InvalidOperationException($"User with ID {entity.UserId} already has event settings.");
            }

            return await base.AddAsync(entity);
        }

        public override async Task<EventSettingsModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(es => es.User)
                .FirstOrDefaultAsync(es => es.Id == id);
        }
    }
}
