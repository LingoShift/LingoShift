using LingoShift.Application.DTOs;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.ValueObjects;

namespace LingoShift.Application.ApplicationServices
{
    public class LlmApplicationService
    {
        private readonly ILlmService _openAiService;
        private readonly ILlmService _anthropicService;
        private readonly ILlmService _ollamaService;

        public LlmApplicationService(
            ILlmService openAiService,
            ILlmService anthropicService,
            ILlmService ollamaService)
        {
            _openAiService = openAiService;
            _anthropicService = anthropicService;
            _ollamaService = ollamaService;
        }

        public async Task<LlmResponseDto> GenerateResponse(LlmRequestDto request)
        {
            var prompt = new LlmPrompt(request.Prompt, request.System);
            LlmResponse response;

            switch (request.Provider.ToLower())
            {
                case "openai":
                    response = await _openAiService.GenerateResponseAsync(prompt);
                    break;
                case "anthropic":
                    response = await _anthropicService.GenerateResponseAsync(prompt);
                    break;
                case "ollama":
                    response = await _ollamaService.GenerateResponseAsync(prompt);
                    break;
                default:
                    throw new ArgumentException("Invalid LLM provider specified", nameof(request.Provider));
            }

            return new LlmResponseDto { Content = response.Content };
        }
        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(LlmRequestDto request)
        {
            var prompt = new LlmPrompt(request.Prompt, request.System);
            IAsyncEnumerable<string> responseStream;

            switch (request.Provider.ToLower())
            {
                case "openai":
                    responseStream = _openAiService.GenerateResponseStreamAsync(prompt);
                    break;
                case "anthropic":
                    responseStream = _anthropicService.GenerateResponseStreamAsync(prompt);
                    break;
                case "ollama":
                    responseStream = _ollamaService.GenerateResponseStreamAsync(prompt);
                    break;
                default:
                    throw new ArgumentException("Invalid LLM provider specified", nameof(request.Provider));
            }

            await foreach (var chunk in responseStream)
            {
                yield return chunk;
            }
        }
    }
}