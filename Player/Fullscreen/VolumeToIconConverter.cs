using System;
using System.Globalization;
using System.Windows.Data;
using Player.Core.Enums;

namespace Player.Fullscreen
{
    /// <summary>
    /// 音量值到音量图标转换器
    /// </summary>
    public class VolumeToIconConverter : IValueConverter
    {
        /// <summary>
        /// 转换方法
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int volume)
            {
                return volume switch
                {
                    0 => IconKind.VolumeMute,
                    < 33 => IconKind.VolumeLow,
                    < 66 => IconKind.VolumeMedium,
                    _ => IconKind.VolumeHigh
                };
            }
            
            return IconKind.VolumeMedium;
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