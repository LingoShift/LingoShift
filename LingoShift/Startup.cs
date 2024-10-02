using LingoShift.Application.ApplicationServices;
using LingoShift.Application.Interfaces;
using LingoShift.Domain.DomainServices;
using LingoShift.Infrastructure.Data;
using LingoShift.Infrastructure.ExternalServices;
using LingoShift.Infrastructure.PlatformSpecificServices;
using LingoShift.Infrastructure.Repositories;
using LingoShift.Infrastructure.Services;
using LingoShift.Services;
using LingoShift.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LingoShift;

public static class Startup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<ITranslationProvider, GoogleTranslateAdapter>();
        services.AddSingleton<IClipboardService, WindowsClipboardService>();
        services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
        services.AddSingleton<WindowsNativeClipboardService>();
        services.AddSingleton<IPopupService, AvaloniaPopupService>();

        services.AddHttpClient();

        // Database configuration
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=lingoshift.db"));

        services.AddScoped<SettingsRepository>();
        services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        //services.AddSingleton<ILlmService, OpenAiService>();
        // services.AddSingleton<ILlmService, AnthropicService>();
        services.AddSingleton<ILlmService, OllamaService>();

        services.AddSingleton<LlmApplicationService>();

        services.AddTransient<TranslationApplicationService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>(provider =>
            new SettingsViewModel(
                provider.GetRequiredService<ISettingsService>(),
                provider.GetRequiredService<TranslationApplicationService>(),
                provider.GetRequiredService<IDispatcherService>()
            )
        );

        services.AddSingleton<TrayIconManager>();
    }

    public static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.MigrateDatabase();
        }
    }
}
