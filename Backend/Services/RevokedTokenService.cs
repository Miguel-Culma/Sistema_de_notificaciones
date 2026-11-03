
using System.Collections.Concurrent;
using PruebaTecnica_MonoLegal.Services.Interfaces;

namespace PruebaTecnica_MonoLegal.Services
{
    public class RevokedTokenService : IRevokedTokenService
    {
        private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

        public void RevokeToken(string tokenId, DateTime expiration)
        {
            _revokedTokens.TryAdd(tokenId, expiration);
        }

        public bool IsTokenRevoked(string tokenId)
        {
            if (!_revokedTokens.TryGetValue(tokenId, out var expiration))
            {
                return false;
            }

            if (expiration <= DateTime.UtcNow)
            {
                _revokedTokens.TryRemove(tokenId, out _);
                return false;
            }

            return true;
        }
    }
}
