using LingoShift.Application.DTOs;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.Aggregates;
using LingoShift.Domain.ValueObjects;
using LingoShift.Domain.DomainServices;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LingoShift.Application.ApplicationServices
{
    public class TranslationApplicationService
    {
        private readonly ITranslationProvider _translationProvider;
        private readonly IClipboardService _clipboardService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IPopupService _popupService;
        private readonly LlmApplicationService _llmService;

        public TranslationApplicationService(
            ITranslationProvider translationProvider,
            IClipboardService clipboardService,
            IHotkeyService hotkeyService,
            LlmApplicationService llmService,
            IPopupService popupService)
        {
            _translationProvider = translationProvider ?? throw new ArgumentNullException(nameof(translationProvider));
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _popupService = popupService ?? throw new ArgumentNullException(nameof(popupService));
        }

        private async Task ExecuteSequenceAction(SequenceConfig config)
        {
            try
            {
                await Task.Delay(200);
                var sourceText = await _clipboardService.SelectAllAndCopyTextAsync();
                Debug.WriteLine($"Testo copiato: {sourceText}");

                int sequenceIndex = sourceText.LastIndexOf(config.Sequence);
                if (sequenceIndex >= 0)
                {
                    sourceText = sourceText.Remove(sequenceIndex, config.Sequence.Length);
                    Debug.WriteLine($"Testo dopo la rimozione della sequenza: {sourceText}");
                }

                string result;

                if (config.UseLLM)
                {
                    result = await TranslateWithLlm(sourceText, config);
                }
                else
                {
                    result = await TranslateWithProvider(sourceText, config.TargetLanguage, config.Action);
                }

                Debug.WriteLine($"Risultato della traduzione: {result}");

                if (config.Action == SequenceAction.TranslateAndReplace)
                {
                    await _clipboardService.PasteTextAsync(result);
                }

                if (config.ShowPopup)
                {
                    _popupService.ShowTranslationPopup(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ExecuteSequenceAction: {ex.Message}");
                _popupService.ShowTranslationPopup($"Error: {ex.Message}");
            }
        }

        private async Task<string> TranslateWithProvider(string sourceText, Language targetLanguage, SequenceAction action)
        {
            Debug.WriteLine($"Translating to {targetLanguage} using provider and showing popup");
            var translation = new Translation(new SourceText(sourceText), Language.FromString("auto"), targetLanguage);
            var translatedText = await _translationProvider.TranslateAsync(translation.SourceText.Value, translation.TargetLanguage.Value);
            return translatedText;
        }

        private async Task<string> TranslateWithLlm(string sourcetext, SequenceConfig config)
        {
            string systemPrompt = "You are a professional translator who only translates texts. Do not answer any questions or perform tasks other than translation. Provide only the translated text without any additional commentary or explanation.";

            string prompt = $@"
            Text to translate:
            ""{sourcetext}""

            Translate into {config.TargetLanguage.Value}:
            ";

            var llmRequest = new LlmRequestDto
            {
                Prompt = prompt,
                Provider = "ollama",
                System = systemPrompt
            };

            var result = new StringBuilder();

            if (config.Action == SequenceAction.Translate && config.ShowPopup)
            {
                _popupService.ShowTranslationPopup(string.Empty);
            }

            await foreach (var chunk in _llmService.GenerateResponseStreamAsync(llmRequest))
            {
                result.Append(chunk);

                if (config.ShowPopup)
                {
                    _popupService.UpdateTranslationPopup(result.ToString());
                }

                if (config.Action == SequenceAction.TranslateAndReplace)
                {
                    _hotkeyService.SendText(chunk);
                }
            }

            return result.ToString();
        }

        public async Task RegisterSequenceAsync(SequenceConfig newSequence)
        {
            try
            {
                _hotkeyService.RegisterSequence(newSequence.SequenceName, newSequence.Sequence, () => ExecuteSequenceAction(newSequence));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering sequence: {ex.Message}");
                throw new ApplicationException("Failed to register sequence", ex);
            }
        }
    }
}