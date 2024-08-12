using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace LingoShift.Views
{
    public partial class TranslationPopup : Window
    {
        public TranslationPopup()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;

            // Impostazioni per la trasparenza
            TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };
            Background = Brushes.Transparent;

            // Opzionale: abilita l'ombra della finestra
            // this.HasSystemDecorations = false;
            // this.TransparencyBackgroundFallback = Brushes.Transparent;
            // this.UseLayoutRounding = true;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> TranslatedTextProperty =
            AvaloniaProperty.Register<TranslationPopup, string>(nameof(TranslatedText));

        public string TranslatedText
        {
            get => GetValue(TranslatedTextProperty);
            set => SetValue(TranslatedTextProperty, value);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPointerPressedTitleBar(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            // Assicura che la finestra sia trasparente
            TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };
            Background = Brushes.Transparent;
        }
    }
}