using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.Helpers;
using Player.Middle;
using Player.Core.Enums;

namespace Player.Bottom
{
    /// <summary>
    /// BottomControl.xaml 的交互逻辑
    /// </summary>
    public partial class BottomControl : UserControl
    {
        // 静音状态
        private bool isMuted = false;
        private double volumeBeforeMute = 80;
        private bool isPlaying = false;
        
        // 计时器用于更新播放进度
        private DispatcherTimer _progressTimer;
        private bool _isUserDragging = false; // 标记用户是否正在拖动进度条
        private bool _wasPlayingBeforeDrag = false; // 记录拖动前的播放状态
        
        // 引用MiddleControl以调用全屏方法
        public MiddleControl MiddleControl { get; set; }

        public BottomControl()
        {
            InitializeComponent();
            // 订阅窗口的Loaded事件以确保DataContext已设置
            Loaded += BottomControl_Loaded;
            
            // 在构造函数中就明确设置播放状态为false
            isPlaying = false;
            
            // 初始化计时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 每100毫秒更新一次
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        /// <summary>
        /// 计时器回调：更新播放进度和时间显示
        /// </summary>
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            // 如果用户正在拖动进度条，不更新
            if (_isUserDragging) return;
            
            var window = Window.GetWindow(this);
            var middleControl = window != null ? VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window) : null;
            
            if (middleControl?.IsPlaying == true)
            {
                // 获取VLC播放器的当前时间和总时长
                var currentTime = middleControl.CurrentTime;
                var totalTime = middleControl.Length;
                
                if (totalTime > 0)
                {
                    // 更新进度条（转换为百分比）
                    double progress = (double)currentTime / totalTime * 100;
                    ProgressBar.Value = progress;
                    
                    // 更新时间显示
                    TimeDisplay.Text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
                }
            }
        }
        
        /// <summary>
        /// 格式化时间（毫秒转换为 mm:ss 或 h:mm:ss）
        /// </summary>
        private string FormatTime(long milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            if (timeSpan.Hours > 0)
                return timeSpan.ToString(@"h\:mm\:ss");
            else
                return timeSpan.ToString(@"m\:ss");
        }

        private void BottomControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化音量值
            if (VolumeSlider != null)
            {
                volumeBeforeMute = VolumeSlider.Value;
                UpdateVolumeIcon(VolumeSlider.Value);
            }
            
            // 明确重置播放状态并设置初始图标为播放图标▶
            isPlaying = false;
            SetPlayButtonToPlayIcon();
            
