using System;
using System.Globalization;
using System.Windows.Data;
using Player.Core.Enums;

namespace Player.Bottom
{
    /// <summary>
    /// 全屏状态到图标的转换器
    /// </summary>
    public class BoolToFullscreenIconConverter : IValueConverter
    {
        /// <summary>
        /// 将全屏状态转换为对应的图标
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                // 根据全屏状态返回不同的图标
                return isFullscreen ? IconKind.FullscreenExit : IconKind.Fullscreen;
            }
            return IconKind.Fullscreen; // 默认返回全屏图标
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