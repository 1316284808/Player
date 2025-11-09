using System;
using System.Globalization;
using System.Windows.Data;
using Player.Core.Enums;

namespace Player.Fullscreen
{
    /// <summary>
    /// 布尔值到播放/暂停图标转换器
    /// </summary>
    public class BoolToPlayPauseIconConverter : IValueConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
            {
                return isPlaying ? IconKind.Pause : IconKind.Play;
            }
            
            return IconKind.Play;
        }

        /// <summary>
        /// 反向转换方法（不需要实现）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}