using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.ComponentModel;

namespace LingoShift.Views
{
    public partial class TranslationPopup : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

            // Rimuovi i decori di sistema
            SystemDecorations = SystemDecorations.None;

            // Imposta l'evento per permettere il trascinamento della finestra
            PointerPressed += OnPointerPressedTitleBar;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private string _translatedText;
        public string TranslatedText
        {
            get => _translatedText;
            set
            {
                if (_translatedText != value)
                {
                    _translatedText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TranslatedText)));
                }
            }
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
