using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Player.Core.Events;
using Player.Core.Services;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Player.ViewModels
{
    /// <summary>
    /// 中间区域视图模型 (MiddleViewModel)
    /// 
    /// MVVM模式说明：
    /// - 负责视频播放的核心逻辑
    /// - 与VLC播放器服务进行交互
    /// - 管理共享的PlaybackState状态
    /// 
    /// 主要职责：
    /// 1. 管理VLC播放器的初始化和销毁
    /// 2. 处理播放控制命令（播放/暂停/停止）
    /// 3. 更新共享的PlaybackState状态
    /// 4. 通过事件总线与其他ViewModel通信
    /// </summary>
    public partial class MiddleViewModel : ObservableObject, IDisposable
    {
        // 依赖注入的服务接口
        private readonly IVlcPlayerService _vlcPlayerService;    // VLC播放器服务
        private readonly IMessengerService _messengerService;    // 消息通信服务
        private readonly Player.Core.Models.PlaybackState _playbackState; // 播放状态私有字段

        // 进度更新频率控制
        private readonly TimeSpan _progressUpdateInterval = TimeSpan.FromMilliseconds(500); // 每0.5秒更新一次（一秒两次）
        private DateTime _lastProgressUpdateTime = DateTime.MinValue;

        // 重写OnPropertyChanged方法，添加调试信息
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(BottomControlContent))
            {
                System.Diagnostics.Debug.WriteLine($"BottomControlContent已更新: {BottomControlContent != null}");
            }
        }

        // MVVM数据绑定属性 - 这些属性会自动通知UI更新

        [ObservableProperty]
        private bool _isVlcInitialized = false;                   // VLC是否已初始化

        [ObservableProperty]
        private bool _isDragging = false;                         // 滑块是否正在拖动

        [ObservableProperty]
        private bool _isControlsVisible = false;                  // 全屏时控制栏是否可见

        [ObservableProperty]
        private object? _bottomControlContent;                    // 全屏时显示的底部控制栏内容

        // 全屏样式属性 - 通过数据绑定控制UI
        [ObservableProperty]
        private CornerRadius _outerBorderCornerRadius = new CornerRadius(8);

        [ObservableProperty]
        private Thickness _outerBorderPadding = new Thickness(5);

        [ObservableProperty]
        private Thickness _outerBorderMargin = new Thickness(10);

        [ObservableProperty]
        private double _outerBorderOpacity = 0.95;

        [ObservableProperty]
        private CornerRadius _innerBorderCornerRadius = new CornerRadius(8);

        [ObservableProperty]
        private Thickness _innerBorderPadding = new Thickness(0);

        [ObservableProperty]
        private Thickness _innerBorderBorderThickness = new Thickness(2);

        [ObservableProperty]
        private double _blurRadius = 8.0;

        [ObservableProperty]
        private Thickness _bottomControlPlaceholderMargin = new Thickness(0, 0, 0, 10);

        // 控制栏透明度管理
        [ObservableProperty]
        private double _controlBarOpacity = 1.0;  // 控制栏透明度，1.0为完全可见，0.01为几乎隐藏

        // 控制栏自动隐藏计时器
        private DispatcherTimer _hideTimer;

        /// <summary>
        /// 共享的播放状态对象，
        /// 作为ViewModel之间的共享数据源，避免重复属性和复杂的事件传递
        /// </summary>
        public Player.Core.Models.PlaybackState PlaybackState { get; }

        /// <summary>
        /// 构造函数 - 依赖注入模式
        /// </summary>
        /// <param name="vlcPlayerService">VLC播放器服务</param>
        /// <param name="messengerService">消息通信服务</param>
        /// <param name="playbackState">共享的播放状态实例（可选）</param>
        public MiddleViewModel(IVlcPlayerService vlcPlayerService, IMessengerService messengerService, Player.Core.Models.PlaybackState? playbackState = null)
        {
            _vlcPlayerService = vlcPlayerService ?? throw new ArgumentNullException(nameof(vlcPlayerService));
            _messengerService = messengerService ?? throw new ArgumentNullException(nameof(messengerService));

            // 使用传入的PlaybackState实例（如果提供）或创建新实例
            _playbackState = playbackState ?? new Player.Core.Models.PlaybackState();
            PlaybackState = _playbackState; // 确保两者引用相同的实例

            // 初始化控制栏自动隐藏计时器
            InitializeHideTimer();

            // 初始化全屏样式
            UpdateFullscreenStyles(_playbackState.IsFullscreen);

            // 订阅播放器服务事件 - 监听VLC播放器的原生事件
            SubscribeToPlayerEvents();

            // 订阅消息 - 监听来自其他ViewModel的消息
            _messengerService.Register<MediaSelectedMessage>(this, OnMediaSelectedMessage);
            _messengerService.Register<SeekMessage>(this, OnSeekMessage);
            _messengerService.Register<ChangeVolumeMessage>(this, OnChangeVolumeMessage);
            _messengerService.Register<ToggleMuteMessage>(this, OnToggleMuteMessage);
            _messengerService.Register<PlaybackStateCommandMessage>(this, OnPlaybackStateCommandMessage);
            _messengerService.Register<ChangePlaybackSpeedMessage>(this, OnChangePlaybackSpeedMessage);
            _messengerService.Register<FullscreenChangedMessage>(this, OnFullscreenChangedMessage);
        }
        
        // 添加一个公共方法来手动播放指定文件，用于测试和直接调用
        public void TestPlayMedia(string filePath)
        {
             
            try
            {
                // 确保VLC已初始化
                if (!IsVlcInitialized)
                {
                    InitializeVlc();
                }
                
                // 直接调用LoadMedia方法
                LoadMedia(filePath);
            }
            catch (Exception ex)
            {
                        }
        }

        #region Speed Conversion Methods

        // 根据索引获取速度值的方法
        private double GetSpeedFromIndex(int index)
        {
            return index switch
            {
                0 => 0.5,
                1 => 1.0,
                2 => 1.5,
                3 => 2.0,
                _ => 1.0 // 默认值
            };
        }

        #endregion

        /// <summary>
        /// 初始化控制栏自动隐藏计时器
        /// </summary>
        private void InitializeHideTimer()
        {
            _hideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 3秒后自动隐藏
            };
            _hideTimer.Tick += OnHideTimerTick;
        }

        /// <summary>
        /// 计时器触发事件处理
        /// </summary>
        private void OnHideTimerTick(object sender, EventArgs e)
        {
            // 停止计时器
            _hideTimer.Stop();
            
            // 只有在全屏状态下才执行隐藏
            if (PlaybackState.IsFullscreen)
            {
                // 直接设置控制栏透明度为极低值（0.01），几乎不可见但仍保持交互
                // 实际的平滑过渡效果通过XAML中的动画实现
                ControlBarOpacity = 0.01;
                
                System.Diagnostics.Debug.WriteLine("控制栏自动隐藏");
            }
        }

        /// <summary>
        /// 显示控制栏（鼠标进入时调用）
        /// </summary>
        public void ShowControlBar()
        {
            // 重置计时器
            _hideTimer.Stop();
            _hideTimer.Start();
            
            // 直接设置控制栏完全可见
            // 实际的平滑过渡效果通过XAML中的动画实现
            ControlBarOpacity = 1.0;
            
            System.Diagnostics.Debug.WriteLine("控制栏显示");
        }

        /// <summary>
        /// 重置隐藏计时器（鼠标离开时调用）
        /// </summary>
        public void ResetHideTimer()
        {
            // 只有在全屏状态下才启动计时器
            if (PlaybackState.IsFullscreen && _hideTimer != null)
            {
                _hideTimer.Stop();
                _hideTimer.Start();
                System.Diagnostics.Debug.WriteLine("重置控制栏隐藏计时器");
            }
        }

        /// <summary>
        /// 初始化VLC播放器
        /// </summary>
        public void InitializeVlc()
        {
            try
            {
                _vlcPlayerService.Initialize();
                IsVlcInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VLC初始化失败: {ex.Message}");
                // 可以通过事件总线通知UI显示错误
            }
        }

        /// <summary>
        /// 加载并播放媒体文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void LoadMedia(string filePath)
        {
            if (!IsVlcInitialized || string.IsNullOrEmpty(filePath))
            {
                    return;
            }

            try
            {
                _vlcPlayerService.LoadMedia(filePath);
                // 更新播放状态中的媒体路径
                PlaybackState.MediaPath = filePath;
                // 触发MediaPlayerChanged事件，通知UI更新绑定
                OnMediaPlayerChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载媒体失败: {ex.Message}");
                // 可以通过事件总线通知UI显示错误
            }
        }

        #region RelayCommands

        [RelayCommand]
        private void TogglePlayPause()
        {
            _vlcPlayerService.TogglePlayPause();
            // 播放状态将由播放器事件回调更新
        }

        [RelayCommand]
        private void Stop()
        {
            _vlcPlayerService.Stop();
            // 播放状态将由播放器事件回调更新
        }

        [RelayCommand]
        private void ToggleMute()
        {
            // 保存当前音量值（如果不是静音状态）
            int savedVolume = PlaybackState.IsMuted ? _playbackState.Volume : PlaybackState.Volume;
            
            PlaybackState.IsMuted = !PlaybackState.IsMuted;
            
            if (PlaybackState.IsMuted)
            {
                // 静音时将音量设为0，这样滑块会移动到最左侧
                _playbackState.Volume = 0;
                _vlcPlayerService.SetVolume(0);
            }
            else
            {
                // 取消静音时恢复之前的音量，如果之前音量为0则使用默认值50
                int volumeToRestore = savedVolume > 0 ? savedVolume : 50;
                _playbackState.Volume = volumeToRestore;
                _vlcPlayerService.SetVolume(volumeToRestore);
            }
            
            _vlcPlayerService.SetMute(PlaybackState.IsMuted);

            // 发送音量变更消息，通知UI更新
            _messengerService.Send(new VolumeChangedMessage(PlaybackState.Volume));
        }

        [RelayCommand]
        private void ToggleFullscreen()
        {
            // 通过消息系统通知全屏状态变化
            bool newFullscreenState = !PlaybackState.IsFullscreen;
            _messengerService.Send(new FullscreenChangedMessage(newFullscreenState));
            
            // 同时更新本地状态
            PlaybackState.IsFullscreen = newFullscreenState;
            
            // 更新全屏样式
            UpdateFullscreenStyles(newFullscreenState);
            
            // 触发PropertyChanged事件，通知UI PlaybackState已更新
            OnPropertyChanged(nameof(PlaybackState));
        }

        /// <summary>
        /// 根据全屏状态更新UI样式
        /// </summary>
        /// <param name="isFullscreen">是否全屏</param>
        private void UpdateFullscreenStyles(bool isFullscreen)
        {
            if (isFullscreen)
            {
                // 全屏样式：移除边距、边框和圆角
                OuterBorderCornerRadius = new CornerRadius(0);
                OuterBorderPadding = new Thickness(0);
                OuterBorderMargin = new Thickness(0);
                OuterBorderOpacity = 1.0;
                InnerBorderCornerRadius = new CornerRadius(0);
                InnerBorderPadding = new Thickness(0);
                InnerBorderBorderThickness = new Thickness(0);
                BlurRadius = 0.0;
                BottomControlPlaceholderMargin = new Thickness(0,0,0,10);//防止控制栏过于贴近屏幕下边缘
                
                // 重置控制栏透明度并启动计时器
                ControlBarOpacity = 1.0;
                ResetHideTimer();
            }
            else
            {
                // 非全屏样式：恢复默认值
                OuterBorderCornerRadius = new CornerRadius(8);
                OuterBorderPadding = new Thickness(5);
                OuterBorderMargin = new Thickness(10);
                OuterBorderOpacity = 0.95;
                InnerBorderCornerRadius = new CornerRadius(8);
                InnerBorderPadding = new Thickness(0);
                InnerBorderBorderThickness = new Thickness(2);
                BlurRadius = 8.0;
                BottomControlPlaceholderMargin = new Thickness(0);
                
                // 非全屏时恢复控制栏透明度并停止计时器
                ControlBarOpacity = 1.0;
                _hideTimer?.Stop();
            }
        }

        [RelayCommand]
        public void Seek(TimeSpan position)
        {
            _vlcPlayerService.Seek(position);
        }

        /// <summary>
        /// 切换全屏控制栏的显示状态
        /// </summary>
        public void ToggleFullscreenControls()
        {
            // 这个方法用于切换控制栏的显示状态
            // 在伪全屏模式下，我们仍然需要控制UI元素的显示/隐藏
            // 这里可以添加相关逻辑

        }



        [RelayCommand]
        private void ChangePlaybackSpeed(int speedIndex)
        {
            // 将索引转换为实际的速度值
            double speed = GetSpeedFromIndex(speedIndex);
            PlaybackState.PlaybackRate = speedIndex;
            _vlcPlayerService.SetPlaybackSpeed((float)speed);
        }

        #endregion

        #region Message Handlers

        private void OnMediaSelectedMessage(object recipient, MediaSelectedMessage message)
        {  if (message.Value != null && !string.IsNullOrEmpty(message.Value.Path))
            {
                // 确保VLC已初始化
                if (!IsVlcInitialized)
                {
                      InitializeVlc();
                }
                LoadMedia(message.Value.Path);
            }
        }

        private void OnSeekMessage(object recipient, SeekMessage message)
        {
            _vlcPlayerService.Seek(message.Value);
        }

        private void OnChangeVolumeMessage(object recipient, ChangeVolumeMessage message)
        {
            // 遵循单一可信源原则：MiddleViewModel负责实际处理音量变更
            int newVolume = Math.Clamp(message.Value, 0, 100); // 确保音量在有效范围内

            // 如果音量大于0且当前处于静音状态，自动取消静音
            if (newVolume > 0 && PlaybackState.IsMuted)
            {
                PlaybackState.IsMuted = false;
            }

            PlaybackState.Volume = newVolume;
            _vlcPlayerService.SetVolume(newVolume);
            _vlcPlayerService.SetMute(PlaybackState.IsMuted);

            // 发送音量变更消息，用于状态同步（其他ViewModel监听）
            _messengerService.Send(new VolumeChangedMessage(PlaybackState.Volume));
        }

        private void OnToggleMuteMessage(object recipient, ToggleMuteMessage message)
        {
            // 调用ToggleMute方法来处理静音逻辑，保持一致性
            ToggleMute();
        }



        private void OnPlaybackStateCommandMessage(object recipient, PlaybackStateCommandMessage message)
        {
            // 处理播放/暂停命令
            if (message.Value)
            {
                // 应该播放 - 调用TogglePlayPause来切换播放状态
                _vlcPlayerService.TogglePlayPause();
            }
            else
            {
                // 应该暂停 - 调用TogglePlayPause来切换播放状态
                _vlcPlayerService.TogglePlayPause();
            }
        }

        private void OnChangePlaybackSpeedMessage(object recipient, ChangePlaybackSpeedMessage message)
        {
            // 遵循单一可信源原则：MiddleViewModel负责实际处理播放速度变更
            int speedIndex = message.Value;
            // 直接调用ChangePlaybackSpeed方法来处理，保持一致性
            ChangePlaybackSpeed(speedIndex);
        }

        private void OnFullscreenChangedMessage(object recipient, FullscreenChangedMessage message)
        {
            // 当收到全屏状态变化消息时，更新样式
            System.Diagnostics.Debug.WriteLine($"MiddleViewModel收到全屏状态变化消息: {message.Value}");
            UpdateFullscreenStyles(message.Value);
        }

        #endregion

        #region Player Service Event Handlers

        private void SubscribeToPlayerEvents()
        {
            _vlcPlayerService.Playing += OnPlayerPlaying;
            _vlcPlayerService.Paused += OnPlayerPaused;
            _vlcPlayerService.Stopped += OnPlayerStopped;
            _vlcPlayerService.EndReached += OnPlayerEndReached;
            // 添加TimeChanged事件订阅，用于更新播放进度
            _vlcPlayerService.TimeChanged += OnPlayerTimeChanged;
            _vlcPlayerService.LengthChanged += OnPlayerLengthChanged;
            _vlcPlayerService.VolumeChanged += OnPlayerVolumeChanged;
        }

        private void UnsubscribeFromPlayerEvents()
        {
            _vlcPlayerService.Playing -= OnPlayerPlaying;
            _vlcPlayerService.Paused -= OnPlayerPaused;
            _vlcPlayerService.Stopped -= OnPlayerStopped;
            _vlcPlayerService.EndReached -= OnPlayerEndReached;
            _vlcPlayerService.TimeChanged -= OnPlayerTimeChanged;
            _vlcPlayerService.LengthChanged -= OnPlayerLengthChanged;
            _vlcPlayerService.VolumeChanged -= OnPlayerVolumeChanged;
        }

        private void OnPlayerPlaying(object? sender, EventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 更新共享播放状态
                    PlaybackState.IsPlaying = true;

                    // 发送播放状态变更消息
                    _messengerService.Send(new PlaybackStateChangedMessage(true));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"播放事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerPaused(object? sender, EventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 更新共享播放状态
                    PlaybackState.IsPlaying = false;

                    // 发送播放状态变更消息
                    _messengerService.Send(new PlaybackStateChangedMessage(false));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"暂停事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerStopped(object? sender, EventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 更新共享播放状态
                    PlaybackState.IsPlaying = false;

                    // 发送播放状态变更消息
                    _messengerService.Send(new PlaybackStateChangedMessage(false));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"停止事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerEndReached(object? sender, EventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 更新共享播放状态
                    PlaybackState.IsPlaying = false;

                    // 发送播放状态变更消息
                    _messengerService.Send(new PlaybackStateChangedMessage(false));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"播放结束事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 限制进度更新频率，避免过于频繁的UI更新
                    var currentTime = DateTime.Now;
                    if ((currentTime - _lastProgressUpdateTime) >= _progressUpdateInterval)
                    {
                        _lastProgressUpdateTime = currentTime;

                        // 正确更新实例属性而不是静态属性
                        _playbackState.PlaybackTime = e.Time;

                        // 计算进度百分比
                        if (_playbackState.TotalDuration > 0)
                        {
                            var progress = (double)e.Time / _playbackState.TotalDuration;
                            _playbackState.Position = (int)(progress * 100);

                            // 发送进度更新消息（0-1范围的浮点数）
                            _messengerService.Send(new ProgressUpdatedMessage(progress));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"时间变更事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 正确更新实例属性而不是静态属性
                    _playbackState.TotalDuration = e.Length;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"时长变更事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnPlayerVolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
        {
            // 使用Dispatcher确保UI线程安全
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 更新共享播放状态 - 确保音量在有效范围内
                    int volumeInt = Math.Clamp(Convert.ToInt32(e.Volume), 0, 100);
                    PlaybackState.Volume = volumeInt;

                    // 同时更新静音状态
                    var isMuted = _vlcPlayerService.MediaPlayer?.Mute ?? (volumeInt == 0);
                    if (PlaybackState.IsMuted != isMuted)
                    {
                        PlaybackState.IsMuted = isMuted;
                    }

                    // 发送音量更新消息
                    _messengerService.Send(new VolumeChangedMessage(volumeInt));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"音量变更事件处理异常: {ex.Message}");
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        #endregion

        #region Public Properties for UI Binding

        /// <summary>
        /// 获取MediaPlayer实例，供UI绑定使用
        /// 
        /// MVVM数据绑定说明：
        /// - 这个属性允许XAML界面直接绑定到VLC播放器控件
        /// - 在MiddleControl.xaml中，VideoView控件的MediaPlayer属性绑定到此属性
        /// - 当MediaPlayer发生变化时，UI会自动更新
        /// </summary>
        public MediaPlayer? MediaPlayer => _vlcPlayerService.MediaPlayer;

        /// <summary>
        /// MediaPlayer属性变更事件
        /// 
        /// MVVM事件模式说明：
        /// - 当MediaPlayer实例发生变化时触发此事件
        /// - UI控件可以监听此事件来更新绑定
        /// </summary>
        public event EventHandler? MediaPlayerChanged;

        /// <summary>
        /// 触发MediaPlayerChanged事件
        /// </summary>
        private void OnMediaPlayerChanged()
        {
            MediaPlayerChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    UnsubscribeFromPlayerEvents();

                    // 停止并释放计时器
                    _hideTimer?.Stop();
                    _hideTimer = null;

                    _messengerService.UnregisterAll(this);

                    _vlcPlayerService.Dispose();
                }

                _disposed = true;
            }
        }

        ~MiddleViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}