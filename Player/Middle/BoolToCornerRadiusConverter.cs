using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Player.Middle
{
    /// <summary>
    /// 布尔值到圆角半径转换器
    /// 全屏时圆角为0，非全屏时圆角为8
    /// </summary>
    public class BoolToCornerRadiusConverter : IValueConverter
    {
        public static readonly BoolToCornerRadiusConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? new CornerRadius(0) : new CornerRadius(8);
            }
            return new CornerRadius(8);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到内边距转换器
    /// 全屏时内边距为0，非全屏时为5
    /// </summary>
    public class BoolToPaddingConverter : IValueConverter
    {
        public static readonly BoolToPaddingConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? new Thickness(0) : new Thickness(5);
            }
            return new Thickness(5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到外边距转换器
    /// 全屏时外边距为0，非全屏时为10
    /// </summary>
    public class BoolToMarginConverter : IValueConverter
    {
        public static readonly BoolToMarginConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? new Thickness(0) : new Thickness(10);
            }
            return new Thickness(10);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到模糊半径转换器
    /// 全屏时模糊半径为0，非全屏时为8
    /// </summary>
    public class BoolToBlurRadiusConverter : IValueConverter
    {
        public static readonly BoolToBlurRadiusConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? 0d : 8d;
            }
            return 8d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到边框粗细转换器
    /// 全屏时边框粗细为0，非全屏时为2
    /// </summary>
    public class BoolToBorderThicknessConverter : IValueConverter
    {
        public static readonly BoolToBorderThicknessConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? new Thickness(0) : new Thickness(2);
            }
            return new Thickness(2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}