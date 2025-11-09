using System;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Runtime;
using System.Text.Json;
using Player.Core.Models;

namespace Player.Core.Services
{
    /// <summary>
    /// 配置管理器 - 负责应用程序设置的加载和保存
    /// </summary>
    public static class ConfigManager
    {
       
        
        static ConfigManager()
        {
            // 确保JSON目录存在
            if (!Directory.Exists(SettingPath.JsonDirectory))
            {
                Directory.CreateDirectory(SettingPath.JsonDirectory);
            }
        }
        
        /// <summary>
        /// 加载应用程序设置
        /// </summary>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingPath.SettingsPath))
                {
                    var json = File.ReadAllText(SettingPath.SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // 忽略错误，返回默认设置
            }
            
            return new AppSettings();
        }
        
        /// <summary>
        /// 保存应用程序设置
        /// </summary>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingPath.SettingsPath, json);
            }
            catch (Exception)
            {
                // 忽略保存错误
            }
        }
        
        /// <summary>
        /// 加载主题设置
        /// </summary>
        public static ThemeSettings LoadTheme()
        {
            try
            {
                if (File.Exists(SettingPath.ThemePath))
                {
                    var json = File.ReadAllText(SettingPath.ThemePath);
                    return JsonSerializer.Deserialize<ThemeSettings>(json) ?? new ThemeSettings();
                }
            }
            catch (Exception)
            {
                // 忽略错误，返回默认设置
            }
            
            return new ThemeSettings();
        }
        
        /// <summary>
        /// 保存主题设置
        /// </summary>
        public static void SaveTheme(ThemeSettings theme)
        {
            try
            {
                var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingPath.ThemePath, json);
            }
            catch (Exception)
            {
                // 忽略保存错误
            }
        }
        
        /// <summary>
        /// 加载硬件设置
        /// </summary>
        public static HardwareSettings LoadHardwareSettings()
        {
            try
            {
                if (File.Exists(SettingPath.HardwarePath))
                {
                    var json = File.ReadAllText(SettingPath.HardwarePath);
                    return JsonSerializer.Deserialize<HardwareSettings>(json) ?? new HardwareSettings();
                }
            }
            catch (Exception)
            {
                // 忽略错误，返回默认设置
            }
            
            return new HardwareSettings();
        }
        
        /// <summary>
        /// 保存硬件设置
        /// </summary>
        public static void SaveHardwareSettings(HardwareSettings hardware)
        {
            try
            {
                var json = JsonSerializer.Serialize(hardware, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingPath.HardwarePath, json);
            }
            catch (Exception)
            {
                // 忽略保存错误
            }
        }
    }
    
    /// <summary>
    /// 主题设置
    /// </summary>
    public class ThemeSettings
    {
        public string PrimaryColor { get; set; } = "#7B68EE";
        public string SecondaryColor { get; set; } = "#F5F5F5";
        public string AccentColor { get; set; } = "#FF4081";
        public bool IsDarkMode { get; set; } = false;
    }
    
    /// <summary>
    /// 硬件设置
    /// </summary>
    public class HardwareSettings
    {
        public bool EnableGPUAcceleration { get; set; } = false;
        public bool EnableSuperResolution { get; set; } = false;
        public int SuperResolutionLevel { get; set; } = 2;
        public string? HardwareDecoder { get; set; } = "d3d11va";
        public string? VideoRenderer { get; set; }="";  
        public int FileCaching { get; set; } = 300;
        public int NetworkCaching { get; set; } = 1000;
        public bool UseSystemMemory { get; set; } = false;
        public bool AutoThreads { get; set; } = false;
        public bool EnableHurryUpDecoding { get; set; } = false;
        public bool EnableHardwareYUV { get; set; } = true;
        public bool SkipLoopFilter { get; set; } = false;
        public bool EnableFastDecoding { get; set; } = true;
        public bool NoDropLateFrames { get; set; } = false;
        public bool NoSkipFrames { get; set; } = false;
        public bool EnableDeinterlace { get; set; } = false;
        public string? DeinterlaceMode { get; set; } = "blend";
    }
}