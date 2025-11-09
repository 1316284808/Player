using System;
using System.Globalization;
using System.Windows.Data;
using Player.Helpers;
using Player.Core.Enums;
namespace Player.Bottom
{
    /// <summary>
    /// 播放状态到图标的转换器
    /// </summary>
    public class BoolToPlayPauseIconConverter : IValueConverter
    {
        /// <summary>
        /// 将播放状态转换为对应的图标
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
            {
                return isPlaying ? IconKind.Pause : IconKind.Play;
            }
            return IconKind.Play; // 默认返回播放图标
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