using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Player.Helpers;
using System.Collections.Concurrent;

namespace Player.Services
{
    /// <summary>
    /// 主题服务实现类
    /// 负责应用主题、管理主题资源和提供主题相关功能
    /// </summary>
    public class ThemeService : IThemeService
    {
        // 预设主题颜色
        private static readonly Dictionary<string, string> _presetThemes = new Dictionary<string, string>
        {
            {"紫色", "#7B68EE"},
            {"蓝色", "#2196F3"},
            {"绿色", "#4CAF50"},
            {"橙色", "#FF9800"},
            {"红色", "#F44336"}
        };

        // 主题文件映射
        private static readonly Dictionary<string, string> _themeFiles = new Dictionary<string, string>
        {
            {"#7B68EE", "Themes/PurpleTheme.xaml"},
            {"#2196F3", "Themes/BlueTheme.xaml"},
            {"#4CAF50", "Themes/GreenTheme.xaml"},
            {"#FF9800", "Themes/OrangeTheme.xaml"},
            {"#F44336", "Themes/RedTheme.xaml"}
        };

        /// <summary>
        /// 应用指定的主题色
        /// </summary>
        public void ApplyTheme(string colorHex)
        {            if (!IsValidColor(colorHex))
            {
                throw new ArgumentException("无效的颜色格式");
            }

            try
            {
                var app = Application.Current;
                if (app == null) return;

                // 移除所有主题和样式资源
                var resourcesToRemove = app.Resources.MergedDictionaries
                    .Where(d => d.Source != null && 
                               (d.Source.OriginalString.Contains("Themes/") || 
                                d.Source.OriginalString.Contains("CustomStyles.xaml")))
                    .ToList();

                foreach (var resource in resourcesToRemove)
                {
                    app.Resources.MergedDictionaries.Remove(resource);
                }

                // 创建新的主题资源字典
                ResourceDictionary themeResource = CreateThemeResourceDictionary(colorHex);

                // 先添加主题资源
                app.Resources.MergedDictionaries.Add(themeResource);
                
                // 然后添加共享样式
                LoadSharedStyles(app);
                
                // 使用队列优化的异步刷新方法
                if (app.MainWindow != null)
                {
                    Task.Run(async () => 
                    {
                        await RefreshDynamicResourcesAsync(app.MainWindow);
                    });
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"主题切换失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取所有可用的预设主题
        /// </summary>
        public Dictionary<string, string> GetAvailableThemes()
        {            return new Dictionary<string, string>(_presetThemes);
        }

        /// <summary>
        /// 获取当前应用的主题颜色
        /// </summary>
        public string GetCurrentThemeColor()
        {            var app = Application.Current;
            if (app == null) return "#7B68EE"; // 默认紫色

            // 尝试从应用资源中获取当前主题颜色
            if (app.Resources.Contains("PrimaryColor"))
            {
                var resource = app.Resources["PrimaryColor"];
                if (resource is Color color)
                {
                    return ColorToHex(color);
                }
                // 处理可能是SolidColorBrush的情况
                else if (resource is SolidColorBrush brush)
                {
                    return ColorToHex(brush.Color);
                }
            }

            // 如果找不到，检查资源字典源
            var currentTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (currentTheme != null)
            {
                return _themeFiles.FirstOrDefault(x => x.Value == currentTheme.Source.OriginalString).Key ?? "#7B68EE";
            }

            return "#7B68EE"; // 默认值
        }

        /// <summary>
        /// 验证颜色格式是否正确
        /// </summary>
        public bool IsValidColor(string colorHex)
        {            try
            {
                // 正确的颜色转换方式 - 显式转换为Color结构类型
                Color color = (Color)ColorConverter.ConvertFromString(colorHex);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 创建主题资源字典
        /// </summary>
        private ResourceDictionary CreateThemeResourceDictionary(string colorHex)
        {            // 检查是否是预设主题
            if (_themeFiles.TryGetValue(colorHex, out string themeFile))
            {
                // 使用预设主题文件
                return new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.Relative)
                };
            }
            else
            {
                // 动态生成自定义主题
                Color primaryColor = (Color)ColorConverter.ConvertFromString(colorHex);
                return CreateDynamicThemeDictionary(primaryColor);
            }
        }

        /// <summary>
        /// 动态生成主题字典
        /// </summary>
        private ResourceDictionary CreateDynamicThemeDictionary(Color primaryColor)
        {            var theme = new ResourceDictionary();

            // 计算颜色变体
            Color primaryLight = LightenColor(primaryColor, 0.2f);
            Color primaryDark = DarkenColor(primaryColor, 0.2f);
            Color secondary = CreateSecondaryColor(primaryColor);
            Color secondaryLight = LightenColor(secondary, 0.1f);
            Color secondaryDark = DarkenColor(secondary, 0.1f);
            Color accent = AdjustSaturation(primaryColor, 1.2f);

            // 添加颜色资源
            theme["PrimaryColor"] = primaryColor;
            theme["PrimaryLightColor"] = primaryLight;
            theme["PrimaryDarkColor"] = primaryDark;
            theme["SecondaryColor"] = secondary;
            theme["SecondaryLightColor"] = secondaryLight;
            theme["SecondaryDarkColor"] = secondaryDark;
            theme["AccentColor"] = accent;

            // 添加画刷资源
            theme["PrimaryBrush"] = new SolidColorBrush(primaryColor);
            theme["PrimaryLightBrush"] = new SolidColorBrush(primaryLight);
            theme["PrimaryDarkBrush"] = new SolidColorBrush(primaryDark);
            theme["SecondaryBrush"] = new SolidColorBrush(secondary);
            theme["SecondaryLightBrush"] = new SolidColorBrush(secondaryLight);
            theme["SecondaryDarkBrush"] = new SolidColorBrush(secondaryDark);
            theme["AccentBrush"] = new SolidColorBrush(accent);

            // 背景色使用固定值
            theme["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            theme["BackgroundMediumBrush"] = new SolidColorBrush(Color.FromRgb(250, 250, 250)); // #FAFAFA
            theme["BackgroundLightBrush"] = new SolidColorBrush(Color.FromRgb(248, 248, 248)); // #F8F8F8

            // 列表和滚动条样式资源
            theme["ListBoxItemSelectedBackground"] = new SolidColorBrush(secondary);
            theme["ListBoxItemSelectedBorder"] = new SolidColorBrush(primaryColor);
            theme["ListBoxItemHoverBackground"] = new SolidColorBrush(secondaryLight);
            theme["ListBoxItemHoverBorder"] = new SolidColorBrush(accent);
            theme["ScrollBarThumbDefault"] = new SolidColorBrush(Color.FromRgb(176, 176, 176));
            theme["ScrollBarThumbHover"] = new SolidColorBrush(primaryColor);

            return theme;
        }

        /// <summary>
        /// 加载共享样式
        /// </summary>
        private void LoadSharedStyles(Application app)
        {            try
            {
                var customStyles = new ResourceDictionary
                {
                    Source = new Uri("Themes/CustomStyles.xaml", UriKind.Relative)
                };
                app.Resources.MergedDictionaries.Add(customStyles);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("加载共享样式失败", ex);
            }
        }

        /// <summary>
        /// 使用队列优化的异步刷新方法
        /// </summary>
        private async Task RefreshDynamicResourcesAsync(FrameworkElement rootElement)
        {            if (rootElement == null)
                return;

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 使用队列代替递归，避免栈溢出
                    var queue = new ConcurrentQueue<DependencyObject>();
                    queue.Enqueue(rootElement);

                    while (queue.Count > 0)
                    {
                        if (queue.TryDequeue(out var current))
                        {
                            // 刷新当前控件的动态资源
                            if (current is FrameworkElement element)
                            {
                                try
                                {
                                    // 优化：只刷新常用的视觉相关属性，避免过度刷新
                                    // 刷新背景和前景色
                                    element.InvalidateProperty(Control.BackgroundProperty);
                                    element.InvalidateProperty(Control.ForegroundProperty);
                                    element.InvalidateProperty(Control.BorderBrushProperty);
                                    element.InvalidateProperty(Control.BorderThicknessProperty);
                                    element.InvalidateProperty(Control.FontFamilyProperty);
                                    element.InvalidateProperty(Control.FontSizeProperty);
                                    element.InvalidateProperty(Control.FontStyleProperty);
                                    element.InvalidateProperty(Control.FontWeightProperty);
                                    
                                    // 对于按钮和其他交互控件
                                    if (element is ButtonBase)
                                    {
                                        element.InvalidateProperty(ButtonBase.IsPressedProperty);
                                        element.InvalidateProperty(ButtonBase.IsEnabledProperty);
                                    }
                                    
                                    // 对于列表项
                                    if (element is ListBoxItem)
                                    {
                                        element.InvalidateProperty(ListBoxItem.IsSelectedProperty);
                                    }

                                    // 强制刷新模板和视觉
                                    if (element is Control control)
                                    {
                                        control.InvalidateProperty(Control.TemplateProperty);
                                        control.InvalidateVisual();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SystemNotificationHelper.ShowError($"刷新控件属性失败: {ex.Message}");
                                }
                            }

                            // 将子控件加入队列
                            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                            {
                                var child = VisualTreeHelper.GetChild(current, i);
                                if (child != null)
                                {
                                    queue.Enqueue(child);
                                }
                            }
                        }
                    }
                });
            });
        }

        // 颜色处理辅助方法
        private Color LightenColor(Color color, float factor)
        {            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + (255 - color.R) * factor),
                (byte)Math.Min(255, color.G + (255 - color.G) * factor),
                (byte)Math.Min(255, color.B + (255 - color.B) * factor)
            );
        }

