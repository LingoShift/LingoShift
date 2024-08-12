namespace LingoShift.Domain.ValueObjects
{
    public class LlmResponse
    {
        public string Content { get; }

        public LlmResponse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Response content cannot be empty", nameof(content));

            Content = content;
        }
    }
}