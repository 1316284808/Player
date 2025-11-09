using System;

namespace Player.Core.Services
{
    /// <summary>
    /// 主题服务接口 - 用于解耦平台特定的主题管理实现
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// 应用主题色（支持自定义颜色）
        /// </summary>
        /// <param name="colorHex">主题颜色值（十六进制格式）</param>
        void ApplyTheme(string colorHex);

        /// <summary>
        /// 刷新所有控件的主题属性
        /// </summary>
        void RefreshThemeProperties();

        /// <summary>
        /// 验证颜色值是否有效
        /// </summary>
        /// <param name="colorHex">颜色值</param>
        /// <returns>是否有效</returns>
        bool IsValidColor(string colorHex);
    }
}