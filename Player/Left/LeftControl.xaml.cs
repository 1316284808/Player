
using System.Windows;
using System.Windows.Controls;

using System.Windows.Threading;
using Player.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;

namespace Player.Left
{
    /// <summary>
    /// LeftControl.xaml 的交互逻辑
    /// 负责显示和管理左侧面板的UI
    /// </summary>
    public partial class LeftControl : UserControl
    {
        // 获取ViewModel
        private LeftViewModel? _viewModel;
        
        internal LeftViewModel? ViewModel => _viewModel;
        
        private DispatcherTimer? _scrollTimer;
        private double _scrollOffset = 0;
        
        public LeftControl()
        {
            InitializeComponent();
            
            // 通过依赖注入容器获取ViewModel实例
            _viewModel = Services.DependencyInjectionService.GetViewModel<LeftViewModel>();
            DataContext = _viewModel;
            
            // 注册键盘快捷键和事件
            Loaded += LeftControl_Loaded;
            Unloaded += LeftControl_Unloaded;

         
        }
        
        private void LeftControl_Loaded(object sender, RoutedEventArgs e)
        {   
            // 移除事件订阅避免内存泄漏
            Loaded -= LeftControl_Loaded;
        }
        
        private void LeftControl_Unloaded(object sender, RoutedEventArgs e)
        {   
            // 停止滚动计时器
            _scrollTimer?.Stop();
            
            // 取消事件订阅
            Unloaded -= LeftControl_Unloaded;
            
            // 清理ViewModel资源
            if (_viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

    }
}