using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Left
{
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? "ChevronRight" : "ChevronLeft";
            }
            return "ChevronLeft"; // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}