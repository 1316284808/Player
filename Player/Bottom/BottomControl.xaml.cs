using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.Helpers;
using Player.Middle;
using Player.Core.Enums;
using Player.Core.Models;

namespace Player.Bottom
{
    /// <summary>
    /// BottomControl.xaml 的交互逻辑
    /// </summary>
    public partial class BottomControl : UserControl
    {
        // 播放状态对象
        public PlaybackState PlaybackState { get; set; } = new PlaybackState();

        // 计时器用于更新播放进度
        private DispatcherTimer _progressTimer;
        private bool _isUserDragging = false; // 标记用户是否正在拖动进度条
        private bool _wasPlayingBeforeDrag = false; // 记录拖动前的播放状态
        private DateTime? _lastProgressUpdate; // 用于限制进度条更新频率
        
        // 引用MiddleControl以调用全屏方法
        public MiddleControl MiddleControl { get; set; }
        
        // 公共属性，用于外部访问播放/暂停按钮
        public Button PlayPauseButtonControl => PlayPauseButton;

        public BottomControl()
        {
            InitializeComponent();
            // 订阅窗口的Loaded事件以确保DataContext已设置
            Loaded += BottomControl_Loaded;

            // 在构造函数中就明确设置播放状态为false
            PlaybackState.IsPlaying = false;
            
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
            
            if (middleControl != null && ProgressBar != null && TimeDisplay != null)
            {                
                // 获取VLC播放器的当前时间和总时长
                var currentTime = middleControl.CurrentTime;
                var totalTime = middleControl.Length;
                
                if (totalTime > 0)
                {                    
                    // 修复问题1：确保进度不会在18秒处停住，正确处理边界
                    // 如果视频已结束（当前时间接近总时长），确保进度显示为100%
                    if (currentTime >= totalTime - 100) // 距离结束100毫秒内视为结束
                    {
                        currentTime = totalTime;
                    }
                    
                    // 计算进度百分比
                    double progress = (double)currentTime / totalTime * 100;
                    
                    // 确保进度在0-100范围内
                    progress = Math.Max(0, Math.Min(100, progress));
                    
                    // 修复问题2：统一限制更新频率，避免鼠标焦点时刷新过快
                    if (_lastProgressUpdate == null || DateTime.Now.Subtract(_lastProgressUpdate.Value).TotalMilliseconds > 200)
                    {                            
                        _lastProgressUpdate = DateTime.Now;
                        ProgressBar.Value = progress;
                    }
                    
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
                PlaybackState.VolumeBeforeMute = (int)VolumeSlider.Value;
                UpdateVolumeIcon(VolumeSlider.Value);
            }

            // 明确重置播放状态并设置初始图标为播放图标▶
            PlaybackState.IsPlaying  = false;
            UIControlManager.SetPlayButtonToPlayIcon(PlayPauseButton);
            
            // 确保视觉树完全加载后再次设置
            Dispatcher.BeginInvoke(new Action(() => {
                UIControlManager.SetPlayButtonToPlayIcon(PlayPauseButton);
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
        
        // SetPlayButtonToPlayIcon方法已移除，改为使用PlayIconManager.SetPlayButtonToPlayIcon()

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
                
                // 修复问题3：在调用播放/暂停前获取当前实际状态
                bool wasPlaying = middleControl.IsPlaying;
                
                // 调用中间控件的播放/暂停方法
                middleControl.TogglePlayPause();
                
                // 基于中间控件的实际播放状态更新UI
                // 延迟一小段时间确保状态已更新
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 从MiddleControl获取实际播放状态
                    bool actualPlayingState = middleControl.IsPlaying;
                    PlaybackState.IsPlaying = actualPlayingState;
                    // 使用UIControlManager更新图标
                    if (PlayPauseButton != null)
                    {
                        UIControlManager.UpdatePlayIcon(PlayPauseButton, actualPlayingState);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // 已移除UpdatePlayIcon方法，使用UIControlManager.UpdatePlayIcon()代替
        // 注意：在外部调用UIControlManager时，需要同时确保PlaybackState.IsPlaying状态被正确设置

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 设置音量
            var window = Window.GetWindow(this);
            var middleControl = window != null ? VisualTreeHelperExtensions.FindVisualChild<Player.Middle.MiddleControl>(window) : null;
            
            middleControl?.SetVolume((int)e.NewValue);
            
            // 更新音量图标
            UpdateVolumeIcon(e.NewValue);
            
            // 如果音量从0变为非0，取消静音状态
            if (e.NewValue > 0 && PlaybackState.IsMuted)
            {
                PlaybackState.IsMuted = false;
            }
            
            // 保存非静音时的音量
            if (!PlaybackState.IsMuted && e.NewValue > 0)
            {
                PlaybackState.VolumeBeforeMute = (int)e.NewValue;
            }
            
            // 更新PlaybackState中的音量
            PlaybackState.Volume = (int)e.NewValue;
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
            
            if (PlaybackState.IsMuted)
            {
                // 取消静音，恢复之前的音量
                VolumeSlider.Value = PlaybackState.VolumeBeforeMute;
                PlaybackState.IsMuted = false;
            }
            else
            {
                // 静音，保存当前音量
                PlaybackState.VolumeBeforeMute = (int)VolumeSlider.Value;
                VolumeSlider.Value = 0;
                PlaybackState.IsMuted = true;
            }
            
            UpdateVolumeIcon(PlaybackState.IsMuted ? 0 : PlaybackState.VolumeBeforeMute);
        }

        private void UpdateVolumeIcon(double volume)
        {
            if (MuteButton == null) return;
            
            var icon = MuteButton.Content as CustomIcon;
            if (icon != null)
            {
                if (volume == 0 || PlaybackState.IsMuted)
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
                            // 获取进度值并确保在有效范围内
                            double progress = ProgressBar.Value / 100;
                            if (progress < 0) progress = 0;
                            if (progress > 1) progress = 1;
                            
                            long newPosition = (long)(totalTime * progress);
                            
                            // 添加边界检查，确保位置有效
                            if (newPosition < 0) newPosition = 0;
                            if (newPosition > totalTime) newPosition = totalTime;
                            
                            try
                            {                                
                                // 设置新位置
                                middleControl.SetPosition(newPosition);
                                
                                // 修复问题3：视频播放完后拖动进度条无法重新播放的问题
                                // 只有在拖动前正在播放，或者视频已播放完毕时，才自动恢复播放
                                bool shouldResumePlayback = _wasPlayingBeforeDrag || 
                                                           (newPosition < totalTime - 1000 && !middleControl.IsPlaying);
                                
                                if (shouldResumePlayback && !middleControl.IsPlaying)
                                {                                    
                                    middleControl.TogglePlayPause();
                                }
                                
                                // 立即更新显示时间
                                UpdateTimeDisplay(newPosition, totalTime);
                                
                                // 更新UI播放状态
                                if (PlayPauseButton != null)
                                {
                                    UIControlManager.UpdatePlayIcon(PlayPauseButton, middleControl.IsPlaying);
                                }
                            }
                            catch (Exception ex)
                            {                                
                                System.Diagnostics.Debug.WriteLine($"进度设置失败: {ex.Message}");
                            }
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
