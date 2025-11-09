using System;
using System.Windows;
using System.Windows.Controls;
using Player.ViewModels;

namespace Player.Middle
{
    /// <summary>
    /// 中间区域控制，负责视频播放UI展示
    /// 
    /// MVVM模式说明：
    /// - View层只负责UI展示和用户交互
    /// - 所有业务逻辑已移至MiddleViewModel
    /// - 通过数据绑定和命令绑定实现与ViewModel的通信
    /// - UI样式通过数据绑定控制，符合MVVM原则
    /// </summary>
    public partial class MiddleControl : UserControl
    {
        public MiddleControl()
        {
            InitializeComponent();
            
            // 订阅Loaded和Unloaded事件以管理资源
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        
        /// <summary>
        /// 控件加载完成时的初始化逻辑
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 移除事件订阅避免内存泄漏
            Loaded -= OnLoaded;
            
            // 确保VideoView控件正确初始化并绑定MediaPlayer
            if (VideoPlayer != null && DataContext is MiddleViewModel viewModel)
            {
                // VideoView控件会在内部自动处理HWND绑定
                // 但需要确保MediaPlayer属性在控件加载后正确设置
                if (viewModel.MediaPlayer != null)
                {
                    System.Diagnostics.Debug.WriteLine("MiddleControl: VideoView已加载，MediaPlayer已可用");
                    
                    // 显式设置MediaPlayer属性，确保HWND绑定完成
                    // 这是防止VLC弹出独立窗口的关键步骤
                    VideoPlayer.MediaPlayer = viewModel.MediaPlayer;
                }
            }
            
            // 全屏控制栏通过BottomControlContent属性动态创建，不需要在这里设置
            System.Diagnostics.Debug.WriteLine("MiddleControl: 使用BottomControlContent动态创建控制栏");
        }

        /// <summary>
        /// 控件卸载时的清理逻辑
        /// </summary>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // 清理资源
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            // 移除事件订阅
            Unloaded -= OnUnloaded;
        }
    }
}