using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Player.Helpers;
using Player.Core.Models;

namespace Player.Left
{
    /// <summary>
    /// SettingsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private string _selectedThemeColor = "#7B68EE";
        private bool _gpuAccelerationEnabled = false;
        private bool _superResolutionEnabled = false;
        private int _superResolutionLevel = 2;
        
        private bool _isUpdatingFromSliders = false;
        private bool _isUpdatingFromTextBox = false;

        public SettingsDialog()
        {
            InitializeComponent();
            LoadSettings();
            
            // 设置对话框显示位置为固定在侧边栏旁边
            Loaded += SettingsDialog_Loaded;
        }
        
        /// <summary>
        /// 对话框加载完成后，设置其位置为固定在侧边栏旁边
        /// </summary>
        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                // 检查是否处于全屏状态
                if (Owner.WindowState == WindowState.Maximized || Owner.WindowState == WindowState.Normal && Owner.ActualWidth == SystemParameters.PrimaryScreenWidth && Owner.ActualHeight == SystemParameters.PrimaryScreenHeight)
                {
                    // 全屏状态：使用屏幕坐标，侧边栏在屏幕左侧
                    double screenHeight = SystemParameters.PrimaryScreenHeight;
                    Left = 55; // 侧边栏宽度，对话框紧贴侧边栏右侧
                    Top = (screenHeight - ActualHeight) / 2; // 垂直居中
                }
                else
                {
                    // 非全屏状态：使用父窗口坐标
                    double ownerLeft = Owner.Left;
                    double ownerTop = Owner.Top;
                    double ownerHeight = Owner.ActualHeight;
                    
                    // 侧边栏宽度约为50px，让对话框紧贴侧边栏右侧显示
                    // 垂直居中对齐
                    Left = ownerLeft + 55; // 侧边栏宽度
                    Top = ownerTop + (ownerHeight - ActualHeight) / 2;
                }
                
