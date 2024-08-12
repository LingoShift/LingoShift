using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using LingoShift.Views;
using LingoShift.Infrastructure.Repositories;

namespace LingoShift.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SettingsRepository _settingsRepository;

        private string _currentTranslationService;
        public string CurrentTranslationService
        {
            get => _currentTranslationService;
            set => this.RaiseAndSetIfChanged(ref _currentTranslationService, value);
        }

        private string _lastTranslationStatus;
        public string LastTranslationStatus
        {
            get => _lastTranslationStatus;
            set => this.RaiseAndSetIfChanged(ref _lastTranslationStatus, value);
        }

        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

        public MainViewModel(SettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);
            CurrentTranslationService = "Not set"; // Default value
            LastTranslationStatus = "No translation performed yet"; // Default value
        }

        private void OpenSettings()
        {
            var settingsViewModel = new SettingsViewModel(_settingsRepository);
            var settingsWindow = new Window
            {
                Content = new SettingsView
                {
                    DataContext = settingsViewModel
                },
                Title = "Settings",
                Width = 400,
                Height = 500
            };
            settingsWindow.Show();
        }
    }
}