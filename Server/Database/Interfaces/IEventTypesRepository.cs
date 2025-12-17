using Server.Models.Events;

namespace Server.Database.Interfaces
{
    public interface IEventTypesRepository : IGenericRepository<EventTypesModel>
    {
        Task<EventTypesModel?> GetByNameAsync(string name);
        Task<bool> EventTypeNameExistsAsync(string name);
    }
}

