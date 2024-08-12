using System.Text;
using System.Text.Json;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.ValueObjects;
using LingoShift.Infrastructure.Repositories;

namespace LingoShift.Infrastructure.ExternalServices
{
    public class OpenAiService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly SettingsRepository _settingsRepository;
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public OpenAiService(HttpClient httpClient, SettingsRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LlmResponse> GenerateResponseAsync(LlmPrompt prompt)
        {
            var apiKey = await _settingsRepository.GetSettingAsync("OpenAiApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not set. Please configure it in the settings.");
            }

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt.Content }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var generatedText = responseObject.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return new LlmResponse(generatedText);
        }
    }
}