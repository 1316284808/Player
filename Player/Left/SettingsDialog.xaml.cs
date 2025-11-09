using System;
using System.Windows;
using IThemeService = Player.Services.IThemeService; // 使用别名解决命名冲突
using Player.Services; // 添加ServiceLocator的引用
using Player.ViewModels;

namespace Player.Left
{
    /// <summary>
    /// SettingsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private SettingsViewModel _viewModel;

        public SettingsDialog()
        {
            InitializeComponent();
            
            // 初始化ViewModel，通过依赖注入容器获取IThemeService实例
            var themeService = Services.DependencyInjectionService.GetService<IThemeService>();
            _viewModel = new SettingsViewModel(themeService);
            DataContext = _viewModel;
            
            // 订阅关闭请求事件
            _viewModel.CloseRequested += (s, e) => this.Close();
            
            // 设置对话框显示位置为固定在侧边栏旁边
            Loaded += SettingsDialog_Loaded;
            
            // 设置关闭按钮行为
            Closing += SettingsDialog_Closing;
        }
        
        private void SettingsDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 确保对话框正确关闭
            e.Cancel = false;
        }
        
        /// <summary>
        /// 对话框加载完成后，设置其位置为固定在侧边栏旁边
        /// </summary>
        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                // 检查是否处于全屏状态
                if (Owner.WindowState == WindowState.Maximized || 
                    (Owner.WindowState == WindowState.Normal && 
                     Owner.ActualWidth >= SystemParameters.PrimaryScreenWidth - 100 && 
                     Owner.ActualHeight >= SystemParameters.PrimaryScreenHeight - 100))
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
        /// 清理资源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // 取消事件订阅
            Loaded -= SettingsDialog_Loaded;
            Closing -= SettingsDialog_Closing;
            
            // 清理ViewModel资源
            if (_viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}