using CarboLifeAPI;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Displays a number rounded to three decimals, so accumulated floating point noise
    /// does not surface in the grid - a volume summed from model elements arrives as
    /// 1.9999999 rather than 2.
    ///
    /// Trailing zeros are dropped, which matches the DataViewer's RoundValue handler used
    /// by the Total Volume, Density, Mass and CO2e columns, so a bound column looks the
    /// same as its neighbours.
    ///
    /// Safe on a two-way binding: an unparseable edit is refused rather than being turned
    /// into zero.
    /// </summary>
    public class RoundTo3DecimalsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            double d;

            if (value is double)
                d = (double)value;
            else if (value is float)
                d = (float)value;
            else if (value is decimal)
                d = (double)(decimal)value;
            else if (value is int)
                d = (int)value;
            else
                return value;

            if (double.IsNaN(d) || double.IsInfinity(d))
                return value;

            return Math.Round(d, 3).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return Binding.DoNothing;

            //Utils.ConvertMeToDouble is what the rest of the application uses to read a
            //typed number, so a comma decimal separator behaves the same here as elsewhere.
            //It returns 0 for anything it cannot read, so check that separately - writing a
            //silent zero into a volume would quietly destroy the row.
            double parsed = Utils.ConvertMeToDouble(text);

            if (parsed == 0 && !looksLikeZero(text))
                return Binding.DoNothing;

            return parsed;
        }

        private static bool looksLikeZero(string text)
        {
            foreach (char c in text)
            {
                if (c != '0' && c != '.' && c != ',' && c != '-' && c != '+' && !char.IsWhiteSpace(c))
                    return false;
            }

            return true;
        }
    }
}
