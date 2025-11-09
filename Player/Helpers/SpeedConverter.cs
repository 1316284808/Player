using System;
using System.Globalization;
using System.Windows.Data;

namespace Player.Helpers
{
    /// <summary>
    /// 播放速度转换器 - 用于在double和字符串值之间进行转换
    /// 解决ComboBox SelectedValue绑定的类型匹配问题
    /// </summary>
    public class SpeedConverter : IValueConverter
    {
        /// <summary>
        /// 将double转换为字符串（如1.0 -> "1.0"）
        /// 这是从ViewModel到UI的转换
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double speed)
            {
                // 返回格式化的字符串，确保与ComboBoxItem的Content格式完全一致
                return speed.ToString("0.0", culture);
            }
            return "1.0"; // 默认值
        }

        /// <summary>
        /// 将字符串转换为double（如"1.0" -> 1.0）
        /// 这是从UI到ViewModel的转换
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 1.0;
            }
            
            string speedString = value.ToString();
            
            if (!string.IsNullOrEmpty(speedString))
            {
                // 尝试直接解析字符串值
                if (double.TryParse(speedString, NumberStyles.Any, culture, out double result))
                {
                    return result;
                }
            }
            return 1.0; // 默认速度
        }
    }
}