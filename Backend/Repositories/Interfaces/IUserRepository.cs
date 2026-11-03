using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
    }
}