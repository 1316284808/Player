using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Player.Left
{
    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? 0 : 300; // 折叠时宽度为0，展开时宽度为300
            }
            return 300; // 默认展开状态
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}