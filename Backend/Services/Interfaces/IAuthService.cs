
using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> ValidateCredentialsAsync(
            string userName,
            string password);
    }


}
