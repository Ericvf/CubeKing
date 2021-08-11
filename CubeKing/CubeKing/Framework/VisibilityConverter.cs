using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CubeKing.Core
{
    public class VisibilityConverter : IValueConverter
    {
        public bool Inversed { get; set; }
        public Visibility FalseValue { get; set; }

        public VisibilityConverter()
        {
            this.FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = false;

            if (value is bool)
                visibility = (bool)value;

            if (value is string)
                visibility = !string.IsNullOrEmpty((string)value);

            if (value is double)
                visibility = (double)value != 0;

            if (value is int)
                visibility = (int)value != 0;
            
            if (Inversed)
                visibility = !visibility;

            return visibility ? Visibility.Visible : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }
}
