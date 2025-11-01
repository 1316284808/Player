using System.Configuration;
using System.Data;
using System.Windows;
using Player.Helpers;

namespace Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 加载所有配置
            ConfigManager.LoadAllConfigs();
            
            // 初始化系统通知
            SystemNotificationHelper.Initialize();
            
            // 加载并应用保存的主题色
            var theme = ConfigManager.LoadTheme();
            if (!string.IsNullOrEmpty(theme.PrimaryColor))
            {
                ThemeManager.ApplyTheme(theme.PrimaryColor);
            }
            else
            {
                // 应用默认紫色主题
                ThemeManager.ApplyTheme("#7B68EE");
            }
            
            // 主窗口将通过App.xaml中的StartupUri自动创建和显示
        }
    }

}
