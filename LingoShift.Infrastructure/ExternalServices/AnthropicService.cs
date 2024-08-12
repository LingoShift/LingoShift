using System.Text;
using System.Text.Json;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.ValueObjects;

namespace LingoShift.Infrastructure.ExternalServices
{
    public class AnthropicService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.anthropic.com/v1/complete";

        public AnthropicService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<LlmResponse> GenerateResponseAsync(LlmPrompt prompt)
        {
            var requestBody = new
            {
                model = "claude-2",
                prompt = $"\n\nHuman: {prompt.Content}\n\nAssistant:",
                max_tokens_to_sample = 300
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var generatedText = responseObject.GetProperty("completion").GetString();

            return new LlmResponse(generatedText);
        }
    }
}