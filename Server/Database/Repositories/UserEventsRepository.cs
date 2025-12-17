using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Models.Events;

namespace Server.Database.Repositories
{
    public class UserEventsRepository : GenericRepository<UserEventsModel>, IUserEventsRepository
    {
        public UserEventsRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserEventsModel>> GetUserEventsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ue => ue.Event)
                .ThenInclude(e => e!.EventType)
                .Include(ue => ue.User)
                .Where(ue => ue.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserEventsModel>> GetUserEventsByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Include(ue => ue.User)
                .Include(ue => ue.Event)
                .ThenInclude(e => e!.EventType)
                .Where(ue => ue.EventId == eventId)
                .ToListAsync();
        }

        public async Task<UserEventsModel?> GetUserEventAsync(int userId, int eventId)
        {
            return await _dbSet
                .Include(ue => ue.User)
                .Include(ue => ue.Event)
                .ThenInclude(e => e!.EventType)
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EventId == eventId);
        }

        public async Task<bool> UserHasEventAsync(int userId, int eventId)
        {
            return await _dbSet.AnyAsync(ue => ue.UserId == userId && ue.EventId == eventId);
        }

        public async Task RemoveUserEventAsync(int userId, int eventId)
        {
            var userEvent = await GetUserEventAsync(userId, eventId);
            if (userEvent != null)
            {
                await DeleteAsync(userEvent);
            }
        }

        public override async Task<UserEventsModel> AddAsync(UserEventsModel entity)
        {
            // Check for duplicate user-event combination constraint before adding
            if (await UserHasEventAsync(entity.UserId, entity.EventId))
            {
                throw new InvalidOperationException($"User with ID {entity.UserId} is already linked to event with ID {entity.EventId}.");
            }

            return await base.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<UserEventsModel> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var duplicates = new List<string>();
            var seenCombinations = new HashSet<(int UserId, int EventId)>();

            // Check for duplicates within the batch itself
            foreach (var entity in entityList)
            {
                var combination = (entity.UserId, entity.EventId);
                if (seenCombinations.Contains(combination))
                {
                    duplicates.Add($"User {entity.UserId} - Event {entity.EventId} (duplicate in batch)");
                }
                else
                {
                    seenCombinations.Add(combination);
                    // Check against existing records in database
                    if (await UserHasEventAsync(entity.UserId, entity.EventId))
                    {
                        duplicates.Add($"User {entity.UserId} - Event {entity.EventId} (already exists)");
                    }
                }
            }

            if (duplicates.Any())
            {
                throw new InvalidOperationException($"The following user-event combinations are invalid: {string.Join(", ", duplicates)}");
            }

            await base.AddRangeAsync(entityList);
        }

        public override async Task<UserEventsModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(ue => ue.User)
                .Include(ue => ue.Event)
                .ThenInclude(e => e!.EventType)
                .FirstOrDefaultAsync(ue => ue.Id == id);
        }
    }
}

