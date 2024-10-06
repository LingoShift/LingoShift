namespace LingoShift.Domain.ValueObjects
{
    public class SourceText
    {
        public string Value { get; }

        public SourceText(string value)
        {
            Value = value;
        }

        public static implicit operator string(SourceText sourceText) => sourceText.Value;
        public static implicit operator SourceText(string value) => new SourceText(value);

        public override string ToString() => Value;
    }
}