            // 确保视觉树完全加载后再次设置
            Dispatcher.BeginInvoke(new Action(() => {
                SetPlayButtonToPlayIcon();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
        
        /// <summary>
        /// 专门设置播放按钮为播放图标(▶)的方法
        /// </summary>
        private void SetPlayButtonToPlayIcon()
        {
            if (PlayPauseButton != null && PlayPauseButton.Content is CustomIcon icon)
            {
                icon.Kind = IconKind.Play; // 确保显示播放图标
               
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取中间控件的引用并调用播放/暂停方法
            var window = Window.GetWindow(this);
            var middleControl = window != null ? VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window) : null;
            
            if (middleControl != null)
            {
                // 检查是否有视频正在播放
                if (string.IsNullOrEmpty(middleControl.CurrentVideoPath))
                {
                    // 如果没有视频播放，提醒用户先选择播放内容
                    SystemNotificationHelper.ShowInfo("请先选择要播放的媒体文件");
                    return;
                }
                
                // 调用中间控件的播放/暂停方法
                middleControl.TogglePlayPause();
                
                // 基于中间控件的实际播放状态更新UI，而不是简单取反
                // 这里我们假设MiddleControl有一个IsPlaying属性可以获取
                // 由于无法直接访问，可以在TogglePlayPause后，根据当前状态推断新状态
                // 更好的做法是从MiddleControl获取实际状态，这里使用现有逻辑但添加注释说明
                isPlaying = !isPlaying;
                UpdatePlayIcon(isPlaying);
            }
        }

        public void UpdatePlayIcon(bool playing)
        {
            if (PlayPauseButton == null) return;
            
            var icon = PlayPauseButton.Content as CustomIcon;
            if (icon != null)
            {
                // 正在播放时显示暂停图标，暂停时显示播放图标
                icon.Kind = playing ? IconKind.Pause : IconKind.Play;
                
                // 更新内部播放状态
                isPlaying = playing;
              
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 设置音量
            var window = Window.GetWindow(this);
            var middleControl = window != null ? VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window) : null;
            
            middleControl?.SetVolume((int)e.NewValue);
            
            // 更新音量图标
            UpdateVolumeIcon(e.NewValue);
            
            // 如果音量从0变为非0，取消静音状态
            if (e.NewValue > 0 && isMuted)
            {
                isMuted = false;
            }
            
            // 保存非静音时的音量
            if (!isMuted && e.NewValue > 0)
            {
                volumeBeforeMute = e.NewValue;
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // ValueChanged 事件只用于监听，不在这里设置位置
            // 实际设置在 PreviewMouseUp 中处理
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用MiddleControl的全屏功能
                if (MiddleControl != null)
                {
                    MiddleControl.ToggleFullscreen();
                }
                else
                {
                    // 如果没有MiddleControl引用，尝试获取
                    FindMiddleControl();
                    if (MiddleControl != null)
                    {
                        MiddleControl.ToggleFullscreen();
                    }
                    else
                    {
                        SystemNotificationHelper.ShowError("无法找到MiddleControl实例");
                    }
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"全屏按钮点击错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试查找MiddleControl实例
        /// </summary>
        private void FindMiddleControl()
        {
            try
            {
                // 从视觉树中查找MiddleControl
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                while (parent != null && MiddleControl == null)
                {
                    if (parent is MainWindow)
                    {
                        MainWindow mainWindow = (parent as MainWindow)!;
                        // 尝试通过名称或属性查找MiddleControl
                        foreach (var child in LogicalTreeHelper.GetChildren(mainWindow))
                        {
                            if (child is MiddleControl)
                            {
                                MiddleControl = (child as MiddleControl)!;
                                break;
                            }
                            // 如果是Grid，查找其中的MiddleControl
                            else if (child is Panel)
                            {
                                foreach (var grandChild in LogicalTreeHelper.GetChildren(child as Panel))
                                {
                                    if (grandChild is MiddleControl)
                                    {
                                        MiddleControl = (grandChild as MiddleControl)!;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"查找MiddleControl失败: {ex.Message}");
            }
        }

        private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 设置播放速度
            var window = Window.GetWindow(this);
            if (window != null && SpeedComboBox != null)
            {
                var middleControl = VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window);
                if (middleControl != null)
                {
                    float rate = 1.0f;
                    switch (SpeedComboBox.SelectedIndex)
                    {
                        case 0: rate = 0.5f; break;
                        case 1: rate = 1.0f; break;
                        case 2: rate = 1.5f; break;
                        case 3: rate = 2.0f; break;
                    }
                    
                    middleControl.SetPlaybackRate(rate);
                }
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider == null || MuteButton == null) return;
            
            if (isMuted)
            {
                // 取消静音，恢复之前的音量
                VolumeSlider.Value = volumeBeforeMute;
                isMuted = false;
            }
            else
            {
                // 静音，保存当前音量
                volumeBeforeMute = VolumeSlider.Value;
                VolumeSlider.Value = 0;
                isMuted = true;
            }
            
            UpdateVolumeIcon(isMuted ? 0 : volumeBeforeMute);
        }

        private void UpdateVolumeIcon(double volume)
        {
            if (MuteButton == null) return;
            
            var icon = MuteButton.Content as CustomIcon;
            if (icon != null)
            {
                if (volume == 0 || isMuted)
                {
                    icon.Kind = IconKind.VolumeMute;
                }
                else if (volume < 30)
                {
                    icon.Kind = IconKind.VolumeLow;
                }
                else if (volume < 70)
                {
                    icon.Kind = IconKind.VolumeMedium;
                }
                else
                {
                    icon.Kind = IconKind.VolumeHigh;
                }
            }
        }
        
        /// <summary>
        /// 进度条鼠标按下：开始拖动
        /// </summary>
        private void ProgressBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isUserDragging = true;
            
            // 记录拖动前的播放状态
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var middleControl = VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window);
                if (middleControl != null)
                {
                    _wasPlayingBeforeDrag = middleControl.IsPlaying;
                }
            }
        }
        
        /// <summary>
        /// 进度条鼠标释放：结束拖动，设置新位置
        /// </summary>
        private void ProgressBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isUserDragging && ProgressBar != null)
            {
                // 计算并设置新位置
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    var middleControl = VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window);
                    if (middleControl != null)
                    {
                        long totalTime = middleControl.Length;
                        if (totalTime > 0)
                        {
                            long newPosition = (long)(totalTime * (ProgressBar.Value / 100));
                            
                            // 添加边界检查，确保位置有效
                            if (newPosition < 0) newPosition = 0;
                            if (newPosition > totalTime) newPosition = totalTime;
                            
                            middleControl.SetPosition(newPosition);
                            
                            // 如果拖动前是播放状态，则继续播放
                            if (_wasPlayingBeforeDrag && !middleControl.IsPlaying)
                            {
                                middleControl.TogglePlayPause();
                            }
                            
                            // 立即更新显示时间
                            UpdateTimeDisplay(newPosition, totalTime);
                        }
                    }
                }
            }
            
            _isUserDragging = false;
        }
        
        /// <summary>
        /// 更新时间显示
        /// </summary>
        private void UpdateTimeDisplay(long currentTime, long totalTime)
        {
            if (TimeDisplay != null)
            {
                TimeDisplay.Text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
            }
        }
    }
}
