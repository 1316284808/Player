using System;
using System.Windows;
using System.Windows.Controls;
 
using Player.ViewModels;

namespace Player.Bottom
{
    /// <summary>
    /// BottomControl.xaml 的交互逻辑
    /// 
    /// MVVM模式说明：
    /// - View层只负责UI展示和用户交互
    /// - 所有业务逻辑已移至BottomViewModel
    /// - 通过数据绑定和命令绑定实现与ViewModel的通信
    /// </summary>
    public partial class BottomControl : UserControl
    {
        // 公共属性，用于外部访问播放/暂停按钮
        public Button PlayPauseButtonControl => PlayPauseButton;

        public BottomControl()
        {
            InitializeComponent();
            
            // 订阅Loaded和Unloaded事件以管理资源
            Loaded += BottomControl_Loaded;
            Unloaded += BottomControl_Unloaded;
           
        }

        private void BottomControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 移除事件订阅避免内存泄漏
            Loaded -= BottomControl_Loaded;
            
            // 在加载时就绑定Opacity到MiddleViewModel的ControlBarOpacity属性
            if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel && 
                mainViewModel.MiddleViewModel != null && !_isBindingInitialized)
            {
                var binding = new System.Windows.Data.Binding("ControlBarOpacity")
                {
                    Source = mainViewModel.MiddleViewModel
                };
                PlayerControlsContainer.SetBinding(System.Windows.UIElement.OpacityProperty, binding);
                _isBindingInitialized = true;
            }
        }
        
        private void BottomControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // 清理资源
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            // 移除事件订阅
            Unloaded -= BottomControl_Unloaded;
        }
        
        private bool _isBindingInitialized = false;
        
        // 鼠标进入控制栏时显示控制栏
        private void PlayerControlsContainer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 尝试获取MiddleViewModel实例
            if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.MiddleViewModel?.ShowControlBar();
            }
        }
        
        // 鼠标离开控制栏时重置计时器
        private void PlayerControlsContainer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 尝试获取MiddleViewModel实例
            if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.MiddleViewModel?.ResetHideTimer();
            }
        }
    }
}