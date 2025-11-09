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
            
            // 初始化配置 - 无需预先加载所有配置，因为需要时会单独加载
            
            // 初始化系统通知
            SystemNotificationHelper.Initialize();
            
            // 初始化依赖注入容器（替代原有的ServiceLocator和ViewModelLocator）
            var serviceProvider = Services.DependencyInjectionService.Initialize();
            
            // 加载并应用保存的主题色
            var theme = LoadConfigManager.LoadTheme();
            var themeService = Services.DependencyInjectionService.GetService<Player.Services.IThemeService>();
            
            if (themeService != null && !string.IsNullOrEmpty(theme.PrimaryColor) && themeService.IsValidColor(theme.PrimaryColor))
            {
                themeService.ApplyTheme(theme.PrimaryColor);
            }
            else if (themeService != null)
            {
                // 应用默认紫色主题
                themeService.ApplyTheme("#7B68EE");
            }
            
            // 主窗口将通过App.xaml中的StartupUri自动创建和显示
        }
    }

}
