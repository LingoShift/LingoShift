using Avalonia.Threading;
using LingoShift.Views;
using System;
using LingoShift.Application.Interfaces;

namespace LingoShift.Services;

public class AvaloniaPopupService : IPopupService
{
    private TranslationPopup _popup;

    public void ShowTranslationPopup(string translatedText)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_popup == null)
            {
                _popup = new TranslationPopup
                {
                    TranslatedText = translatedText
                };
                _popup.Closed += Popup_Closed;
                _popup.Show();
            }
            else
            {
                _popup.TranslatedText = translatedText;
            }
        });
    }

    public void UpdateTranslationPopup(string translatedText)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_popup != null)
            {
                _popup.TranslatedText = translatedText;
            }
        });
    }

    public void CloseTranslationPopup()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_popup != null)
            {
                _popup.Close();
                _popup = null;
            }
        });
    }

    private void Popup_Closed(object sender, EventArgs e)
    {
        _popup = null;
    }
}
