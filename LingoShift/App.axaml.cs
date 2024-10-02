using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LingoShift.Application.ApplicationServices;
using Avalonia.Controls;
using System;
using LingoShift.Application.Interfaces;
using LingoShift.Services;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Text;

namespace LingoShift
{
    public partial class App : Avalonia.Application
    {
        private TrayIconManager? _trayIconManager;
        private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;
        private IServiceProvider? _serviceProvider;
        private VoiceActivator voiceActivator;
        private StringBuilder result = new StringBuilder();
        private IPopupService _popupService;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _desktopLifetime = desktop;
                var services = new ServiceCollection();
                Startup.ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Inizializza il database e altri servizi sul thread dell'UI
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Startup.InitializeDatabase(_serviceProvider);

                    _trayIconManager = _serviceProvider.GetRequiredService<TrayIconManager>();
                    _trayIconManager.Initialize();

                    var translationService = _serviceProvider.GetRequiredService<TranslationApplicationService>();
                    translationService.RegisterDefaultSequencesAsync();

                    _popupService = _serviceProvider.GetRequiredService<IPopupService>();
                    var dispatcherService = _serviceProvider.GetRequiredService<IDispatcherService>();

                    translationService.TranslationCompleted += (sender, e) =>
                    {
                        dispatcherService.InvokeAsync(() =>
                        {
                            _popupService.ShowTranslationPopup(e.TranslatedText);
                        });
                    };
                });

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.ShutdownRequested += OnShutdownRequested;

                StartVoiceRecognition();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void StartVoiceRecognition()
        {
            Task.Run(() =>
            {
                try
                {
                    var modelPath = @"C:\Users\CRS\Downloads\vosk-model-it-0.22\vosk-model-it-0.22";
                    var activationPhrase = "ciao";
                    var deactivationPhrase = "grazie";

                    voiceActivator = new VoiceActivator(modelPath, activationPhrase, deactivationPhrase);

                    voiceActivator.OnActivation += () =>
                    {
                        _popupService.ShowTranslationPopup("Assistente attivato!");
                    };

                    voiceActivator.OnTranscription += (transcription) =>
                    {
                        if (string.IsNullOrEmpty(transcription))
                            return;
                        //result.Append(transcription);
                        //_popupService.UpdateTranslationPopup(result.ToString());
                        _popupService.ShowTranslationPopup(transcription);
                    };

                    voiceActivator.OnDeactivation += () =>
                    {
                        _popupService.ShowTranslationPopup("Assistente disattivato!");
                    };

                    voiceActivator.Start();
                }
                catch (Exception ex)
                {
                    var popupService = _serviceProvider!.GetRequiredService<IPopupService>();
                    var dispatcherService = _serviceProvider.GetRequiredService<IDispatcherService>();
                    dispatcherService.InvokeAsync(() =>
                    {
                        popupService.ShowTranslationPopup(ex.Message);
                    });
                }
            });
        }


        private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            _trayIconManager?.Dispose();
        }

        public static new App? Current => Avalonia.Application.Current as App;

        public void Shutdown()
        {
            CleanupResources();
            _desktopLifetime?.Shutdown();
        }
    }
}