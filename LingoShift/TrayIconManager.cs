using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LingoShift.Application.Interfaces;
using LingoShift.ViewModels;
using LingoShift.Views;
using System;

namespace LingoShift;

public class TrayIconManager : IDisposable
{
    private TrayIcon? _trayIcon;
    private MainViewModel _mainViewModel;
    private SettingsViewModel _settingsViewModel;
    private IDispatcherService _dispatcherService;
    
    public TrayIconManager(MainViewModel mainViewModel, SettingsViewModel settingsViewModel, IDispatcherService dispatcherService)
    {
        _mainViewModel = mainViewModel;
        _settingsViewModel = settingsViewModel;
        _dispatcherService = dispatcherService;
    }

    public void Initialize()
    {
        _trayIcon = new TrayIcon();

        if (Avalonia.Application.Current?.Resources["AppIcon"] is Bitmap iconBitmap)
        {
            _trayIcon.Icon = new WindowIcon(iconBitmap);
        }
        else
        {
            // Fallback if the icon can't be loaded
            _trayIcon.Icon = CreateDefaultIcon();
        }

        _trayIcon.ToolTipText = "LingoShift";
        _trayIcon.Clicked += TrayIcon_Clicked;

        var menu = new NativeMenu();
        var openSettingsItem = new NativeMenuItem("Open Settings");
        openSettingsItem.Click += OpenSettings;
        menu.Add(openSettingsItem);

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += Exit;
        menu.Add(exitItem);
        _trayIcon.Menu = menu;

        _trayIcon.IsVisible = true;
    }

    private WindowIcon CreateDefaultIcon()
    {
        // Create a simple default icon
        var size = 16;
        var bitmap = new WriteableBitmap(new PixelSize(size, size), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        using (var context = bitmap.Lock())
        {
            unsafe
            {
                var ptr = (uint*)context.Address;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        ptr[y * size + x] = 0xFF000000; // Opaque black
            }
        }
        return new WindowIcon(bitmap);
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        // You can add logic here for when the icon is clicked
    }

    private void OpenSettings(object? sender, EventArgs e)
    {
        _dispatcherService.InvokeAsync(() =>
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
        });
    }

    private void Exit(object? sender, EventArgs e)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}