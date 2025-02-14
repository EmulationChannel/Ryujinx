using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ryujinx.Ava.UI.Helpers
{
    public class BoolToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 0 : 1;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index == 0;
            }
            return false;
        }
    }
}
