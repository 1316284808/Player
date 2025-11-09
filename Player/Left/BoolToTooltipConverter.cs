using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Left
{
    public class BoolToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? "展开侧边栏" : "折叠侧边栏";
            }
            return "折叠侧边栏"; // 默认值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}