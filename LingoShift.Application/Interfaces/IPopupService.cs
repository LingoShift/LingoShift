﻿namespace LingoShift.Application.Interfaces;

public interface IPopupService
{
    void ShowTranslationPopup(string translatedText);
    void UpdateTranslationPopup(string translatedText);
    void CloseTranslationPopup();
}
