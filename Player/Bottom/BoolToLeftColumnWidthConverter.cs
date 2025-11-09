using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Player.Bottom
{
    /// <summary>
    /// 全屏状态到左侧列宽度的转换器
    /// </summary>
    public class BoolToLeftColumnWidthConverter : IValueConverter
    {
        /// <summary>
        /// 将全屏状态转换为对应的左侧列宽度
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                // 全屏时折叠左侧控件（宽度为0），非全屏时恢复默认宽度（auto）
                return isFullscreen ? new GridLength(0) : GridLength.Auto;
            }
            return GridLength.Auto; // 默认自动宽度
        }

        /// <summary>
        /// 反向转换方法（未使用）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}