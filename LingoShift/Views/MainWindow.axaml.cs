using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LingoShift.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new Window
        {
            Title = "Settings",
            Content = new SettingsView(),
            Width = 400,
            Height = 300
        };

        settingsWindow.Show();
    }
}
