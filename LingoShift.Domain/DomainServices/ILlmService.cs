using LingoShift.Domain.ValueObjects;

namespace LingoShift.Domain.DomainServices
{
    public interface ILlmService
    {
        Task<LlmResponse> GenerateResponseAsync(LlmPrompt prompt);
        IAsyncEnumerable<string> GenerateResponseStreamAsync(LlmPrompt prompt);
    }
}