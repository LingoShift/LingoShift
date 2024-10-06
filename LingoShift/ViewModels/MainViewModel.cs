using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using LingoShift.Views;

namespace LingoShift.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
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

        public MainViewModel()
        {

        }
    }
}