                // 确保对话框不会超出屏幕范围
                EnsureDialogIsWithinScreenBounds();
            }
        }
        
        /// <summary>
        /// 确保对话框不会超出屏幕范围
        /// </summary>
        private void EnsureDialogIsWithinScreenBounds()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double borderThickness = 10; // 保留边距
            
            // 检查右侧边界
            if (Left + ActualWidth > screenWidth - borderThickness)
            {
                Left = screenWidth - ActualWidth - borderThickness;
            }
            
            // 检查左侧边界
            if (Left < borderThickness)
            {
                Left = borderThickness;
            }
            
            // 检查底部边界
            if (Top + ActualHeight > screenHeight - borderThickness)
            {
                Top = screenHeight - ActualHeight - borderThickness;
            }
            
            // 检查顶部边界
            if (Top < borderThickness)
            {
                Top = borderThickness;
            }
        }
        
        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 加载硬件设置配置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();
                _gpuAccelerationEnabled = hardwareSettings.EnableGPUAcceleration;
                _superResolutionEnabled = hardwareSettings.EnableSuperResolution;
                _superResolutionLevel = hardwareSettings.SuperResolutionLevel;

                // 加载主题配置
                var themeSettings = ConfigManager.LoadTheme();
                _selectedThemeColor = themeSettings.PrimaryColor;

                // 加载主题色
                UpdateCustomColorControls(_selectedThemeColor);
                
                // 高亮对应的预设按钮（如果匹配）
                HighlightPresetColorButton(_selectedThemeColor);
                
                // 加载 GPU 设置
                GpuAccelerationToggle.IsChecked = _gpuAccelerationEnabled;
                
                // 加载超分辨率设置
                SuperResolutionToggle.IsChecked = _superResolutionEnabled;
                
                // 加载VLC高级设置
                VideoRendererComboBox.SelectedValue = hardwareSettings.VideoRenderer ?? "direct3d11";
                FileCachingTextBox.Text = hardwareSettings.FileCaching.ToString();
                NetworkCachingTextBox.Text = hardwareSettings.NetworkCaching.ToString();
                
                // 性能优化选项
                UseSystemMemoryToggle.IsChecked = hardwareSettings.UseSystemMemory;
                EnableHardwareYUVToggle.IsChecked = hardwareSettings.EnableHardwareYUV;
                SkipLoopFilterToggle.IsChecked = hardwareSettings.SkipLoopFilter;
                EnableFastDecodingToggle.IsChecked = hardwareSettings.EnableFastDecoding;
                
                // 视频质量选项
                NoDropLateFramesToggle.IsChecked = hardwareSettings.NoDropLateFrames;
                NoSkipFramesToggle.IsChecked = hardwareSettings.NoSkipFrames;
                EnableDeinterlaceToggle.IsChecked = hardwareSettings.EnableDeinterlace;
                DeinterlaceModeComboBox.SelectedValue = hardwareSettings.DeinterlaceMode ?? "blend";
                
                // 超分辨率级别设置暂时未实现UI控件
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"加载设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 高亮预设颜色按钮
        /// </summary>
        private void HighlightPresetColorButton(string colorHex)
        {
            ResetColorButtonBorders();
            
            // 查找匹配的按钮
            var buttons = new[] { ColorPurple, ColorBlue, ColorGreen, ColorOrange, ColorRed };
            foreach (var button in buttons)
            {
                if (button.Tag is string tag && tag.Equals(colorHex, StringComparison.OrdinalIgnoreCase))
                {
                    button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    button.BorderThickness = new Thickness(3);
                    break;
                }
            }
        }

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 主题色选择
        /// </summary>
        private void ThemeColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colorHex)
            {
                _selectedThemeColor = colorHex;
                // 视觉反馈：给选中的按钮添加边框
                ResetColorButtonBorders();
                button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                button.BorderThickness = new Thickness(3);
                
                // 同步到自定义颜色区域
                UpdateCustomColorControls(colorHex);
            }
        }
        
        /// <summary>
        /// 自定义颜色文本框改变
        /// </summary>
        private void CustomColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromSliders) return;
            
            // 检查控件是否已初始化
            if (CustomColorPreview == null || CustomRedSlider == null || 
                CustomGreenSlider == null || CustomBlueSlider == null)
                return;

            string colorText = CustomColorTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(colorText) || !colorText.StartsWith("#")) return;

            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorText);
                CustomColorPreview.Fill = new SolidColorBrush(color);
                _selectedThemeColor = colorText.ToUpper();

                _isUpdatingFromTextBox = true;
                CustomRedSlider.Value = color.R;
                CustomGreenSlider.Value = color.G;
                CustomBlueSlider.Value = color.B;
                _isUpdatingFromTextBox = false;
                
                // 重置预设按钮边框
                ResetColorButtonBorders();
            }
            catch
            {
                // 无效颜色值，忽略
            }
        }
        
        /// <summary>
        /// RGB 滑块值改变
        /// </summary>
        private void CustomRgbSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdatingFromTextBox) return;
            
            // 检查控件是否已初始化
            if (CustomRedValue == null || CustomGreenValue == null || CustomBlueValue == null || 
                CustomColorTextBox == null || CustomColorPreview == null)
                return;

            _isUpdatingFromSliders = true;

            byte r = (byte)CustomRedSlider.Value;
            byte g = (byte)CustomGreenSlider.Value;
            byte b = (byte)CustomBlueSlider.Value;

            CustomRedValue.Text = r.ToString();
            CustomGreenValue.Text = g.ToString();
            CustomBlueValue.Text = b.ToString();

            string colorHex = $"#{r:X2}{g:X2}{b:X2}";
            CustomColorTextBox.Text = colorHex;
            CustomColorPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            _selectedThemeColor = colorHex;

            _isUpdatingFromSliders = false;
            
            // 重置预设按钮边框
            ResetColorButtonBorders();
        }
        
        /// <summary>
        /// 更新自定义颜色控件
        /// </summary>
        private void UpdateCustomColorControls(string colorHex)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorHex);
                
                _isUpdatingFromTextBox = true;
                CustomColorTextBox.Text = colorHex;
                CustomColorPreview.Fill = new SolidColorBrush(color);
                CustomRedSlider.Value = color.R;
                CustomGreenSlider.Value = color.G;
                CustomBlueSlider.Value = color.B;
                _isUpdatingFromTextBox = false;
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 重置所有颜色按钮边框
        /// </summary>
        private void ResetColorButtonBorders()
        {
            ColorPurple.BorderThickness = new Thickness(0);
            ColorBlue.BorderThickness = new Thickness(0);
            ColorGreen.BorderThickness = new Thickness(0);
            ColorOrange.BorderThickness = new Thickness(0);
            ColorRed.BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// GPU加速开关
        /// </summary>
        private void GpuAcceleration_Changed(object sender, RoutedEventArgs e)
        {
            _gpuAccelerationEnabled = GpuAccelerationToggle.IsChecked ?? false;
            // 如果关闭GPU加速，也需要关闭超分辨率
            if (!_gpuAccelerationEnabled && _superResolutionEnabled)
            {
                SuperResolutionToggle.IsChecked = false;
                _superResolutionEnabled = false;
            }
        }

        /// <summary>
        /// 视频超分辨率开关
        /// </summary>
        private void SuperResolution_Changed(object sender, RoutedEventArgs e)
        {
            _superResolutionEnabled = SuperResolutionToggle.IsChecked ?? false;
            // 超分辨率需要GPU支持
            if (_superResolutionEnabled && !_gpuAccelerationEnabled)
            {
                GpuAccelerationToggle.IsChecked = true;
                _gpuAccelerationEnabled = true;
            }
        }

        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 验证颜色值
                if (!_selectedThemeColor.StartsWith("#") || _selectedThemeColor.Length != 7)
                {
                    SystemNotificationHelper.ShowWarning("请输入有效的颜色值（格式：#RRGGBB）");
                    return;
                }
                
                // 保存硬件设置配置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();
                hardwareSettings.EnableGPUAcceleration = _gpuAccelerationEnabled;
                hardwareSettings.EnableSuperResolution = _superResolutionEnabled;
                hardwareSettings.SuperResolutionLevel = _superResolutionLevel;
                
                // 保存VLC高级设置
                hardwareSettings.VideoRenderer = VideoRendererComboBox.SelectedValue != null ? VideoRendererComboBox.SelectedValue.ToString() : null;
                
                // 验证并设置缓存值
                if (int.TryParse(FileCachingTextBox.Text, out int fileCaching) && fileCaching >= 0)
                    hardwareSettings.FileCaching = fileCaching;
                
                if (int.TryParse(NetworkCachingTextBox.Text, out int networkCaching) && networkCaching >= 0)
                    hardwareSettings.NetworkCaching = networkCaching;
                
                // 性能优化选项
                hardwareSettings.UseSystemMemory = UseSystemMemoryToggle.IsChecked ?? false;
                hardwareSettings.EnableHardwareYUV = EnableHardwareYUVToggle.IsChecked ?? false;
                hardwareSettings.SkipLoopFilter = SkipLoopFilterToggle.IsChecked ?? false;
                hardwareSettings.EnableFastDecoding = EnableFastDecodingToggle.IsChecked ?? false;
                
                // 视频质量选项
                hardwareSettings.NoDropLateFrames = NoDropLateFramesToggle.IsChecked ?? false;
                hardwareSettings.NoSkipFrames = NoSkipFramesToggle.IsChecked ?? false;
                hardwareSettings.EnableDeinterlace = EnableDeinterlaceToggle.IsChecked ?? false;
                hardwareSettings.DeinterlaceMode = DeinterlaceModeComboBox.SelectedValue != null ? DeinterlaceModeComboBox.SelectedValue.ToString() : null;
                
                ConfigManager.SaveHardwareSettings(hardwareSettings);

                // 保存主题配置
                var themeSettings = ConfigManager.LoadTheme();
                themeSettings.PrimaryColor = _selectedThemeColor;
                ConfigManager.SaveTheme(themeSettings); 
                // 重新保存更新后的主题设置
                ConfigManager.SaveTheme(themeSettings);

                // 刷新主窗口主题
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.RefreshTheme();
                }
                
                // 显示成功消息
                SystemNotificationHelper.ShowSuccess($"设置已保存并应用！\n\n主题色: {_selectedThemeColor}\nGPU加速: {(_gpuAccelerationEnabled ? "已启用" : "已禁用")}\n视频超分辨率: {(_superResolutionEnabled ? "已启用" : "已禁用")}");
                
                // 关闭对话框
                this.Close();
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"应用设置失败: {ex.Message}");
            }
        }
         
      
    }
}