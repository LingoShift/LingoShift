namespace LingoShift.Application.Interfaces;

public interface ITranslationProvider
{
    Task<string> TranslateAsync(string sourceText, string targetLanguageCode);
}
