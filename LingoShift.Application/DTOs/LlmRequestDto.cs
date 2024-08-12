namespace LingoShift.Application.DTOs
{
    public class LlmRequestDto
    {
        /// <summary>
        /// The prompt to use for the request.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// The provider to use for the request.
        /// Possible values are OpenAI, Anthropic, or Ollama.
        /// Default is OpenAI.
        /// </summary>
        public string Provider { get; set; } = "openai";
    }
}