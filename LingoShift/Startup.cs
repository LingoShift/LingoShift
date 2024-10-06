using LingoShift.Application.ApplicationServices;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.DomainServices;
using LingoShift.Domain.Repositories;
using LingoShift.Hubs;
using LingoShift.Infrastructure.ExternalServices;
using LingoShift.Infrastructure.PlatformSpecificServices;
using LingoShift.Infrastructure.Repositories;
using LingoShift.Services;
using LingoShift.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LingoShift
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(configure => configure.AddConsole());

            services.AddSingleton<ITranslationProvider, GoogleTranslateAdapter>();
            services.AddSingleton<IClipboardService, WindowsClipboardService>();
            services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
            services.AddSingleton<WindowsNativeClipboardService>();
            services.AddSingleton<IPopupService, AvaloniaPopupService>();
            services.AddHttpClient();

            // MongoDB Configuration
            services.Configure<MongoDbSettings>(option =>
            {
                new MongoDbSettings()
                {
                    DatabaseName = "LingoShift"
                    ,
                    ConnectionString = "mongodb://localhost:27017"
                };
            });

            //services.AddSingleton<ITranscriptionService, SpeechRecognitionService>();
            services.AddSingleton<ITranscriptionService>(sp =>
                new VoskTranscriptionService(
                    sp.GetRequiredService<ILogger<VoskTranscriptionService>>(),
                    sp.GetRequiredService<IHubContext<TranscriptionHub>>(),
                    @"C:\Users\CRS\Downloads\vosk-model-it-0.22\vosk-model-it-0.22"
                )
            );

            services.AddSingleton<ISequenceConfigRepository, MongoSequenceConfigRepository>();

            services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
            services.AddSingleton<ILlmService, OllamaService>();
            services.AddSingleton<LlmApplicationService>();

            services.AddSingleton<WhisperTranscriptionService>();
            services.AddTransient<TranslationApplicationService>();

            services.AddTransient<MainViewModel>();

            services.AddSingleton<TrayIconManager>();
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}