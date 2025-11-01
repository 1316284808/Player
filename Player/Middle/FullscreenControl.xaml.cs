using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.Helpers;
using Player.Core.Enums;
using LibVLCSharp.Shared;

namespace Player.Middle
{
    /// <summary>
    /// FullscreenControl.xaml 的交互逻辑
    /// </summary>
    public partial class FullscreenControl : UserControl
    {
        // 公共属性，用于与父窗口通信
        public LibVLCSharp.Shared.MediaPlayer? MediaPlayer { get; set; }
        public bool IsPlaying 
        { 
            get { return isPlaying; }
            set 
            { 
                isPlaying = value; 
                UpdatePlayIcon(isPlaying);
            } 
        }
        
        // 状态变量
        private bool isMuted = false;
        private double volumeBeforeMute = 80;
        private bool isPlaying = false;
        
        // 计时器用于更新播放进度，允许为null
        private DispatcherTimer? _progressTimer;
        // 计时器用于控制栏自动隐藏
        private DispatcherTimer? _hideControlsTimer;
        private bool _isUserDragging = false; // 标记用户是否正在拖动进度条
        private bool _wasPlayingBeforeDrag = false; // 记录拖动前的播放状态
        private bool _controlsVisible = true; // 控制栏可见状态
        
        public event RoutedEventHandler? ExitFullscreen;
        
        public FullscreenControl()
        {
            InitializeComponent();
            // 订阅窗口的Loaded事件以确保DataContext已设置
            Loaded += FullscreenControl_Loaded;
            // 订阅Unloaded事件以清理资源
            Unloaded += FullscreenControl_Unloaded;
            
            // 在构造函数中就明确设置播放状态为false，使用公共属性
            IsPlaying = false;
            
            // 初始化播放进度计时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 每100毫秒更新一次
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            
            // 初始化控制栏自动隐藏计时器
            _hideControlsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 3秒后隐藏控制栏
            };
            _hideControlsTimer.Tick += HideControlsTimer_Tick;
        }
        
        // UpdatePlayIcon方法已存在，直接使用公共方法

        /// <summary>
        /// 计时器回调：更新播放进度和时间显示
        /// </summary>
        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            // 如果用户正在拖动进度条，不更新
            if (_isUserDragging) return;
            
            // 增加额外的空检查，确保UI元素仍然存在
            if (ProgressBar == null || TimeDisplay == null) return;
            
