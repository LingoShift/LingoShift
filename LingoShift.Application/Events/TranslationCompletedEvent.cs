namespace LingoShift.Application.Events;

public class TranslationCompletedEvent
{
    public string TranslatedText { get; }

    public TranslationCompletedEvent(string translatedText)
    {
        TranslatedText = translatedText;
    }
}