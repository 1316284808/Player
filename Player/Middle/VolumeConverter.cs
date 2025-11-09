using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Middle
{
    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float volume)
            {
                return volume * 100;
            }
            return 100; // 默认100%
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue)
            {
                return (float)(sliderValue / 100);
            }
            return 1.0f; // 默认100%
        }
    }
}