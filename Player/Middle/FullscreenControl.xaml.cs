using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Player.Helpers;
using Player.Core.Enums;
using Player.Core.Models;
using LibVLCSharp.Shared;

namespace Player.Middle
{
    /// <summary>
    /// FullscreenControl.xaml 的交互逻辑
    /// </summary>
    public partial class FullscreenControl : UserControl
    {
        // 私有字段用于存储MediaPlayer引用
        private LibVLCSharp.Shared.MediaPlayer? _mediaPlayer;
           private PlaybackState _playbackState = new PlaybackState();
        // 公共属性，用于与父窗口通信
        public LibVLCSharp.Shared.MediaPlayer? MediaPlayer 
        { 
            get { return _mediaPlayer; }
            set 
            { 
                // 移除旧的MediaPlayer事件订阅
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Playing -= MediaPlayer_Playing;
                    _mediaPlayer.Paused -= MediaPlayer_Paused;
                    _mediaPlayer.Stopped -= MediaPlayer_Stopped;
                    _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                }
                
                // 设置新的MediaPlayer引用
                _mediaPlayer = value;
                
                // 订阅新的MediaPlayer事件
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Playing += MediaPlayer_Playing;
                    _mediaPlayer.Paused += MediaPlayer_Paused;
                    _mediaPlayer.Stopped += MediaPlayer_Stopped;
                    _mediaPlayer.EndReached += MediaPlayer_EndReached;
                    
                    // 根据MediaPlayer的实际状态更新UI
                    IsPlaying = _mediaPlayer.IsPlaying;
                }
            } 
        }
        
        // 播放状态对象
     
        
        public bool IsPlaying 
        { 
            get { return _playbackState.IsPlaying; } 
            set 
            { 
                if (_playbackState.IsPlaying != value)
                {
                    _playbackState.IsPlaying = value; 
                    // 直接使用UIControlManager更新图标
                    if (PlayPauseButton != null)
                    {
                        UIControlManager.UpdatePlayIcon(PlayPauseButton, value);
                    }
                }
            } 
        }
        
        // 计时器用于更新播放进度，允许为null
        private DispatcherTimer? _progressTimer;
        // 计时器用于控制栏自动隐藏
        private DispatcherTimer? _hideControlsTimer;
        private bool _isUserDragging = false; // 标记用户是否正在拖动进度条
        private bool _wasPlayingBeforeDrag = false; // 记录拖动前的播放状态
        private DateTime? _lastProgressUpdate; // 用于限制进度条更新频率
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
            _playbackState.Volume = 80;
            _playbackState.VolumeBeforeMute = 80;
            
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
                                    // 修复问题1：确保进度不会在18秒处停住，正确处理边界
                                    // 如果视频已结束（当前时间接近总时长），确保进度显示为100%
                                    if (currentTime >= totalTime - 100) // 距离结束100毫秒内视为结束
                                    {
                                        currentTime = totalTime;
                                    }
                                    
                                    // 更新进度条（转换为百分比）
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"进度更新错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"进度更新错误: {ex.Message}");
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
                _playbackState.VolumeBeforeMute = (int)VolumeSlider.Value;
                UpdateVolumeIcon(VolumeSlider.Value);
                
                // 设置媒体播放器的初始音量
                if (MediaPlayer != null)
                {
                    MediaPlayer.Volume = (int)VolumeSlider.Value;
                }
            }
            
            // 明确重置播放状态并设置初始图标为播放图标▶
            IsPlaying = false;
            UIControlManager.SetPlayButtonToPlayIcon(PlayPauseButton);
            
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
            
            // 清理MediaPlayer事件订阅
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Playing -= MediaPlayer_Playing;
                _mediaPlayer.Paused -= MediaPlayer_Paused;
                _mediaPlayer.Stopped -= MediaPlayer_Stopped;
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
            }
        }
        
        // MediaPlayer事件处理程序 - 播放开始时
        private void MediaPlayer_Playing(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsPlaying = true;
            });
        }
        
        // MediaPlayer事件处理程序 - 暂停时
        private void MediaPlayer_Paused(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsPlaying = false;
            });
        }
        
        // MediaPlayer事件处理程序 - 停止时
        private void MediaPlayer_Stopped(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsPlaying = false;
            });
        }
        
        // MediaPlayer事件处理程序 - 播放结束时
        private void MediaPlayer_EndReached(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IsPlaying = false;
            });
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
        


        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer != null)
            {
                try
                {
                    // 修复问题3：视频播放完后无法重新播放的问题
                    // 检查是否播放完毕（进度接近100%）
                    bool isAtEnd = false;
                    if (MediaPlayer.Length > 0)
                    {
                        double progress = (double)MediaPlayer.Time / MediaPlayer.Length;
                        isAtEnd = progress >= 0.99; // 进度超过99%视为播放完毕
                    }
                    
                    if (IsPlaying)
                    {
                        MediaPlayer.Pause();
                    }
                    else
                    {
                        // 如果已经播放完毕，重置到开始位置
                        if (isAtEnd)
                        {
                            MediaPlayer.Time = 0;
                        }
                        MediaPlayer.Play();
                    }
                }
                catch (Exception ex)
                {
                    SystemNotificationHelper.ShowError("播放控制出错: " + ex.Message);
                }
            }
        }



        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {            
            if (MediaPlayer != null && VolumeSlider != null)
            {                
                // 切换静音状态
                _playbackState.IsMuted = !_playbackState.IsMuted;
                
                if (_playbackState.IsMuted)
                {
                    // 静音时保存当前音量
                    _playbackState.VolumeBeforeMute = (int)VolumeSlider.Value;
                    MediaPlayer.Volume = 0;
                    VolumeSlider.Value = 0;
                }
                else
                {
                    // 取消静音时恢复之前的音量
                    MediaPlayer.Volume = _playbackState.VolumeBeforeMute;
                    VolumeSlider.Value = _playbackState.VolumeBeforeMute;
                }
                
                // 更新静音图标
                UpdateVolumeIcon(_playbackState.IsMuted ? 0 : _playbackState.VolumeBeforeMute);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {            
            if (MediaPlayer != null && VolumeSlider != null)
            {                
                double newVolume = e.NewValue;
                
                // 如果用户手动调整音量，取消静音状态
                if (_playbackState.IsMuted && newVolume > 0)
                {
                    _playbackState.IsMuted = false;
                }
                
                // 更新媒体播放器的音量
                MediaPlayer.Volume = (int)newVolume;
                _playbackState.Volume = (int)newVolume;
                
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
                    // 确保进度值在有效范围内
                    if (progress < 0) progress = 0;
                    if (progress > 1) progress = 1;
                    
                    long newPosition = (long)(progress * MediaPlayer.Length);
                    MediaPlayer.Time = newPosition;
                    
                    // 更新时间显示
                    if (TimeDisplay != null)
                    {
                        TimeDisplay.Text = $"{FormatTime(newPosition)} / {FormatTime(MediaPlayer.Length)}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"进度设置失败: {ex.Message}");
                }
            }
        }

        private void ProgressBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {            
            // 记录拖动前的播放状态
            _wasPlayingBeforeDrag = _playbackState.IsPlaying;
            // 拖动时暂停播放
            if (MediaPlayer != null && _playbackState.IsPlaying)
            {                
                MediaPlayer.Pause();
            }
            _isUserDragging = true;
        }

        private void ProgressBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isUserDragging = false;
            
            // 修复问题3：视频播放完后拖动进度条无法重新播放的问题
            if (MediaPlayer != null)
            {
                try
                {
                    // 只有在拖动前正在播放，或者视频已播放完毕时，才自动恢复播放
                    bool shouldResumePlayback = _wasPlayingBeforeDrag || 
                                               (MediaPlayer.Time < MediaPlayer.Length - 1000 && !_playbackState.IsPlaying);
                    
                    if (shouldResumePlayback && !MediaPlayer.IsPlaying)
                    {
                        MediaPlayer.Play();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"恢复播放失败: {ex.Message}");
                }
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