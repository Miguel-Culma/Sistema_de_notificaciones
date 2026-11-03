
using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(string id);
        Task<List<Client>> GetAllAsync();
    }
}
