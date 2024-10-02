using LingoShift.Application.DTOs;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.Aggregates;
using LingoShift.Domain.ValueObjects;
using LingoShift.Domain.DomainServices;
using LingoShift.Application.Events;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace LingoShift.Application.ApplicationServices
{
    public class TranslationApplicationService
    {
        private readonly IDispatcherService _dispatcherService;

        private readonly ITranslationProvider _translationProvider;
        private readonly IClipboardService _clipboardService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IPopupService _popupService;
        private readonly LlmApplicationService _llmService;
        private readonly ISettingsService _settingsService;

        private readonly List<SequenceConfig> _defaultSequences =
        [
            new SequenceConfig
            {
                SequenceName = "LingoTriggerOpenEn",
                Sequence = "<en",
                TargetLanguage = Language.English,
                Action = SequenceAction.Translate,
                UseLLM = false,
                ShowPopup = true
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerOpenIt",
                Sequence = "<it",
                TargetLanguage = Language.Italian,
                Action = SequenceAction.Translate,
                UseLLM = false,
                ShowPopup = true
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerReplaceEn",
                Sequence = "<ren",
                TargetLanguage = Language.English,
                Action = SequenceAction.TranslateAndReplace,
                UseLLM = false,
                ShowPopup = false
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerReplaceIt",
                Sequence = "<rit",
                TargetLanguage = Language.Italian,
                Action = SequenceAction.TranslateAndReplace,
                UseLLM = false,
                ShowPopup = false
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerLlmIt",
                Sequence = "<lmit",
                TargetLanguage = Language.Italian,
                Action = SequenceAction.Translate,
                UseLLM = true,
                ShowPopup = true
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerLlmEn",
                Sequence = "<lmen",
                TargetLanguage = Language.English,
                Action = SequenceAction.Translate,
                UseLLM = true,
                ShowPopup = true
            },
             new SequenceConfig
            {
                SequenceName = "LingoTriggerLlmReplaceIt",
                Sequence = "<rlmit",
                TargetLanguage = Language.Italian,
                Action = SequenceAction.TranslateAndReplace,
                UseLLM = true,
                ShowPopup = false
            },
            new SequenceConfig
            {
                SequenceName = "LingoTriggerLlmReplaceEn",
                Sequence = "<rlmen",
                TargetLanguage = Language.English,
                Action = SequenceAction.TranslateAndReplace,
                UseLLM = true,
                ShowPopup = false
            },
        ];

        public event EventHandler<TranslationCompletedEvent> TranslationCompleted;
        public event EventHandler<TranslationProgressUpdatedEvent> TranslationProgressUpdated;


        private List<SequenceConfig> _sequenceConfigs;

        public TranslationApplicationService(
            ITranslationProvider translationProvider,
            IClipboardService clipboardService,
            IHotkeyService hotkeyService,
            LlmApplicationService llmService,
            IPopupService popupService,
            IDispatcherService dispatcherService,
            ISettingsService settingsService)
        {
            _translationProvider = translationProvider;
            _clipboardService = clipboardService;
            _hotkeyService = hotkeyService;
            _llmService = llmService;
            _popupService = popupService;
            _settingsService = settingsService;
            _dispatcherService = dispatcherService;
        }

        public async Task RegisterSequencesAsync()
        {
            _sequenceConfigs = await _settingsService.GetSequenceConfigsAsync();
            foreach (var config in _sequenceConfigs)
            {
                _hotkeyService.RegisterSequence(config.SequenceName, config.Sequence, () => ExecuteSequenceAction(config));
            }
        }

        public async Task RegisterDefaultSequencesAsync()
        {
            await _settingsService.SetSequenceConfigsAsync(_defaultSequences);
            await RegisterSequencesAsync();
        }

        private async Task ExecuteSequenceAction(SequenceConfig config)
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
                await TranslateWithLlm(sourceText, config);
                return;
            }
            else
            {
                result = await TranslateWithProvider(sourceText, config.TargetLanguage.Code, config.Action);
            }

            Debug.WriteLine($"Risultato della traduzione: {result}");

            if (config.Action.Equals(SequenceAction.TranslateAndReplace))
            {
                await _clipboardService.PasteTextAsync(result);
            }


            if (config.ShowPopup)
            {
                OnTranslationCompleted(result);
            }
        }

        private async Task<string> TranslateWithProvider(string sourceText, string targetLanguage, string action)
        {
            Debug.WriteLine($"Translating to {targetLanguage} using provider and showing popup");
            var translation = new Translation(new SourceText(sourceText), new Language("auto", ""), new Language(targetLanguage, ""));
            var translatedText = await _translationProvider.TranslateAsync(translation.Source.Value, translation.TargetLanguage.Code);
            return translatedText;
        }

        private async Task<string> TranslateWithLlm(string sourcetext, SequenceConfig config)
        {
            // Define the system prompt to set the model's role
            string systemPrompt = "You are a professional translator who only translates texts. Do not answer any questions or perform tasks other than translation. Provide only the translated text without any additional commentary or explanation.";

            // Create the prompt to send to the model
            string prompt = $@"
            Text to translate:
            ""{sourcetext}""

            Translate into {config.TargetLanguage.Name}:
            ";

            var llmRequest = new LlmRequestDto
            {
                Prompt = prompt,
                Provider = "ollama",
                System = systemPrompt
            };

            var result = new StringBuilder();

            if (config.Action.Equals(SequenceAction.Translate) && config.ShowPopup)
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

                if (config.Action.Equals(SequenceAction.TranslateAndReplace))
                {
                    _hotkeyService.SendText(chunk);
                }
            }

            return result.ToString();
        }


        protected virtual void OnTranslationProgressUpdated(string partialResult, bool isCompleted)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                TranslationProgressUpdated?.Invoke(this, new TranslationProgressUpdatedEvent(partialResult, isCompleted));
            });
        }

        protected virtual void OnTranslationCompleted(string translatedText)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                TranslationCompleted?.Invoke(this, new TranslationCompletedEvent(translatedText));
            });
        }
    }
}