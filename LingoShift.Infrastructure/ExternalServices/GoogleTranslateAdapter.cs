using GTranslate.Translators;
using LingoShift.Application.Interfaces;

namespace LingoShift.Infrastructure.ExternalServices;

public class GoogleTranslateAdapter : ITranslationProvider
{
    private readonly GoogleTranslator _translator;
    public GoogleTranslateAdapter()
    {
        _translator = new GoogleTranslator();
    }

    public async Task<string> TranslateAsync(string sourceText, string targetLanguageCode)
    {
        var translateResult = await _translator.TranslateAsync(sourceText, targetLanguageCode);

        return translateResult.Translation;
    }
}
