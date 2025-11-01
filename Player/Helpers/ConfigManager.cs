using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Player.Core.Models;

namespace Player.Helpers
{
    /// <summary>
    /// 应用程序配置管理器
    /// </summary>
    public class ConfigManager
    {
        private static readonly string JsonDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON");
        private static readonly string SettingsFilePath = Path.Combine(JsonDirectory, "settings.json");
        private static readonly string ThemeFilePath = Path.Combine(JsonDirectory, "theme.json");
        private static readonly string HardwareSettingsFilePath = Path.Combine(JsonDirectory, "hardware_settings.json");

        /// <summary>
        /// 加载所有配置（设置、主题和硬件设置）
        /// </summary>
        public static void LoadAllConfigs()
        {
            try
            {
                // 确保JSON目录存在
                if (!Directory.Exists(JsonDirectory))
                {
                    Directory.CreateDirectory(JsonDirectory);
                    // 创建默认配置文件
                    CreateDefaultFiles();
                }

                // 加载设置配置
                LoadSettings();
                
                // 加载主题配置
                LoadTheme();
                
                // 加载硬件设置配置
                LoadHardwareSettings();
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"加载配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载设置配置
        /// </summary>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return CreateDefaultSettingsFile();
                }

                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? CreateDefaultSettingsFile();
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"加载设置配置失败: {ex.Message}");
                return new AppSettings();
            }
        }

        /// <summary>
        /// 加载主题配置
        /// </summary>
        public static ThemeSettings LoadTheme()
        {
            try
            {
                if (!File.Exists(ThemeFilePath))
                {
                    return CreateDefaultThemeFile();
                }

                string json = File.ReadAllText(ThemeFilePath);
                var theme = JsonSerializer.Deserialize<ThemeSettings>(json);
                return theme ?? CreateDefaultThemeFile();
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"加载主题配置失败: {ex.Message}");
                return new ThemeSettings();
            }
        }

        /// <summary>
        /// 保存设置配置
        /// </summary>
        public static void SaveSettings(AppSettings settings)
        {
            SaveConfig(SettingsFilePath, settings);
        }

        /// <summary>
        /// 保存主题配置
        /// </summary>
        public static void SaveTheme(ThemeSettings theme)
        {
            SaveConfig(ThemeFilePath, theme);
        }

        /// <summary>
        /// 通用保存配置方法
        /// </summary>
        private static void SaveConfig<T>(string filePath, T config)
        {
            try
            {
                // 序列化为 JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // 格式化输出
                };
                string json = JsonSerializer.Serialize(config, options);

                // 写入文件
                File.WriteAllText(filePath, json);
                SystemNotificationHelper.ShowSuccess($"配置已保存到: {filePath}");
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建所有默认配置文件
        /// </summary>
        private static void CreateDefaultFiles()
        {
            CreateDefaultSettingsFile();
            CreateDefaultThemeFile();
            CreateDefaultHardwareSettingsFile();
        }
        
        /// <summary>
        /// 加载硬件设置配置
        /// </summary>
        public static HardwareSettings LoadHardwareSettings()
        {
            try
            {
                if (!File.Exists(HardwareSettingsFilePath))
                {
                    return CreateDefaultHardwareSettingsFile();
                }

                string json = File.ReadAllText(HardwareSettingsFilePath);
                var settings = JsonSerializer.Deserialize<HardwareSettings>(json);
                return settings ?? CreateDefaultHardwareSettingsFile();
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"加载硬件设置配置失败: {ex.Message}");
                return new HardwareSettings();
            }
        }
        
        /// <summary>
        /// 保存硬件设置配置
        /// </summary>
        public static void SaveHardwareSettings(HardwareSettings settings)
        {
            SaveConfig(HardwareSettingsFilePath, settings);
        }
        
        /// <summary>
        /// 创建默认硬件设置文件
        /// </summary>
        private static HardwareSettings CreateDefaultHardwareSettingsFile()
        {
            var defaultSettings = new HardwareSettings();
            SaveConfig(HardwareSettingsFilePath, defaultSettings);
            return defaultSettings;
        }

        /// <summary>
        /// 创建默认设置文件
        /// </summary>
        private static AppSettings CreateDefaultSettingsFile()
        {
            var defaultSettings = new AppSettings();
            SaveConfig(SettingsFilePath, defaultSettings);
            return defaultSettings;
        }

        /// <summary>
        /// 创建默认主题文件
        /// </summary>
        private static ThemeSettings CreateDefaultThemeFile()
        {
            // 创建默认主题设置，与PurpleTheme.xaml中的默认值保持一致
            var themeSettings = new ThemeSettings
            {
                PrimaryColor = "#7B68EE",
                SecondaryColor = "#E6E6FA",
                AccentColor = "#9370DB",
                BackgroundBrush = "#1E1E1E",
                SecondaryBrush = "#E6E6FA",
                SecondaryLightBrush = "#F5F5FF"
            };
            
            SaveConfig(ThemeFilePath, themeSettings);
            return themeSettings;
        }
    }
}
