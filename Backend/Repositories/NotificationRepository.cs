
using Microsoft.EntityFrameworkCore;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDBContext _context;

        public NotificationRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Notifications notification)
        {
            _context.notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notifications>> GetAllAsync()
        {
            return await _context.notifications
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
