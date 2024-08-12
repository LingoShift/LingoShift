using LingoShift.Domain.Entities;
using LingoShift.Infrastructure.Data;

namespace LingoShift.Infrastructure.Repositories
{
    public class SettingsRepository
    {
        private readonly AppDbContext _context;

        public SettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetSettingAsync(string key)
        {
            var setting = await _context.Settings.FindAsync(key);
            return setting?.Value;
        }

        public async Task SetSettingAsync(string key, string value)
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetSequencesAsync()
        {
            var sequencesSetting = await GetSettingAsync("Sequences");
            return sequencesSetting?.Split(',') ?? Enumerable.Empty<string>();
        }

        public async Task SetSequencesAsync(IEnumerable<string> sequences)
        {
            var sequencesString = string.Join(",", sequences);
            await SetSettingAsync("Sequences", sequencesString);
        }
    }
}