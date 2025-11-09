using System.Collections.Generic;
using System.Windows.Media;

namespace Player.Services
{
    /// <summary>
    /// 主题服务接口
    /// 提供主题切换、管理和获取的核心功能
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// 应用指定的主题色
        /// </summary>
        /// <param name="colorHex">颜色的十六进制字符串表示（如 "#2196F3"）</param>
        void ApplyTheme(string colorHex);
        
        /// <summary>
        /// 获取所有可用的预设主题
        /// </summary>
        /// <returns>主题名称和颜色的字典</returns>
        Dictionary<string, string> GetAvailableThemes();
        
        /// <summary>
        /// 获取当前应用的主题颜色
        /// </summary>
        /// <returns>当前主题颜色的十六进制字符串</returns>
        string GetCurrentThemeColor();
        
        /// <summary>
        /// 验证颜色格式是否正确
        /// </summary>
        /// <param name="colorHex">颜色的十六进制字符串</param>
        /// <returns>是否为有效的颜色格式</returns>
        bool IsValidColor(string colorHex);
    }
}