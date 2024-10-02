using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using LingoShift.Views;

namespace LingoShift.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SettingsViewModel _settingsViewModel;

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

        public MainViewModel(SettingsViewModel settingsViewModel)
        {
            OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);
            CurrentTranslationService = "Not set"; // Default value
            LastTranslationStatus = "No translation performed yet"; // Default value
            _settingsViewModel = settingsViewModel;

        }

        private void OpenSettings()
        {
            var settingsWindow = new Window
            {
                Content = new SettingsView
                {
                    DataContext = _settingsViewModel
                },
                Title = "Settings",
                Width = 400,
                Height = 500
            };
            settingsWindow.Show();
        }
    }
}