            try
            {
                // 首先检查MediaPlayer是否仍然有效
                if (MediaPlayer != null)
                {
                    try
                    {
                        // 检查Media是否仍然有效
                        if (MediaPlayer.Media != null)
                        {
                            // 安全地获取当前时间和总时长
                            var currentTime = MediaPlayer.Time;
                            var totalTime = MediaPlayer.Length;
                            
                            if (totalTime > 0)
                            {
                                // 确保UI元素仍然有效
                                if (ProgressBar != null && TimeDisplay != null)
                                {
                                    // 更新进度条（转换为百分比）
                                    double progress = (double)currentTime / totalTime * 100;
                                    ProgressBar.Value = progress;
                                    
                                    // 更新时间显示
                                    TimeDisplay.Text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
                                }
                            }
                        }
                    }
                    catch (AccessViolationException)
                    {
                        // 特别捕获访问冲突异常，这通常发生在MediaPlayer已被释放但引用仍然存在时
                        // 停止计时器以防止进一步的错误
                        if (_progressTimer != null)
                        {
                            _progressTimer.Stop();
                        }
                    }
                    catch (Exception)
                    {
                        // 捕获其他可能的异常
                    }
                }
            }
            catch (Exception)
            {
                // 最外层异常捕获，确保计时器不会因为任何未预期的错误而崩溃
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

        private void FullscreenControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化音量值
            if (VolumeSlider != null)
            {
                volumeBeforeMute = VolumeSlider.Value;
                UpdateVolumeIcon(VolumeSlider.Value);
                
                // 设置媒体播放器的初始音量
                if (MediaPlayer != null)
                {
                    MediaPlayer.Volume = (int)VolumeSlider.Value;
                }
            }
            
            // 明确重置播放状态并设置初始图标为播放图标▶
            isPlaying = false;
            SetPlayButtonToPlayIcon();
            
            // 确保视觉树完全加载后再次设置
            Dispatcher.BeginInvoke(new Action(() => {
                SetPlayButtonToPlayIcon();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            
            // 启动计时器
            if (_progressTimer != null)
            {
                _progressTimer.Start();
            }
            
            // 订阅鼠标移动事件以显示控制栏
            MouseMove += OnMouseMove;
            
            // 为控制栏容器添加鼠标进入/离开事件
            var rootElement = (Border)this.Content;
            if (rootElement != null)
            {
                rootElement.MouseEnter += OnControlsMouseEnter;
                rootElement.MouseLeave += OnControlsMouseLeave;
            }
            
            // 启动控制栏自动隐藏计时器
            StartHideControlsTimer();
        }
        
        private void FullscreenControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // 停止并清理计时器
            if (_progressTimer != null)
            {
                _progressTimer.Stop();
                _progressTimer.Tick -= ProgressTimer_Tick;
                _progressTimer = null;
            }
            
            // 停止并清理控制栏自动隐藏计时器
            if (_hideControlsTimer != null)
            {
                _hideControlsTimer.Stop();
                _hideControlsTimer.Tick -= HideControlsTimer_Tick;
                _hideControlsTimer = null;
            }
            
            // 清理事件订阅
            Loaded -= FullscreenControl_Loaded;
            Unloaded -= FullscreenControl_Unloaded;
            MouseMove -= OnMouseMove;
            
            var rootElement = (Border)this.Content;
            if (rootElement != null)
            {
                rootElement.MouseEnter -= OnControlsMouseEnter;
                rootElement.MouseLeave -= OnControlsMouseLeave;
            }
            
            // 断开MediaPlayer引用 - 注意：这里不能赋值为null，因为属性可能不允许null值
                // MediaPlayer = null;  // 注释掉这行，避免null赋值警告
        }
        
        /// <summary>
        /// 控制栏自动隐藏计时器触发事件
        /// </summary>
        private void HideControlsTimer_Tick(object? sender, EventArgs e)
        {
            HideControls();
        }
        
        /// <summary>
        /// 鼠标移动事件处理 - 显示控制栏并重置计时器
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ShowControls();
            StartHideControlsTimer();
        }
        
        /// <summary>
        /// 鼠标进入控制栏事件处理
        /// </summary>
        private void OnControlsMouseEnter(object sender, MouseEventArgs e)
        {
            ShowControls();
            // 鼠标在控制栏上时暂停计时器
            StopHideControlsTimer();
        }
        
        /// <summary>
        /// 鼠标离开控制栏事件处理
        /// </summary>
        private void OnControlsMouseLeave(object sender, MouseEventArgs e)
        {
            // 鼠标离开控制栏时重新启动计时器
            StartHideControlsTimer();
        }
        
        /// <summary>
        /// 显示控制栏 - 移除透明属性
        /// </summary>
        private void ShowControls()
        {
            if (!_controlsVisible)
            {
                var rootElement = (Border)this.Content;
                if (rootElement != null)
                {
                    // 移除透明效果
                    rootElement.Opacity = 1.0;
                    _controlsVisible = true;
                }
            }
        }
        
        /// <summary>
        /// 隐藏控制栏 - 设置半透明效果而不是完全隐藏
        /// </summary>
        private void HideControls()
        {
            if (_controlsVisible)
            {
                var rootElement = (Border)this.Content;
                if (rootElement != null)
                {
                    //尝试过Visibility.Hidden 但是没用所以这只能设置很低的透明度，让用户能隐约看到控制栏(几乎不可见)，但不影响视频观看
                    rootElement.Opacity = 0.01;
                    _controlsVisible = false;
                }
            }
        }
        
        /// <summary>
        /// 启动控制栏自动隐藏计时器
        /// </summary>
        private void StartHideControlsTimer()
        {
            if (_hideControlsTimer != null)
            {
                _hideControlsTimer.Stop();
                _hideControlsTimer.Start();
            }
        }
        
        /// <summary>
        /// 停止控制栏自动隐藏计时器
        /// </summary>
        private void StopHideControlsTimer()
        {
            if (_hideControlsTimer != null)
            {
                _hideControlsTimer.Stop();
            }
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
            if (MediaPlayer != null)
            {
                try
                {
                    // 基于实际播放状态进行操作，然后更新UI状态
                    if (IsPlaying)
                    {
                        MediaPlayer.Pause();
                        // 根据实际操作更新状态
                        IsPlaying = false;
                    }
                    else
                    {
                        MediaPlayer.Play();
                        // 根据实际操作更新状态
                        IsPlaying = true;
                    }
                }
                catch (Exception ex)
                {
                    SystemNotificationHelper.ShowError("播放控制出错: " + ex.Message);
                }
            }
        }

        public void UpdatePlayIcon(bool playing)
        {
            if (PlayPauseButton == null) return;
            
            var icon = PlayPauseButton.Content as CustomIcon;
            if (icon != null)
            {
                icon.Kind = playing ? IconKind.Pause : IconKind.Play;
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer != null && VolumeSlider != null)
            {
                // 切换静音状态
                isMuted = !isMuted;
                
                if (isMuted)
                {
                    // 静音时保存当前音量
                    volumeBeforeMute = VolumeSlider.Value;
                    MediaPlayer.Volume = 0;
                    VolumeSlider.Value = 0;
                }
                else
                {
                    // 取消静音时恢复之前的音量
                    MediaPlayer.Volume = (int)volumeBeforeMute;
                    VolumeSlider.Value = volumeBeforeMute;
                }
                
                // 更新静音图标
                UpdateVolumeIcon(isMuted ? 0 : volumeBeforeMute);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaPlayer != null && VolumeSlider != null)
            {
                double newVolume = e.NewValue;
                
                // 如果用户手动调整音量，取消静音状态
                if (isMuted && newVolume > 0)
                {
                    isMuted = false;
                }
                
                // 更新媒体播放器的音量
                MediaPlayer.Volume = (int)newVolume;
                
                // 更新音量图标
                UpdateVolumeIcon(newVolume);
            }
        }

        private void UpdateVolumeIcon(double volume)
        {
            if (MuteButton == null || MuteButton.Content is not CustomIcon icon)
                return;
            
            if (volume <= 0)
            {
                icon.Kind = IconKind.VolumeMute;
            }
            else if (volume < 33)
            {
                icon.Kind = IconKind.VolumeLow;
            }
            else if (volume < 66)
            {
                icon.Kind = IconKind.VolumeMedium;
            }
            else
            {
                icon.Kind = IconKind.VolumeHigh;
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 只有在用户拖动时才手动设置进度
            if (_isUserDragging && MediaPlayer != null && MediaPlayer.Media != null)
            {
                try
                {
                    double progress = e.NewValue / 100;
                    long newPosition = (long)(progress * MediaPlayer.Length);
                    MediaPlayer.Time = newPosition;
                    
                    // 更新时间显示
                    TimeDisplay.Text = $"{FormatTime(newPosition)} / {FormatTime(MediaPlayer.Length)}";
                }
                catch (Exception)
            {
                // 捕获可能的异常 - 移除未使用的变量
            }
            }
        }

        private void ProgressBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 记录拖动前的播放状态
            _wasPlayingBeforeDrag = isPlaying;
            // 拖动时暂停播放
            if (MediaPlayer != null && isPlaying)
            {
                MediaPlayer.Pause();
            }
            _isUserDragging = true;
        }

        private void ProgressBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isUserDragging = false;
            // 如果拖动前正在播放，则恢复播放
            if (MediaPlayer != null && _wasPlayingBeforeDrag)
            {
                MediaPlayer.Play();
            }
        }

        private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MediaPlayer != null && SpeedComboBox != null && SpeedComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string speedText = selectedItem.Content.ToString() ?? "1.0x";
                if (double.TryParse(speedText.Replace("x", ""), out double speed))
                {
                    try
                    {
                        MediaPlayer.SetRate((float)speed);
                    }
                    catch (Exception ex)
                    {
                        SystemNotificationHelper.ShowError("设置播放速度出错: " + ex.Message);
                    }
                }
            }
        }

        // 退出全屏按钮点击事件
        private void ExitFullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            // 触发退出全屏事件，让父窗口处理
            ExitFullscreen?.Invoke(this, e);
        }
    }
}