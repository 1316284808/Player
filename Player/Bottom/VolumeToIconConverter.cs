using System;
using System.Globalization;
using System.Windows.Data;
using Player.Helpers;
using Player.Core.Enums;

namespace Player.Bottom
{
    /// <summary>
    /// 音量值到图标转换器
    /// </summary>
    public class VolumeToIconConverter : IValueConverter
    {
        /// <summary>
        /// 将音量值转换为对应的图标类型
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 支持 int 和 double 类型的音量值
            double volume = 0;
            if (value is int intVolume)
            {
                volume = intVolume;
            }
            else if (value is double doubleVolume)
            {
                volume = doubleVolume;
            }
            else
            {
                return IconKind.VolumeHigh; // 默认返回高音量图标
            }
            
            if (volume <= 0)
                return IconKind.VolumeMute;
            else if (volume < 30)
                return IconKind.VolumeLow;
            else if (volume < 70)
                return IconKind.VolumeMedium;
            else
                return IconKind.VolumeHigh;
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