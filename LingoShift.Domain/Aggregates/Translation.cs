using System;
using LingoShift.Domain.ValueObjects;

namespace LingoShift.Domain.Aggregates
{
    public class Translation
    {
        public Guid Id { get; private set; }
        public SourceText SourceText { get; private set; }
        public TranslatedText? TranslatedText { get; private set; }
        public Language SourceLanguage { get; private set; }
        public Language TargetLanguage { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? TranslatedAt { get; private set; }

        public Translation(SourceText sourceText, Language sourceLanguage, Language targetLanguage)
        {
            Id = Guid.NewGuid();
            SourceText = sourceText;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetTranslatedText(TranslatedText translatedText)
        {
            TranslatedText = translatedText;
            TranslatedAt = DateTime.UtcNow;
        }
    }
}
