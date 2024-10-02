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

        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(LlmPrompt prompt)
        {
            var requestBody = new
            {
                model = "llama3.2",
                prompt = prompt.Content,
                stream = true,
                system = prompt.System
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Crea un HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = content
            };

            // Usa SendAsync con HttpCompletionOption.ResponseHeadersRead
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Leggi il contenuto come stream
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line == "[DONE]")
                {
                    break;
                }

                // Rimuovi eventuale prefisso "data: "
                if (line.StartsWith("data: "))
                {
                    line = line.Substring("data: ".Length);
                }

                string generatedText = null;

                try
                {
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(line);

                    if (responseObject.TryGetProperty("response", out JsonElement responseElement))
                    {
                        generatedText = responseElement.GetString();
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Errore nel parsing del JSON: {ex.Message}");
                }

                if (!string.IsNullOrEmpty(generatedText))
                {
                    yield return generatedText;
                }
            }
        }

        public async Task<LlmResponse> GenerateResponseAsync(LlmPrompt prompt)
        {
            var requestBody = new
            {
                model = "llama3.2",
                prompt = prompt.Content,
                stream = false,
                format = "json"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var generatedText = responseObject.GetProperty("response").GetString();

            var responsefinal = JsonSerializer.Deserialize<responseContent>(generatedText);

            return new LlmResponse(responsefinal!.TranslatedText);
        }

        class responseContent
        {
            public string TranslatedText { get; set; }
        }
    }
}
