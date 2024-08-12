using LingoShift.Application.DTOs;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.Aggregates;
using LingoShift.Domain.ValueObjects;
using LingoShift.Domain.DomainServices;
using LingoShift.Application.Events;
using System.Diagnostics;

namespace LingoShift.Application.ApplicationServices
{
    public class TranslationApplicationService
    {
        private readonly ITranslationProvider _translationProvider;
        private readonly IClipboardService _clipboardService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IPopupService _popupService;
        private readonly LlmApplicationService _llmService;

        public event EventHandler<TranslationCompletedEvent> TranslationCompleted;

        public TranslationApplicationService(
            ITranslationProvider translationProvider,
            IClipboardService clipboardService,
            IHotkeyService hotkeyService,
            LlmApplicationService llmService,
            IPopupService popupService)
        {
            _translationProvider = translationProvider;
            _clipboardService = clipboardService;
            _hotkeyService = hotkeyService;
            _llmService = llmService;
            _popupService = popupService;
        }

        public void RegisterDefaultHotkeys()
        {
            _hotkeyService.RegisterSequence("LingoTriggerOpenEn", "<openen", async () => await TranslateAndShowPopupAsync("en"));
            _hotkeyService.RegisterSequence("LingoTriggerOpenIt", "<openit", async () => await TranslateAndShowPopupAsync("it"));

            _hotkeyService.RegisterSequence("LingoTriggerReplaceEn", "<repen", async () => await TranslateAndReplaceAsync("en"));
            _hotkeyService.RegisterSequence("LingoTriggerReplaceIt", "<repit", async () => await TranslateAndReplaceAsync("it"));

            _hotkeyService.RegisterSequence("LingoTriggerLlmIt", "<llmit", async () => await TranslateAndShowPopupWithLlmAsync("Italian"));
            _hotkeyService.RegisterSequence("LingoTriggerLlmEn", "<llmen", async () => await TranslateAndShowPopupWithLlmAsync("English"));

            _hotkeyService.RegisterSequence("LingoTriggerLlmFormattedIt", "<llmfmit", async () => await TranslateAndShowPopupWithLlmFormattedAsync("Italian"));
            _hotkeyService.RegisterSequence("LingoTriggerLlmFormattedEn", "<llfmen", async () => await TranslateAndShowPopupWithLlmFormattedAsync("English"));
        }

        private async Task TranslateAndReplaceAsync(string targetLanguage)
        {
            Debug.WriteLine($"Translating to {targetLanguage} and replacing clipboard text");

            var sourceText = await _clipboardService.GetTextAsync();
            var result = await TranslateAsync(new TranslationRequestDto
            {
                SourceText = sourceText,
                SourceLanguageCode = "auto",
                TargetLanguageCode = targetLanguage
            });
            await _clipboardService.SetTextAsync(result.TranslatedText);
        }

        private async Task TranslateAndShowPopupAsync(string targetLanguage)
        {
            Debug.WriteLine($"Translating to {targetLanguage} and showing popup");

            var sourceText = await _clipboardService.GetTextAsync();
            var result = await TranslateAsync(new TranslationRequestDto
            {
                SourceText = sourceText,
                SourceLanguageCode = "auto",
                TargetLanguageCode = targetLanguage
            });
            OnTranslationCompleted(new TranslationCompletedEvent(result.TranslatedText));
        }

        public async Task TranslateAndShowPopupWithLlmAsync(string targetLanguage)
        {
            Debug.WriteLine($"Translating to {targetLanguage} using LLM and showing popup");

            var llmRequest = new LlmRequestDto
            {
                Prompt = $"Translate the following text to {targetLanguage}:\n\n" + await _clipboardService.GetTextAsync(),
                Provider = "openai"
            };
            var result = await _llmService.GenerateResponse(llmRequest);

            OnTranslationCompleted(new TranslationCompletedEvent(result.Content));
        }

        public async Task TranslateAndShowPopupWithLlmFormattedAsync(string targetLanguage)
        {
            Debug.WriteLine($"Translating to {targetLanguage} using LLM and showing popup");

            var prompt = $"Translate the following text to {targetLanguage}:\n\n" + await _clipboardService.GetTextAsync();
            string instructions = "Please translate the text using formal language and include bullet points for lists. Ensure correct punctuation, grammar, spelling, capitalization, tone, style, formatting, structure, terminology, register, voice, and tense.";
            string example = "Example translation: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.'";
            var llmRequest = new LlmRequestDto
            {
                Prompt = prompt + "\n\n" + instructions + "\n\n" + example,
                Provider = "openai"
            };

            var result = await _llmService.GenerateResponse(llmRequest);

            OnTranslationCompleted(new TranslationCompletedEvent(result.Content));
        }

        protected virtual void OnTranslationCompleted(TranslationCompletedEvent e)
        {
            TranslationCompleted?.Invoke(this, e);
        }

        public async Task<TranslationResultDto> TranslateAsync(TranslationRequestDto request)
        {
            var sourceLanguage = new Language(request.SourceLanguageCode, "");
            var targetLanguage = new Language(request.TargetLanguageCode, "");
            var translation = new Translation(new SourceText(request.SourceText), sourceLanguage, targetLanguage);

            var translatedText = await _translationProvider.TranslateAsync(translation.Source.Value, translation.TargetLanguage.Code);
            translation.SetResult(new TranslatedText(translatedText));

            return new TranslationResultDto
            {
                SourceText = translation.Source.Value,
                TranslatedText = translation.Result.Value,
                TargetLanguageCode = translation.TargetLanguage.Code
            };
        }
    }
}