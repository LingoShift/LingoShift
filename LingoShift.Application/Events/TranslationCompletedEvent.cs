namespace LingoShift.Application.Events;

public class TranslationCompletedEvent
{
    public string TranslatedText { get; }

    public TranslationCompletedEvent(string translatedText)
    {
        TranslatedText = translatedText;
    }
}
public class TranslationProgressUpdatedEvent : EventArgs
{
    public string PartialResult { get; }
    public bool IsCompleted { get; }

    public TranslationProgressUpdatedEvent(string partialResult, bool isCompleted)
    {
        PartialResult = partialResult;
        IsCompleted = isCompleted;
    }
}
