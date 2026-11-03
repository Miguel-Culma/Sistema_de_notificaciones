
using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
