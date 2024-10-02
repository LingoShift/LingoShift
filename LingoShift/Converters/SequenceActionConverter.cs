using Avalonia.Data.Converters;
using LingoShift.Domain.ValueObjects;
using System;
using System.Globalization;
using System.Linq;

namespace LingoShift.Converters
{
    public class SequenceActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SequenceAction action)
            {
                var availableActions = SequenceAction.GetValues();
                return availableActions.FirstOrDefault(a => a.Value == action.Value) ?? action;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class LanguageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Language language)
            {
                var availableLanguages = Language.GetValues();
                return availableLanguages.FirstOrDefault(l => l.Code == language.Code) ?? language;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}