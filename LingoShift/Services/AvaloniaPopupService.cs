using Avalonia.Threading;
using LingoShift.Application.Interfaces;
using LingoShift.Views;

namespace LingoShift.Services
{
    public class AvaloniaPopupService : IPopupService
    {
        public void ShowTranslationPopup(string translatedText)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var popup = new TranslationPopup
                {
                    TranslatedText = translatedText
                };
                popup.Show();
            });
        }
    }
}
