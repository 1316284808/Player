using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Player.Bottom
{
    /// <summary>
    /// 全屏状态到窗口样式的转换器
    /// </summary>
    public class BoolToWindowStyleConverter : IValueConverter
    {
        /// <summary>
        /// 将全屏状态转换为对应的窗口样式
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                // 全屏时隐藏标题栏，非全屏时显示标题栏
                return isFullscreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
            }
            return WindowStyle.SingleBorderWindow; // 默认显示标题栏
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