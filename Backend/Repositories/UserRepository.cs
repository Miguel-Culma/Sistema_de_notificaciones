using Microsoft.EntityFrameworkCore;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDBContext _context;

        public UserRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.users
                .FirstOrDefaultAsync(user => user.userName == userName);
        }
    }
}