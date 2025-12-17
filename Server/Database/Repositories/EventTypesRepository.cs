using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Database.Services;
using Server.Models.Events;

namespace Server.Database.Repositories
{
    public class EventTypesRepository : GenericRepository<EventTypesModel>, IEventTypesRepository
    {
        public EventTypesRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<EventTypesModel?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(et => et.Name == name);
        }

        public async Task<bool> EventTypeNameExistsAsync(string name)
        {
            return await _dbSet.AnyAsync(et => et.Name == name);
        }

        public override async Task<EventTypesModel> AddAsync(EventTypesModel entity)
        {
            // Check for duplicate name constraint before adding
            if (await EventTypeNameExistsAsync(entity.Name))
            {
                throw new InvalidOperationException($"Event type with name '{entity.Name}' already exists.");
            }

            return await base.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<EventTypesModel> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var duplicates = new List<string>();
            var seenNames = new HashSet<string>();

            // Check for duplicates within the batch itself
            foreach (var entity in entityList)
            {
                if (seenNames.Contains(entity.Name))
                {
                    duplicates.Add($"'{entity.Name}' (duplicate in batch)");
                }
                else
                {
                    seenNames.Add(entity.Name);
                    // Check against existing records in database
                    if (await EventTypeNameExistsAsync(entity.Name))
                    {
                        duplicates.Add($"'{entity.Name}' (already exists)");
                    }
                }
            }

            if (duplicates.Any())
            {
                throw new InvalidOperationException($"The following event type names are invalid: {string.Join(", ", duplicates)}");
            }

            await base.AddRangeAsync(entityList);
        }

        public override async Task<EventTypesModel?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(et => et.Id == id);
        }
    }
}

