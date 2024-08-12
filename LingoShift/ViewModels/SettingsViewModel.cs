using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using ReactiveUI;
using System.Threading.Tasks;
using LingoShift.Infrastructure.Repositories;
using System.Reactive;

namespace LingoShift.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsRepository _settingsRepository;

        private string _openAiApiKey;
        public string OpenAiApiKey
        {
            get => _openAiApiKey;
            set => this.RaiseAndSetIfChanged(ref _openAiApiKey, value);
        }

        public ObservableCollection<string> Sequences { get; } = new ObservableCollection<string>();

        private string _newSequence;
        public string NewSequence
        {
            get => _newSequence;
            set => this.RaiseAndSetIfChanged(ref _newSequence, value);
        }

        public ICommand AddSequenceCommand { get; }
        public ReactiveCommand<string, Unit> RemoveSequenceCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel(SettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;

            AddSequenceCommand = ReactiveCommand.Create(AddSequence);
            RemoveSequenceCommand = ReactiveCommand.Create<string>(RemoveSequence);
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettings);

            // Load initial settings
            LoadSettings();
        }

        private void AddSequence()
        {
            if (!string.IsNullOrWhiteSpace(NewSequence))
            {
                Sequences.Add(NewSequence);
                NewSequence = string.Empty;
            }
        }

        private void RemoveSequence(string sequence)
        {
            Sequences.Remove(sequence);
        }

        private async Task SaveSettings()
        {
            await _settingsRepository.SetSettingAsync("OpenAiApiKey", OpenAiApiKey);
            await _settingsRepository.SetSequencesAsync(Sequences);
        }

        private async void LoadSettings()
        {
            OpenAiApiKey = await _settingsRepository.GetSettingAsync("OpenAiApiKey");
            var sequences = await _settingsRepository.GetSequencesAsync();
            Sequences.Clear();
            foreach (var sequence in sequences)
            {
                Sequences.Add(sequence);
            }
        }
    }
}