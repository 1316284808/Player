using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Player.Helpers
{
    /// <summary>
    /// 主题管理器 - 统一管理主题切换和资源引用
    /// </summary>
    public static class ThemeManager
    {
        private static readonly Dictionary<string, string> _themeFiles = new Dictionary<string, string>
        {
            ["#7B68EE"] = "Themes/PurpleTheme.xaml",
            ["#2196F3"] = "Themes/BlueTheme.xaml",
            ["#4CAF50"] = "Themes/GreenTheme.xaml",
            ["#FF9800"] = "Themes/OrangeTheme.xaml",
            ["#F44336"] = "Themes/RedTheme.xaml"
        };

        /// <summary>
        /// 应用主题色（支持自定义颜色）
        /// </summary>
        public static void ApplyTheme(string colorHex)
        {
            try
            {
                // 解析颜色
                Color primaryColor = (Color)ColorConverter.ConvertFromString(colorHex);
                
                ResourceDictionary newTheme;
                
                // 检查是否是预设主题
                if (_themeFiles.TryGetValue(colorHex, out string themeFile))
                {
                    // 使用预设主题文件
                    newTheme = new ResourceDictionary
                    {
                        Source = new Uri(themeFile, UriKind.Relative)
                    };
                }
                else
                {
                    // 动态生成主题资源
                    newTheme = CreateDynamicTheme(primaryColor);
                }

                // 创建自定义样式资源
                var customStyles = new ResourceDictionary
                {
                    Source = new Uri("Themes/CustomStyles.xaml", UriKind.Relative)
                };

                // 获取当前应用实例
                var app = Application.Current;
                if (app == null) return;

                // 查找并移除旧的主题和样式资源
                var oldResources = app.Resources.MergedDictionaries
                    .Where(d => d.Source != null && 
                               (d.Source.OriginalString.Contains("Themes/") || 
                                d.Source.OriginalString.Contains("CustomStyles.xaml")))
                    .ToList();

                // 先移除旧资源
                foreach (var oldResource in oldResources)
                {
                    app.Resources.MergedDictionaries.Remove(oldResource);
                }
                
                // 添加新主题
                app.Resources.MergedDictionaries.Add(newTheme);
                
                // 再添加自定义样式资源
                app.Resources.MergedDictionaries.Add(customStyles);

                // 强制刷新所有动态资源 - 使用异步方法
                _ = RefreshDynamicResourcesAsync(app); 
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"主题切换失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 动态创建主题资源字典
        /// </summary>
        private static ResourceDictionary CreateDynamicTheme(Color primaryColor)
        {
            var theme = new ResourceDictionary();
            
            // 计算颜色变体
            Color primaryLight = LightenColor(primaryColor, 0.2f);
            Color primaryDark = DarkenColor(primaryColor, 0.2f);
            Color secondary = ConvertToSecondary(primaryColor);
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
            
            // 背景色使用固定的灰白色，不随主题变化
            theme["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            theme["BackgroundMediumBrush"] = new SolidColorBrush(Color.FromRgb(250, 250, 250)); // #FAFAFA
            theme["BackgroundLightBrush"] = new SolidColorBrush(Color.FromRgb(248, 248, 248)); // #F8F8F8
            
            // ListBox 项目样式
            theme["ListBoxItemSelectedBackground"] = new SolidColorBrush(secondary);
            theme["ListBoxItemSelectedBorder"] = new SolidColorBrush(primaryColor);
            theme["ListBoxItemHoverBackground"] = new SolidColorBrush(secondaryLight);
            theme["ListBoxItemHoverBorder"] = new SolidColorBrush(accent);
            
            // 滚动条样式
            theme["ScrollBarThumbDefault"] = new SolidColorBrush(Color.FromRgb(176, 176, 176));
            theme["ScrollBarThumbHover"] = new SolidColorBrush(primaryColor);
            
            return theme;
        }
        
        /// <summary>
        /// 增亮颜色（公共方法）
        /// </summary>
        public static Color LightenColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + (255 - color.R) * factor),
                (byte)Math.Min(255, color.G + (255 - color.G) * factor),
                (byte)Math.Min(255, color.B + (255 - color.B) * factor)
            );
        }
        
        /// <summary>
        /// 加深颜色（公共方法）
        /// </summary>
        public static Color DarkenColor(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }
        
        /// <summary>
        /// 转换为次要颜色（淡色版本）（公共方法）
        /// </summary>
        public static Color ConvertToSecondary(Color primary)
        {
            // 转换为 HSL
            float h, s, l;
            RgbToHsl(primary, out h, out s, out l);
            
            // 降低饱和度，提高亮度
            s *= 0.3f;
            l = Math.Max(0.85f, l);
            
            return HslToRgb(h, s, l, primary.A);
        }
        
        /// <summary>
        /// 调整饱和度（公共方法）
        /// </summary>
        public static Color AdjustSaturation(Color color, float factor)
        {
            float h, s, l;
            RgbToHsl(color, out h, out s, out l);
            s = Math.Min(1.0f, s * factor);
            return HslToRgb(h, s, l, color.A);
        }
        
        /// <summary>
        /// RGB 转 HSL（公共方法）
        /// </summary>
        public static void RgbToHsl(Color rgb, out float h, out float s, out float l)
        {
            float r = rgb.R / 255f;
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
        
        /// <summary>
        /// HSL 转 RGB（公共方法）
        /// </summary>
        public static Color HslToRgb(float h, float s, float l, byte a)
        {
            float r, g, b;
            
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
        
        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
            return p;
        }

        /// <summary>
        /// 强制刷新动态资源引用（优化版）
        /// </summary>
        private static async Task RefreshDynamicResourcesAsync(Application app)
        {
            // 获取主窗口
            var mainWindow = app.MainWindow;
            if (mainWindow == null) return;

            try
            {
                // 使用异步方式刷新，避免UI阻塞
                await Task.Run(() =>
                {
                    // 在主线程上执行资源刷新
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        RefreshControlResources(mainWindow);
                    });
                });
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"刷新资源失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 递归刷新控件的动态资源 - 优化后的实现
        /// </summary>
        private static void RefreshControlResources(DependencyObject parent)
        {
            if (parent == null) return;

            // 使用队列代替递归，避免栈溢出风险
            var queue = new System.Collections.Generic.Queue<DependencyObject>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

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

        /// <summary>
        /// 获取所有可用的主题颜色
        /// </summary>
        public static Dictionary<string, string> GetAvailableThemes()
        {
            return new Dictionary<string, string>(_themeFiles);
        }

        /// <summary>
        /// 获取当前主题颜色
        /// </summary>
        public static string GetCurrentTheme()
        {
            var app = Application.Current;
            if (app == null) return "#7B68EE";

            var currentTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (currentTheme != null)
            {
                return _themeFiles.FirstOrDefault(x => x.Value == currentTheme.Source.OriginalString).Key ?? "#7B68EE";
            }

            return "#7B68EE";
        }
    }
}