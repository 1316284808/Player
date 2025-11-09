using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Player.Helpers
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                try
                {
                    // 尝试将颜色字符串转换为Color对象
                    Color color;
                    if (colorString.StartsWith("#"))
                    {
                        // 处理十六进制颜色值
                        color = (Color)ColorConverter.ConvertFromString(colorString);
                    }
                    else
                    {
                        // 尝试直接转换
                        color = (Color)ColorConverter.ConvertFromString(colorString);
                    }
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // 如果转换失败，返回默认颜色
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}