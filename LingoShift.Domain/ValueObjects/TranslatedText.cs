namespace LingoShift.Domain.ValueObjects
{
    public class TranslatedText
    {
        public string Value { get; }

        public TranslatedText(string value)
        {
            Value = value;
        }

        public static implicit operator string(TranslatedText translatedText) => translatedText.Value;
        public static implicit operator TranslatedText(string value) => new TranslatedText(value);

        public override string ToString() => Value;
    }
}