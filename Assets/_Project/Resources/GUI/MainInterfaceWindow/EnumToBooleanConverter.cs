using System;
using System.Globalization;
using Noesis;

namespace NuclearBand.Game
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Binding.DoNothing;
            if (!(value is bool)) return Binding.DoNothing;
            if ((bool)value)
            {
                try
                {
                    return Enum.Parse(targetType, parameter.ToString());
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }

            return Binding.DoNothing;
        }
    }
}

