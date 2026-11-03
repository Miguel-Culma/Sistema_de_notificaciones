
namespace PruebaTecnica_MonoLegal.Services.Interfaces
{
    public interface IRevokedTokenService
    {
        void RevokeToken(string tokenId, DateTime expiration);

        bool IsTokenRevoked(string tokenId);
    }
}
