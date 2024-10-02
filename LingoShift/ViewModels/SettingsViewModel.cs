using Avalonia.ReactiveUI;
using LingoShift.Application.ApplicationServices;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.ValueObjects;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LingoShift.ViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        private readonly ISettingsService _settingsService;
        private readonly TranslationApplicationService _translationService;
        private readonly IDispatcherService _dispatcherService;

        private ObservableCollection<SequenceConfig> _sequences;
        public ObservableCollection<SequenceConfig> Sequences
        {
            get => _sequences;
            private set => this.RaiseAndSetIfChanged(ref _sequences, value);
        }

        public List<SequenceAction> AvailableActions { get; } = SequenceAction.GetValues().ToList();
        public List<Language> AvailableLanguages { get; } = Language.GetValues().ToList();

        private SequenceConfig _newSequenceConfig;
        public SequenceConfig NewSequenceConfig
        {
            get => _newSequenceConfig;
            set => this.RaiseAndSetIfChanged(ref _newSequenceConfig, value);
        }

        public ReactiveCommand<Unit, Unit> AddSequenceCommand { get; }
        public ReactiveCommand<SequenceConfig, Unit> RemoveSequenceCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; }

        public SettingsViewModel(ISettingsService settingsService, TranslationApplicationService translationService, IDispatcherService dispatcherService)
        {
            _settingsService = settingsService;
            _translationService = translationService;
            _dispatcherService = dispatcherService;

            Sequences = new ObservableCollection<SequenceConfig>();

            NewSequenceConfig = new SequenceConfig
            {
                Action = AvailableActions.First(),
                TargetLanguage = AvailableLanguages.First()
            };

            var canAddSequence = this.WhenAnyValue(x => x.NewSequenceConfig.Sequence)
                .Select(sequence => !string.IsNullOrWhiteSpace(sequence))
                .ObserveOn(RxApp.MainThreadScheduler);

            AddSequenceCommand = ReactiveCommand.CreateFromTask(AddSequence, canAddSequence);
            RemoveSequenceCommand = ReactiveCommand.Create<SequenceConfig>(RemoveSequence);
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettings);

            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

            LoadSettings();
        }

        private async Task AddSequence()
        {
            await _dispatcherService.InvokeAsync(() =>
            {
                var newConfig = new SequenceConfig
                {
                    SequenceName = NewSequenceConfig.SequenceName,
                    Sequence = NewSequenceConfig.Sequence,
                    Action = AvailableActions.FirstOrDefault(a => a.Value == NewSequenceConfig.Action.Value) ?? AvailableActions.First(),
                    TargetLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == NewSequenceConfig.TargetLanguage.Code) ?? AvailableLanguages.First(),
                    UseLLM = NewSequenceConfig.UseLLM,
                    ShowPopup = NewSequenceConfig.ShowPopup
                };

                Sequences.Add(newConfig);

                NewSequenceConfig = new SequenceConfig
                {
                    Action = AvailableActions.First(),
                    TargetLanguage = AvailableLanguages.First()
                };
            });
        }

        private void RemoveSequence(SequenceConfig sequenceConfig)
        {
            _dispatcherService.InvokeAsync(() =>
            {
                Sequences.Remove(sequenceConfig);
            });
        }

        private async Task SaveSettings()
        {
            var sequencesList = await _dispatcherService.InvokeAsync(() => Sequences.ToList());
            await _settingsService.SetSequenceConfigsAsync(sequencesList);
            await _translationService.RegisterSequencesAsync();
        }

        private async void LoadSettings()
        {
            var sequenceConfigs = await _settingsService.GetSequenceConfigsAsync();
            await _dispatcherService.InvokeAsync(() =>
            {
                Sequences.Clear();
                foreach (var config in sequenceConfigs)
                {
                    config.Action = AvailableActions.FirstOrDefault(a => a.Value == config.Action.Value) ?? AvailableActions.First();
                    config.TargetLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == config.TargetLanguage.Code) ?? AvailableLanguages.First();
                    Sequences.Add(config);
                }
            });
        }
    }
}