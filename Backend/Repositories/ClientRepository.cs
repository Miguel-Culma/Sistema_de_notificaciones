
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDBContext _context;

        public ClientRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return null;

            return await _context.clients
                .FirstOrDefaultAsync(c => c._id == objectId);
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.clients.ToListAsync();
        }
    }
}
