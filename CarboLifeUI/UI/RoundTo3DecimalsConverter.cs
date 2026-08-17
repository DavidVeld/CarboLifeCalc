using System;
using System.Globalization;
using System.Windows.Data;

public class RoundTo3DecimalsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return Math.Round(d, 3).ToString("F3");
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (double.TryParse(value?.ToString(), out double d))
            return Math.Round(d, 3);
        return 0.0;
    }
}