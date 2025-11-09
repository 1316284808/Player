using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Middle
{
    public class BoolToFullscreenIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? "ExitFullscreen" : "Fullscreen";
            }
            return "Fullscreen"; // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}