        private Color DarkenColor(Color color, float factor)
        {            return Color.FromArgb(
                color.A,
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }

        private Color CreateSecondaryColor(Color primary)
        {            // 转换为HSL色彩空间
            float h, s, l;
            RgbToHsl(primary, out h, out s, out l);
            
            // 创建一个适合作为次要颜色的变体
            s *= 0.3f; // 降低饱和度
            l = Math.Max(0.85f, l); // 提高亮度
            
            return HslToRgb(h, s, l, primary.A);
        }

        private Color AdjustSaturation(Color color, float factor)
        {            float h, s, l;
            RgbToHsl(color, out h, out s, out l);
            s = Math.Min(1.0f, s * factor);
            return HslToRgb(h, s, l, color.A);
        }

        private void RgbToHsl(Color rgb, out float h, out float s, out float l)
        {            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;
            
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            
            l = (max + min) / 2f;
            
            if (delta == 0)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l < 0.5f ? delta / (max + min) : delta / (2f - max - min);
                
                if (max == r)
                    h = ((g - b) / delta + (g < b ? 6 : 0)) / 6f;
                else if (max == g)
                    h = ((b - r) / delta + 2) / 6f;
                else
                    h = ((r - g) / delta + 4) / 6f;
            }
        }

        private Color HslToRgb(float h, float s, float l, byte a)
        {            float r, g, b;
            
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                
                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }
            
            return Color.FromArgb(a, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private float HueToRgb(float p, float q, float t)
        {            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
            return p;
        }

        private string ColorToHex(Color color)
        {            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}