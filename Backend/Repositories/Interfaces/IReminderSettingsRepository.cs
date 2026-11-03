using PruebaTecnica_MonoLegal.Models;

namespace PruebaTecnica_MonoLegal.Repositories.Interfaces
{
    public interface IReminderSettingsRepository
    {
        Task<ReminderSettings> GetOrCreateAsync();

        Task<ReminderSettings> UpdateIntervalAsync(int intervalMinutes);

        Task<ReminderSettings> UpdateEnabledAsync(bool enabled);
    }
}