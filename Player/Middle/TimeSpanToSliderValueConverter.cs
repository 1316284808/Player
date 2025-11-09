using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Middle
{
    public class TimeSpanToSliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan currentTime)
            {
                // 通过parameter获取总时长
                if (parameter is TimeSpan totalTime && totalTime.TotalSeconds > 0)
                {
                    double percentage = (currentTime.TotalSeconds / totalTime.TotalSeconds) * 100;
                    return Math.Max(0, Math.Min(100, percentage)); // 确保在0-100范围内
                }
                
                // 如果没有总时长参数，返回基于当前时间的合理值
                return (currentTime.TotalSeconds / 60.0) * 100; // 假设60秒为最大时长
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue && parameter is TimeSpan totalTime)
            {
                double seconds = (sliderValue / 100.0) * totalTime.TotalSeconds;
                return TimeSpan.FromSeconds(seconds);
            }
            return TimeSpan.Zero;
        }
    }
}