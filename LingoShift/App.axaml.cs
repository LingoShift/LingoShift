using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using System;
using System.Text;
using LingoShift.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using LingoShift.Hubs;

namespace LingoShift
{
    public partial class App : Avalonia.Application
    {
        private TrayIconManager? _trayIconManager;
        private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;
        private IServiceProvider? _serviceProvider;
        private StringBuilder _result = new();
        private IHost _webHost;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _desktopLifetime = desktop;

                // Create and start the web host
                _webHost = CreateWebHostBuilder().Build();
                _webHost.Start();

                _serviceProvider = _webHost.Services;

                _trayIconManager = _serviceProvider.GetRequiredService<TrayIconManager>();
                _trayIconManager.Initialize();

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.ShutdownRequested += OnShutdownRequested;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private IHostBuilder CreateWebHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseUrls("http://localhost:5800")
                        .ConfigureServices((hostContext, services) =>
                        {
                            var configuration = hostContext.Configuration;
                            Startup.ConfigureServices(services, configuration);

                            services.AddControllers();
                            services.AddEndpointsApiExplorer();
                            services.AddSwaggerGen();
                            services.AddSignalR();

                            // Rimuovi questa riga
                            // services.AddSingleton<VoskTranscriptionService>(sp =>
                            //     new VoskTranscriptionService(@"C:\Users\CRS\Downloads\vosk-model-it-0.22\vosk-model-it-0.22"));

                            services.AddCors(options =>
                            {
                                options.AddDefaultPolicy(builder =>
                                {
                                    builder.WithOrigins("http://localhost:3000")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials();
                                });
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseCors();
                            app.UseSwagger();
                            app.UseSwaggerUI();

                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllers();
                                endpoints.MapHub<TranscriptionHub>("/transcriptionHub");
                            });
                        });
                });
        }

        private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            _trayIconManager?.Dispose();
            _webHost?.StopAsync().Wait();
            _webHost?.Dispose();
        }

        public new static App? Current => Avalonia.Application.Current as App;

        public void Shutdown()
        {
            CleanupResources();
            _desktopLifetime?.Shutdown();
        }
    }
}