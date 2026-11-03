
using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateAsync(Notifications notification);
        Task<List<Notifications>> GetAllAsync();
    }

}
