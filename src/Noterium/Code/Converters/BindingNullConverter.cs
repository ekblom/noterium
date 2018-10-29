using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Noterium.Code.Converters
{
    public class BindingNullConverter<T> : IValueConverter
    {
        public BindingNullConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T) value, True);
        }
    }
}