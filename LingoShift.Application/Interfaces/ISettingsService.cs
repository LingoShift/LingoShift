using LingoShift.Domain.ValueObjects;

namespace LingoShift.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<string> GetSettingAsync(string key);
        Task SetSettingAsync(string key, string value);
        Task<List<SequenceConfig>> GetSequenceConfigsAsync();
        Task SetSequenceConfigsAsync(List<SequenceConfig> configs);
    }
}