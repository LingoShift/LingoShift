using LingoShift.Domain.ValueObjects;

namespace LingoShift.Domain.Aggregates;

public class Translation
{
    public Guid Id { get; private set; }
    public SourceText Source { get; private set; }
    public TranslatedText Result { get; private set; }
    public Language SourceLanguage { get; private set; }
    public Language TargetLanguage { get; private set; }

    public Translation(SourceText source, Language sourceLanguage, Language targetLanguage)
    {
        Id = Guid.NewGuid();
        Source = source;
        SourceLanguage = sourceLanguage;
        TargetLanguage = targetLanguage;
    }

    public void SetResult(TranslatedText result)
    {
        Result = result;
    }
}
