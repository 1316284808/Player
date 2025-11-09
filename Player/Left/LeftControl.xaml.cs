using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.ViewModels;
using Player.Core.Models;
using Player.Core.Services;
using Player.Services;
using Player.Core.Services;
using Player.Core.Events;

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

        /// <summary>
        /// 历史记录列表选择变更事件
        /// </summary>
        private async void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null || HistoryListBox.SelectedItem == null)
                return;
                
            var selectedDate = HistoryListBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(selectedDate))
            {
                // 调用ViewModel中的命令来显示历史记录
                //await _viewModel.ShowHistoryByDateAsync(selectedDate);
            }
        }
    }
}