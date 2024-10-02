using LingoShift.Application.Interfaces;
using LingoShift.Domain.ValueObjects;
using LingoShift.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace LingoShift.Infrastructure.Services
{
    public class SettingsService : ISettingsService, IDisposable
    {
        private readonly SettingsRepository _settingsRepository;
        private readonly IDispatcherService _dispatcher;
        private const string SequenceConfigKey = "SequenceConfigs";

        public SettingsService(SettingsRepository settingsRepository, IDispatcherService dispatcher)
        {
            _settingsRepository = settingsRepository;
            _dispatcher = dispatcher;
        }

        public async Task<List<SequenceConfig>> GetSequenceConfigsAsync()
        {
            return await _dispatcher.InvokeAsync(async () =>
            {
                string json = await _settingsRepository.GetSettingAsync(SequenceConfigKey);
                if (string.IsNullOrEmpty(json))
                {
                    return new List<SequenceConfig>();
                }
                return JsonSerializer.Deserialize<List<SequenceConfig>>(json) ?? new List<SequenceConfig>();
            });
        }

        public async Task<string> GetSettingAsync(string key)
        {
            return await _dispatcher.InvokeAsync(() => _settingsRepository.GetSettingAsync(key));
        }

        public async Task SetSequenceConfigsAsync(List<SequenceConfig> configs)
        {
            await _dispatcher.InvokeAsync(async () =>
            {
                string json = JsonSerializer.Serialize(configs);
                await _settingsRepository.SetSettingAsync(SequenceConfigKey, json);
            });
        }

        public async Task SetSettingAsync(string key, string value)
        {
            await _dispatcher.InvokeAsync(() => _settingsRepository.SetSettingAsync(key, value));
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources here if needed
        }
    }
}