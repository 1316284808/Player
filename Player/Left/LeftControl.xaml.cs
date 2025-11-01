﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.Helpers;
using Player.ViewModel;
using Player.Core.Models;
using Player.Core.Enums;

namespace Player.Left
{
    /// <summary>
    /// LeftControl.xaml 的交互逻辑
    /// </summary>
    public partial class LeftControl : UserControl
    {
        private bool _isCollapsed = false;
        private DispatcherTimer _scrollTimer;
        private double _scrollOffset = 0;
        
        public LeftControl()
        {
            // 为TextBlock添加焦点事件处理程序
            EventManager.RegisterClassHandler(typeof(TextBlock), TextBlock.GotFocusEvent, new RoutedEventHandler(TextBlock_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBlock), TextBlock.LostFocusEvent, new RoutedEventHandler(TextBlock_LostFocus));
            InitializeComponent();
            
            // 注册键盘快捷键
            Loaded += LeftControl_Loaded;
            Unloaded += LeftControl_Unloaded;
        }
        
        private void LeftControl_Loaded(object sender, RoutedEventArgs e)
        {   
            // 加载历史记录日期列表和播放列表
            InitializeFromHistory();
            
            // 注册快捷键
            RegisterHotKeys();
        }
        
        private void InitializeFromHistory()
        {   
            try
            {   
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "history.json");
                if (File.Exists(jsonPath))
                {   
                    string jsonContent = File.ReadAllText(jsonPath);
                    var historyData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);
                    
                    if (historyData != null && historyData.Any())
                    {   
                        // 按日期降序排序
                        var sortedDates = historyData.Keys.OrderByDescending(date => date).ToList();
                        
                        // 填充HistoryListBox
                        HistoryListBox.ItemsSource = sortedDates;
                        
                        // 默认选择第一个日期（最新日期）并加载对应的文件列表
                        string firstDate = sortedDates.First();
                        HistoryListBox.SelectedItem = firstDate;
                        LoadHistoryFiles(firstDate, historyData[firstDate]);
                    }
                }
            }
            catch (Exception ex)
            {   
                Console.WriteLine($"加载历史记录失败: {ex.Message}");
            }
        }
        
        // 注册 Ctrl+B 快捷键用于折叠/展开侧边栏
        private void RegisterHotKeys()
        {   
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.KeyDown += Window_KeyDown;
            }
        }
        
        private void LeftControl_Unloaded(object sender, RoutedEventArgs e)
        {   
            // 停止滚动计时器
            _scrollTimer?.Stop();
            // 注销键盘事件
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.KeyDown -= Window_KeyDown;
            }
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+B 切换侧边栏折叠状态
            if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                ToggleSidebar();
            }
        }

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {   
            if (sender is TextBlock textBlock && textBlock.Text.Length > 0)
            {   
                // 开始自动滚动
                _scrollOffset = 0;
                _scrollTimer = new DispatcherTimer
                {   
                    Interval = TimeSpan.FromMilliseconds(50)
                };
                _scrollTimer.Tick += (s, args) => ScrollTextBlock(textBlock);
                _scrollTimer.Start();
            }
        }

        private void TextBlock_LostFocus(object sender, RoutedEventArgs e)
        {   
            _scrollTimer?.Stop();
            _scrollTimer = null;
            
            if (sender is TextBlock textBlock)
            {   
                // 重置文本位置
                textBlock.RenderTransform = null;
            }
        }

        private void ScrollTextBlock(TextBlock textBlock)
        {   
            if (textBlock == null || _scrollTimer == null)
                return;
                
            // 测量文本长度
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                textBlock.FlowDirection,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                textBlock.Foreground,
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);
                
            double textWidth = formattedText.Width;
            double containerWidth = textBlock.ActualWidth - textBlock.Padding.Left - textBlock.Padding.Right;
            
            // 如果文本长度超过容器宽度，才进行滚动
            if (textWidth > containerWidth)
            {   
                // 计算滚动距离（来回滚动）
                double maxScroll = textWidth - containerWidth + 10; // 额外留一点边距
                double cycle = 4; // 4秒完成一个来回
                double cyclePosition = (_scrollOffset % (cycle * 1000)) / (cycle * 1000);
                
                double scrollAmount;
                if (cyclePosition < 0.5)
                {   
                    // 向前滚动
                    scrollAmount = cyclePosition * 2 * maxScroll;
                }
                else
                {   
                    // 向后滚动
                    scrollAmount = (1 - (cyclePosition - 0.5) * 2) * maxScroll;
                }
                
                // 应用滚动转换
                var transform = new TranslateTransform(-scrollAmount, 0);
                textBlock.RenderTransform = transform;
                
                _scrollOffset += 50; // 每次滚动增加的偏移量
            }
        }

        public void LoadHistoryDates()
        {   
            try
            {   
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "history.json");
                if (File.Exists(jsonPath))
                {   
                    string jsonContent = File.ReadAllText(jsonPath);
                    var historyData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);
                    
                    // 按日期降序排序
                    if (historyData != null)
                    {   
                        var sortedDates = historyData.Keys.OrderByDescending(date => date).ToList();
                        
                        // 添加到历史记录列表
                        HistoryListBox.ItemsSource = sortedDates;
                        
                        // 默认加载最近的历史记录
                        if (sortedDates.Count > 0 && historyData.ContainsKey(sortedDates[0]))
                        {   
                            LoadHistoryFiles(sortedDates[0], historyData[sortedDates[0]]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {   
                Console.WriteLine($"加载历史记录出错: {ex.Message}");
            }
        }

        private void LoadHistoryFiles(string date, List<string> filePaths)
        {   
            // 清空当前播放列表
            var playlistItems = new List<MediaItem>();
            
            // 处理历史文件，提取文件名
            foreach (string filePath in filePaths)
            {   
                var mediaItem = new MediaItem
                {   
                    Name = Path.GetFileName(filePath), // 只显示文件名
                    Path = filePath
                };
                playlistItems.Add(mediaItem);
            }
            
            // 直接设置PlaylistListBox的ItemsSource
            PlaylistListBox.ItemsSource = playlistItems;
            
            // 同时更新ViewModel的Playlist（如果需要）
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null && mainWindow.DataContext is PlayerViewModel viewModel)
            {   
                viewModel.Playlist.Clear();
                foreach (var item in playlistItems)
                {
                    viewModel.Playlist.Add(item);
                }
            }
        }

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            if (sender is ListBox listBox && listBox.SelectedItem is string selectedDate)
            {   
                try
                {   
                    string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "history.json");
                    if (File.Exists(jsonPath))
                    {   
                        string jsonContent = File.ReadAllText(jsonPath);
                        var historyData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);
                        
                        if (historyData != null && historyData.ContainsKey(selectedDate))
                        {   
                            LoadHistoryFiles(selectedDate, historyData[selectedDate]);
                        }
                    }
                }
                catch (Exception ex)
                {   
                    Console.WriteLine($"加载指定日期的历史记录失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 播放列表项被选中时的处理
        /// </summary>
        private void PlaylistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is MediaItem selectedItem)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // 播放新的媒体文件
                    mainWindow.PlayMediaFile(selectedItem.Path);
                    
                    // 使用Dispatcher.BeginInvoke延迟更新图标，确保播放状态已更新
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 更新播放/暂停按钮的icon为实际播放状态
                        UpdatePlayPauseButtonIcon();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
        
        /// <summary>
        /// 更新播放/暂停按钮的icon（根据实际播放状态）
        /// </summary>
        private void UpdatePlayPauseButtonIcon()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // 查找MiddleControl获取实际播放状态
                var middleControl = VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(mainWindow);
                
                // 查找BottomControl中的播放/暂停按钮
                var bottomControl = VisualTreeHelperExtensions.FindVisualChild<Bottom.BottomControl>(mainWindow);
                
                if (middleControl != null && bottomControl != null)
                {
                    bool isPlaying = middleControl.IsPlaying;
                    
                    // 直接使用UIControlManager更新图标
                    UIControlManager.UpdatePlayIcon(bottomControl.PlayPauseButtonControl, isPlaying);
                }
            }
        }

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog();
            settingsDialog.Owner = Window.GetWindow(this);
            settingsDialog.ShowDialog();
        }
        
        /// <summary>
        /// 折叠/展开侧边栏
        /// </summary>
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            
            _isCollapsed = !_isCollapsed;
            
            if (_isCollapsed)
            {
                // 折叠：隐藏整个LeftControl的内容区域，只显示侧边栏按钮
                var grid = this.Content as Grid;
                if (grid?.ColumnDefinitions.Count > 1)
                {
                    grid.ColumnDefinitions[1].Width = new GridLength(0);
                }
                
                // 更改图标为右箭头
                CollapseIcon.Kind = IconKind.ChevronRight;
                CollapseButton.ToolTip = "展开侧边栏";
            }
            else
            {
                // 展开：显示整个LeftControl
                var grid = this.Content as Grid;
                if (grid?.ColumnDefinitions.Count > 1)
                {
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                }
                
                // 更改图标为左箭头
                CollapseIcon.Kind = IconKind.ChevronLeft;
                CollapseButton.ToolTip = "折叠侧边栏";
            }
            
            // 触发侧边栏状态变化事件
            OnSidebarStateChanged(_isCollapsed);
        }
        

        
        /// <summary>
        /// 侧边栏状态变化事件
        /// </summary>
        public event EventHandler<bool> SidebarStateChanged;
        
        protected virtual void OnSidebarStateChanged(bool isCollapsed)
        {
            SidebarStateChanged?.Invoke(this, isCollapsed);
        }
        
        /// <summary>
        /// 播放列表标题双击事件
        /// </summary>
        private void PlaylistTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 双击播放列表标题切换侧边栏状态
            if (e.ClickCount == 2)
            {
                e.Handled = true;
                ToggleSidebar();
            }
        }
        
        /// <summary>
        /// 切换侧边栏状态（公开方法，可供外部调用）
        /// </summary>
        public void ToggleSidebar()
        {
            CollapseButton_Click(CollapseButton, new RoutedEventArgs());
        }
        
        /// <summary>
        /// 获取当前侧边栏状态
        /// </summary>
        public bool IsSidebarCollapsed
        {
            get { return _isCollapsed; }
        }
    }
}