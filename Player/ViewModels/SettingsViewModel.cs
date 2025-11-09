using System;
using System.Collections.Generic;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Player.Core.Models;
using Player.Core.Services; // 使用Player.Core.Services中的ConfigManager
using Player.Core.ViewModels;
using Player.Services; // 添加WpfNotificationService的引用

namespace Player.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly Player.Services.IThemeService _themeService;
        private WpfNotificationService Notification=new WpfNotificationService();

        private string _selectedThemeColor = "#7B68EE";
        public string SelectedThemeColor
        {
            get => _selectedThemeColor;
            set
            {
                if (SetProperty(ref _selectedThemeColor, value))
                {
                    UpdateRgbValuesFromColor();
                }
            }
        }

        private byte _rValue = 123;
        public byte RValue
        {
            get => _rValue;
            set
            {
                if (SetProperty(ref _rValue, value))
                {
                    UpdateColorFromRgbValues();
                }
            }
        }

        private byte _gValue = 104;
        public byte GValue
        {
            get => _gValue;
            set
            {
                if (SetProperty(ref _gValue, value))
                {
                    UpdateColorFromRgbValues();
                }
            }
        }

        private byte _bValue = 238;
        public byte BValue
        {
            get => _bValue;
            set
            {
                if (SetProperty(ref _bValue, value))
                {
                    UpdateColorFromRgbValues();
                }
            }
        }
        
        [ObservableProperty]
        private bool _enableGpuAcceleration = false;
        
        [ObservableProperty]
        private string _hardwareDecoder = "d3d11va";
        
        [ObservableProperty]
        private bool _enableSuperResolution = false;
        
        [ObservableProperty]
        private int _superResolutionLevel = 2;
        
        [ObservableProperty]
        private string _videoRenderer = "direct3d11";
        
        [ObservableProperty]
        private int _fileCaching = 1000;
        
        [ObservableProperty]
        private int _networkCaching = 2000;
        
        [ObservableProperty]
        private bool _useSystemMemory = true;
        
        [ObservableProperty]
        private bool _enableHardwareYUV = true;
        
        [ObservableProperty]
        private bool _skipLoopFilter = true;
        
        [ObservableProperty]
        private bool _enableFastDecoding = true;
        
        [ObservableProperty]
        private bool _noDropLateFrames = true;
        
        [ObservableProperty]
        private bool _noSkipFrames = true;
        
        [ObservableProperty]
        private bool _enableDeinterlace = true;
        
        [ObservableProperty]
        private string _deinterlaceMode = "blend";
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isStatusVisible = false;
        
        public SettingsViewModel(Player.Services.IThemeService themeService)
        {
            _themeService = themeService;
            LoadSettings();
        }
        
        private void LoadSettings()
        {
            try
            {
                // 加载硬件设置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();
                EnableGpuAcceleration = hardwareSettings.EnableGPUAcceleration;
                HardwareDecoder = hardwareSettings.HardwareDecoder ?? "d3d11va";
                EnableSuperResolution = hardwareSettings.EnableSuperResolution;
                SuperResolutionLevel = hardwareSettings.SuperResolutionLevel;
                VideoRenderer = hardwareSettings.VideoRenderer ?? "direct3d11";
                FileCaching = hardwareSettings.FileCaching;
                NetworkCaching = hardwareSettings.NetworkCaching;
                UseSystemMemory = hardwareSettings.UseSystemMemory;
                EnableHardwareYUV = hardwareSettings.EnableHardwareYUV;
                SkipLoopFilter = hardwareSettings.SkipLoopFilter;
                EnableFastDecoding = hardwareSettings.EnableFastDecoding;
                NoDropLateFrames = hardwareSettings.NoDropLateFrames;
                NoSkipFrames = hardwareSettings.NoSkipFrames;
                EnableDeinterlace = hardwareSettings.EnableDeinterlace;
                DeinterlaceMode = hardwareSettings.DeinterlaceMode ?? "blend";
                
                // 加载主题设置
                var themeSettings = ConfigManager.LoadTheme();
                SelectedThemeColor = themeSettings.PrimaryColor;
            }
            catch (Exception ex)
            {
                Notification.ShowInfo($"加载设置失败: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private void ApplySettings()
        {
            try
            {
                // 验证颜色值
                if (!_themeService.IsValidColor(SelectedThemeColor))
                {
                    Notification.ShowInfo("请输入有效的颜色值（格式：#RRGGBB）");
                    return;
                }
                
                // 保存硬件设置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();
                hardwareSettings.EnableGPUAcceleration = EnableGpuAcceleration;
                hardwareSettings.HardwareDecoder = HardwareDecoder;
                hardwareSettings.EnableSuperResolution = EnableSuperResolution;
                hardwareSettings.SuperResolutionLevel = SuperResolutionLevel;
                hardwareSettings.VideoRenderer = VideoRenderer;
                hardwareSettings.FileCaching = FileCaching;
                hardwareSettings.NetworkCaching = NetworkCaching;
                hardwareSettings.UseSystemMemory = UseSystemMemory;
                hardwareSettings.EnableHardwareYUV = EnableHardwareYUV;
                hardwareSettings.SkipLoopFilter = SkipLoopFilter;
                hardwareSettings.EnableFastDecoding = EnableFastDecoding;
                hardwareSettings.NoDropLateFrames = NoDropLateFrames;
                hardwareSettings.NoSkipFrames = NoSkipFrames;
                hardwareSettings.EnableDeinterlace = EnableDeinterlace;
                hardwareSettings.DeinterlaceMode = DeinterlaceMode;
                
                ConfigManager.SaveHardwareSettings(hardwareSettings);
                
                // 保存主题设置
                var themeSettings = ConfigManager.LoadTheme();
                themeSettings.PrimaryColor = SelectedThemeColor;
                ConfigManager.SaveTheme(themeSettings);
                
                // 应用主题
                _themeService.ApplyTheme(SelectedThemeColor);

                Notification.ShowInfo($"设置已保存并应用！\n主题色: {SelectedThemeColor}\nGPU加速: {(EnableGpuAcceleration ? "已启用" : "已禁用")}\n视频超分辨率: {(EnableSuperResolution ? "已启用" : "已禁用")}");
                
                // 通知主窗口关闭设置对话框
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Notification.ShowInfo($"应用设置失败: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        
        [RelayCommand]
        private void SelectThemeColor(string colorHex)
        {
            // 直接设置SelectedThemeColor，这会自动触发属性变更和RGB值更新
            SelectedThemeColor = colorHex;
        }

        /// <summary>
        /// 从颜色字符串更新RGB值
        /// </summary>
        private void UpdateRgbValuesFromColor()
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedThemeColor))
                {
                    // 尝试将颜色字符串转换为Color对象
                    var color = System.Windows.Media.ColorConverter.ConvertFromString(SelectedThemeColor) as System.Windows.Media.Color?;
                    if (color.HasValue)
                    {
                        // 使用SetProperty来避免循环更新
                        SetProperty(ref _rValue, color.Value.R);
                        SetProperty(ref _gValue, color.Value.G);
                        SetProperty(ref _bValue, color.Value.B);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating RGB values: {ex.Message}");
            }
        }

        /// <summary>
        /// 从RGB值更新颜色字符串
        /// </summary>
        private void UpdateColorFromRgbValues()
        {
            try
            {
                // 创建新的颜色
                var color = System.Windows.Media.Color.FromRgb(RValue, GValue, BValue);
                // 转换为十六进制字符串
                var colorString = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                
                // 暂停通知，防止循环更新
                OnPropertyChanging(nameof(SelectedThemeColor));
                _selectedThemeColor = colorString;
                OnPropertyChanged(nameof(SelectedThemeColor));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating color string: {ex.Message}");
            }
        }
        
        
        
        public event EventHandler CloseRequested;
    }
}