using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LingoShift.Application.ApplicationServices;
using Avalonia.Controls;
using System;
using LingoShift.Application.Interfaces;
using LingoShift.Services;
using Avalonia.Threading;

namespace LingoShift
{
    public partial class App : Avalonia.Application
    {
        private TrayIconManager? _trayIconManager;
        private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;
        private IServiceProvider? _serviceProvider;

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

                    var popupService = _serviceProvider.GetRequiredService<IPopupService>();
                    var dispatcherService = _serviceProvider.GetRequiredService<IDispatcherService>();

                    translationService.TranslationCompleted += (sender, e) =>
                    {
                        dispatcherService.InvokeAsync(() =>
                        {
                            popupService.ShowTranslationPopup(e.TranslatedText);
                        });
                    };
                });

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.ShutdownRequested += OnShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
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