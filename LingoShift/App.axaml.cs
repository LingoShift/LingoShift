using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using LingoShift.Application.ApplicationServices;
using Avalonia.Controls;
using System;
using LingoShift.Application.Interfaces;
using LingoShift.Services;
using LingoShift.ViewModels;
using LingoShift.Infrastructure.Repositories;

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
                Startup.InitializeDatabase(services.BuildServiceProvider());
                services.AddSingleton<IPopupService, AvaloniaPopupService>();
                _serviceProvider = services.BuildServiceProvider();

                var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                var settingsrepository = _serviceProvider.GetRequiredService<SettingsRepository>();

                _trayIconManager = new TrayIconManager(mainViewModel, settingsrepository);
                _trayIconManager.Initialize();

                var translationService = _serviceProvider.GetRequiredService<TranslationApplicationService>();
                translationService.RegisterDefaultHotkeys();

                var popupService = _serviceProvider.GetRequiredService<IPopupService>();
                translationService.TranslationCompleted += (sender, e) =>
                {
                    popupService.ShowTranslationPopup(e.TranslatedText);
                };

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