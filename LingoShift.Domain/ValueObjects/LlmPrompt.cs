namespace LingoShift.Domain.ValueObjects
{
    public class LlmPrompt
    {
        public string Content { get; }
        public object System { get; set; }

        public LlmPrompt(string content, string system)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Prompt content cannot be empty", nameof(content));

            if (string.IsNullOrWhiteSpace(system))
                throw new ArgumentException("System content cannot be empty", nameof(system));

            Content = content;
            System = system;
        }
    }
}