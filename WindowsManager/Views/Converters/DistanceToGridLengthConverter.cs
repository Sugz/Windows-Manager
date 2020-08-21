using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using WindowsManager.Views.Controls;

namespace WindowsManager.Views.Converters
{
    public class DistanceToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int distance = (int)value;
            Position position = (Position)parameter;
            return position switch
            {
                Position.Side => distance + 3d,
                Position.Center => distance + 6d,
                _ => 0,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int distance = System.Convert.ToInt32(value);
            Position position = (Position)parameter;
            return position switch
            {
                Position.Side => distance - 3,
                Position.Center => distance - 6,
                _ => 0,
            };
        }
    }
}
