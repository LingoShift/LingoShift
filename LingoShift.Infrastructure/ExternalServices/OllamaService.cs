using System.Text;
using System.Text.Json;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.ValueObjects;

namespace LingoShift.Infrastructure.ExternalServices
{
    public class OllamaService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://localhost:11434/api/generate";

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LlmResponse> GenerateResponseAsync(LlmPrompt prompt)
        {
            var requestBody = new
            {
                model = "llama3.1",
                prompt = prompt.Content
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var generatedText = responseObject.GetProperty("response").GetString();

            return new LlmResponse(generatedText);
        }
    }
}