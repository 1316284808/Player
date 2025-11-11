using System;
using System.Windows.Media;

namespace Player.Core.Services
{
    /// <summary>
    /// 主题服务实现 - 跨平台主题管理
    /// </summary>
    public class CoreThemeService // : IThemeService (接口已移至Player.Services命名空间)
    {
        /// <summary>
        /// 主题应用事件
        /// </summary>
        public event EventHandler<ThemeAppliedEventArgs>? OnThemeApplied;

        /// <summary>
        /// 主题属性刷新事件
        /// </summary>
        public event EventHandler? OnThemePropertiesRefreshed;

        /// <summary>
        /// 应用主题色（支持自定义颜色）
        /// </summary>
        /// <param name="colorHex">主题颜色值（十六进制格式）</param>
        public void ApplyTheme(string colorHex)
        {
            try
            {
                // 验证颜色值
                if (!IsValidColor(colorHex))
                {
                    throw new ArgumentException("无效的颜色值格式");
                }

                // 主题应用逻辑由具体的平台实现处理
                // 这里只负责验证颜色值，实际应用在WPF项目中处理
                OnThemeApplied?.Invoke(this, new ThemeAppliedEventArgs(colorHex));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"应用主题失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 刷新所有控件的主题属性
        /// </summary>
        public void RefreshThemeProperties()
        {
            // 主题属性刷新逻辑由具体的平台实现处理
            // 这里只提供空实现，实际刷新在WPF项目中处理
            OnThemePropertiesRefreshed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 验证颜色值是否有效
        /// </summary>
        /// <param name="colorHex">颜色值</param>
        /// <returns>是否有效</returns>
        public bool IsValidColor(string colorHex)
        {
            if (string.IsNullOrWhiteSpace(colorHex))
                return false;

            if (!colorHex.StartsWith("#"))
                return false;

            if (colorHex.Length != 7) // #RRGGBB 格式
                return false;

            try
            {
                // 尝试解析颜色值
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 主题应用事件参数
    /// </summary>
    public class ThemeAppliedEventArgs : EventArgs
    {
        public string ColorHex { get; }

        public ThemeAppliedEventArgs(string colorHex)
        {
            ColorHex = colorHex;
        }
    }
}