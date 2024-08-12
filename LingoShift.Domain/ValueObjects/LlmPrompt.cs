namespace LingoShift.Domain.ValueObjects
{
    public class LlmPrompt
    {
        public string Content { get; }

        public LlmPrompt(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Prompt content cannot be empty", nameof(content));

            Content = content;
        }
    }
}