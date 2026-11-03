using Microsoft.EntityFrameworkCore;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Models;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;

namespace PruebaTecnica_MonoLegal.Repositories
{
    public class ReminderSettingsRepository : IReminderSettingsRepository
    {
        private readonly AppDBContext _context;

        public ReminderSettingsRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<ReminderSettings> GetOrCreateAsync()
        {
            var settings = await _context.reminderSettings
                .FirstOrDefaultAsync();

            if (settings != null)
                return settings;

            settings = new ReminderSettings
            {
                intervalMinutes = 5,
                enabled = true,
                updatedAt = DateTime.UtcNow
            };

            _context.reminderSettings.Add(settings);

            await _context.SaveChangesAsync();

            return settings;
        }

        public async Task<ReminderSettings> UpdateIntervalAsync(
            int intervalMinutes)
        {
            var settings = await GetOrCreateAsync();

            settings.intervalMinutes = intervalMinutes;
            settings.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return settings;
        }

        public async Task<ReminderSettings> UpdateEnabledAsync(
            bool enabled)
        {
            var settings = await GetOrCreateAsync();

            settings.enabled = enabled;
            settings.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return settings;
        }
    }
}