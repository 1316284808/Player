using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Player.Core.Models;
using Player.Core.Events;
using Player.Core.Services;
using Player.Core.ViewModels;
using Player.Core.Repositories;

namespace Player.ViewModels
{
    /// <summary>
    /// 主视图模型 (MainViewModel)
    /// 
    /// MVVM模式说明：
    /// - Model: 数据模型，如MediaItem、PlaybackState等
    /// - View: XAML界面文件，如MainWindow.xaml
    /// - ViewModel: 本类，负责连接View和Model，处理业务逻辑
    /// 
    /// 主要职责：
    /// 1. 管理应用程序的整体状态
    /// 2. 协调各个子ViewModel之间的通信
    /// 3. 处理用户界面交互命令
    /// 4. 维护播放列表和播放状态
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        // 依赖注入的服务接口
        private readonly IMessengerService? _messengerService;
        private readonly IMediaRepository? _mediaRepository;

        // 事件定义
        public event EventHandler? MediaPlayerChanged;

        // MVVM数据绑定属性 - 这些属性会自动通知UI更新
        // [ObservableProperty]特性会自动生成属性变更通知代码
        
        private PlaybackState _playbackState = new();        // 播放状态（播放/暂停/停止等）
        
        public PlaybackState PlaybackState
        {
            get => _playbackState;
            set
            {
                if (_playbackState != value)
                {
                    _playbackState = value;
                    OnPropertyChanged(nameof(PlaybackState));
                }
            }
        }

        [ObservableProperty]
        private string _currentTime = "0:00";                // 当前播放时间显示

        [ObservableProperty]
        private string _totalTime = "0:00";                   // 总时长显示

        [ObservableProperty]
        private double _progress;                             // 播放进度（0-1）

        // 移除重复状态，统一使用PlaybackState作为唯一数据源

        [ObservableProperty]
        private bool _isUserInitiated = false;                 // 是否为用户主动操作（避免循环事件）

        [ObservableProperty]
        private string _currentVideoPath = string.Empty;       // 当前播放的视频路径

        [ObservableProperty]
        private ObservableCollection<MediaItem> _playlist = new();  // 播放列表

        [ObservableProperty]
        private MediaItem? _selectedMediaItem;                 // 当前选中的媒体项

        [ObservableProperty]
        private bool _isFullscreen = false;                    // 是否全屏

        [ObservableProperty]
        private bool _isLeftPanelVisible = true;               // 左侧面板是否可见

        [ObservableProperty]
        private string _windowTitle = "媒体播放器";            // 窗口标题

        [ObservableProperty]
        private string _windowState = "Normal";                // 窗口状态（正常/最大化）

        [ObservableProperty]
        private double _windowWidth = 1200;                    // 窗口宽度

        [ObservableProperty]
        private double _windowHeight = 800;                    // 窗口高度

        [ObservableProperty]
        private double _windowPositionX = 100;                // 窗口X坐标

        [ObservableProperty]
        private double _windowPositionY = 100;                 // 窗口Y坐标

        [ObservableProperty]
        private GridLength _leftPanelWidth = new GridLength(300);  // 左侧面板宽度
        
        /// <summary>
        /// 中间区域ViewModel - 负责视频播放控制
        /// </summary>
        public MiddleViewModel? MiddleViewModel { get; set; }

        /// <summary>
        /// 无参构造函数 - 用于XAML设计时实例化
        /// </summary>
        public MainViewModel()
        {
            // 设计时构造函数，不进行任何初始化
            // 运行时将通过依赖注入的构造函数进行初始化
        }

        /// <summary>
        /// 构造函数 - 用于运行时使用
        /// </summary>
        public MainViewModel(IMessengerService messengerService, IMediaRepository mediaRepository)
        {
            _messengerService = messengerService ?? throw new ArgumentNullException(nameof(messengerService));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            
            // 不再需要进度更新定时器，现在使用VLC的TimeChanged事件
            
            // 使用消息服务注册消息处理器
            _messengerService.Register<MediaSelectedMessage>(this, OnMediaSelectedMessage);
            _messengerService.Register<PlaybackStateChangedMessage>(this, OnPlaybackStateChangedMessage);
            _messengerService.Register<VolumeChangedMessage>(this, OnVolumeChangedMessage);
            _messengerService.Register<ProgressUpdatedMessage>(this, OnProgressUpdatedMessage);
            _messengerService.Register<OpenSettingsMessage>(this, OnOpenSettingsMessage);
            _messengerService.Register<FullscreenChangedMessage>(this, OnFullscreenChangedMessage);
            _messengerService.Register<PlaylistUpdatedMessage>(this, OnPlaylistUpdatedMessage);
        }

        #region RelayCommands

        /// <summary>
        /// 播放/暂停切换命令
        /// </summary>
        [RelayCommand]
        private void PlayPause()
        {
            // 发送播放状态变更命令消息，通知MiddleViewModel处理播放/暂停
            _messengerService?.Send(new PlaybackStateCommandMessage(!PlaybackState.IsPlaying));
            
            // 移除定时器控制，现在使用VLC的TimeChanged事件更新进度
        }

        /// <summary>
        /// 全屏切换命令
        /// </summary>
        [RelayCommand]
        private void ToggleFullscreen()
        {
            // 通过MiddleViewModel处理全屏切换，使用WPF伪全屏功能
            MiddleViewModel?.ToggleFullscreenCommand.Execute(null);
        }

        /// <summary>
        /// 打开文件命令
        /// </summary>
        [RelayCommand]
        private async Task OpenFileAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "媒体文件|*.mp4;*.avi;*.mkv;*.wmv;*.mov;*.flv;*.m4v;*.webm|所有文件|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Playlist.Clear();
                var mediaItems = new List<MediaItem>();
                
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var mediaItem = new MediaItem
                    {
                        Name = Path.GetFileName(filePath),
                        Path = filePath
                    };
                    Playlist.Add(mediaItem);
                    mediaItems.Add(mediaItem);
                }
                
                if (Playlist.Count > 0)
                {
                    SelectedMediaItem = Playlist[0];
                    PlayMedia(SelectedMediaItem.Path);
                    
                    // 保存播放历史到Repository
                    await _mediaRepository.SaveMediaHistoryAsync(mediaItems);
                }
            }
        }

        /// <summary>
        /// 选择媒体命令
        /// </summary>
        [RelayCommand]
        private void SelectMedia(MediaItem mediaItem)
        {
            if (mediaItem != null)
            {
                SelectedMediaItem = mediaItem;
                PlayMedia(SelectedMediaItem.Path);
            }
        }

        /// <summary>
        /// 切换左侧面板命令
        /// </summary>
        [RelayCommand]
        private void ToggleLeftPanel()
        {
            IsLeftPanelVisible = !IsLeftPanelVisible;
        }

        /// <summary>
        /// 快进命令
        /// </summary>
        [RelayCommand]
        private void SeekForward()
        {
            if (PlaybackState.TotalDuration > 0)
            {
                var newTime = Math.Min(PlaybackState.CurrentTime + 10000, PlaybackState.TotalDuration);
                PlaybackState.CurrentTime = newTime;
                PlaybackState.Position = (double)newTime / PlaybackState.TotalDuration * 100;
                _messengerService?.Send(new SeekMessage(TimeSpan.FromMilliseconds(newTime)));
            }
        }

        /// <summary>
        /// 快退命令
        /// </summary>
        [RelayCommand]
        private void SeekBackward()
        {
            if (PlaybackState.TotalDuration > 0)
            {
                var newTime = Math.Max(PlaybackState.CurrentTime - 10000, 0);
                PlaybackState.CurrentTime = newTime;
                PlaybackState.Position = (double)newTime / PlaybackState.TotalDuration * 100;
                _messengerService?.Send(new SeekMessage(TimeSpan.FromMilliseconds(newTime)));
            }
        }

        /// <summary>
        /// 增加音量命令
        /// 
        /// 遵循单一可信源原则：发送音量变更命令，由MiddleViewModel处理
        /// </summary>
        [RelayCommand]
        private void IncreaseVolume()
        {
            int newVolume = Math.Min(PlaybackState.Volume + 5, 100);
            _messengerService?.Send(new ChangeVolumeMessage(newVolume));
        }

        /// <summary>
        /// 减少音量命令
        /// 
        /// 遵循单一可信源原则：发送音量变更命令，由MiddleViewModel处理
        /// </summary>
        [RelayCommand]
        private void DecreaseVolume()
        {
            int newVolume = Math.Max(PlaybackState.Volume - 5, 0);
            _messengerService?.Send(new ChangeVolumeMessage(newVolume));
        }

        /// <summary>
        /// 静音切换命令
        /// 
        /// 遵循单一可信源原则：发送静音切换命令，由MiddleViewModel处理
        /// </summary>
        [RelayCommand]
        private void ToggleMute()
        {
            _messengerService?.Send(new ToggleMuteMessage());
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 进度更新定时器事件
        /// </summary>
        // 移除进度更新定时器逻辑，现在使用VLC的TimeChanged事件更新进度
        // private void OnProgressTimerTick(object? sender, EventArgs e)
        // {
        //     // 重构：使用定时器稳定更新进度，避免VLC事件循环问题
        //     if (MiddleViewModel?.MediaPlayer != null && PlaybackState.IsPlaying)
        //     {
        //         try
        //         {
        //             var currentTime = MiddleViewModel.MediaPlayer.Time;
        //             var totalTime = MiddleViewModel.MediaPlayer.Length;
        //             
        //             if (totalTime > 0)
        //             {
        //                 // 直接更新共享状态，避免事件循环
        //                 PlaybackState.CurrentTime = currentTime;
        //                 PlaybackState.TotalDuration = totalTime;
        //                 PlaybackState.Position = (double)currentTime / totalTime * 100;
        //                 
        //                 // 更新UI显示的时间
        //                 CurrentTime = FormatTime(currentTime);
        //                 TotalTime = FormatTime(totalTime);
        //                 Progress = (double)currentTime / totalTime;
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             // 忽略定时器异常，避免UI卡死
        //             System.Diagnostics.Debug.WriteLine($"进度更新异常: {ex.Message}");
        //         }
        //     }
        // }

        /// <summary>
        /// 处理播放状态变更消息
        /// </summary>
        private void OnPlaybackStateChangedMessage(object recipient, PlaybackStateChangedMessage message)
        {
            // 更新共享播放状态
            PlaybackState.IsPlaying = message.Value;
            
            // 已移除进度更新定时器，不再需要同步控制
        }
        
        /// <summary>
        /// 处理音量变更消息
        /// </summary>
        private void OnVolumeChangedMessage(object recipient, VolumeChangedMessage message)
        {
            // 更新音量显示 - 统一使用PlaybackState作为单一数据源
            PlaybackState.Volume = message.Value;
        }
        
        /// <summary>
        /// 处理进度更新消息
        /// </summary>
        private void OnProgressUpdatedMessage(object recipient, ProgressUpdatedMessage message)
        {
            // 确保不是用户主动拖动进度条导致的更新，避免循环更新
            if (!IsUserInitiated)
            {
                // 更新进度显示
                Progress = message.Value / 100.0; // 转换为0-1范围
                
                // 同时更新UI显示的时间
                CurrentTime = FormatTime(PlaybackState.CurrentTime);
                TotalTime = FormatTime(PlaybackState.TotalDuration);
            }
        }



        /// <summary>
        /// 处理来自LeftViewModel的媒体选择消息
        /// </summary>
        private void OnMediaSelectedMessage(object recipient, MediaSelectedMessage message)
        {
            if (message.Value != null && !string.IsNullOrEmpty(message.Value.Path))
            {
                // 不再在这里处理媒体播放，而是让MiddleViewModel处理
                // 这里只更新共享的PlaybackState状态
                PlaybackState.MediaPath = message.Value.Path;
                PlaybackState.WindowTitle = $"媒体播放器 - {System.IO.Path.GetFileName(message.Value.Path)}";
                
                // 更新当前视频路径
                CurrentVideoPath = message.Value.Path;
            }
        }
        
        /// <summary>
        /// 处理全屏状态变更消息
        /// </summary>
        private void OnFullscreenChangedMessage(object recipient, FullscreenChangedMessage message)
        {
            // 更新共享播放状态的全屏状态
            PlaybackState.IsFullscreen = message.Value;
        }

        /// <summary>
        /// 处理打开设置窗体的消息
        /// </summary>
        private void OnOpenSettingsMessage(object recipient, OpenSettingsMessage message)
        {
            try
            {
                // 直接创建并显示设置窗体
                var settingsWindow = new Player.Left.SettingsDialog();
                
                // 设置窗体的所有者为主窗口
                if (Application.Current.MainWindow != null)
                {
                    settingsWindow.Owner = Application.Current.MainWindow;
                    settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                
                // 显示窗体
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                // 使用消息框显示错误信息
                MessageBox.Show(Application.Current.MainWindow, 
                    $"打开设置窗体失败: {ex.Message}", 
                    "错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理播放列表更新消息
        /// </summary>
        private void OnPlaylistUpdatedMessage(object recipient, PlaylistUpdatedMessage message)
        {
            try
            {
                // 清空当前播放列表
                Playlist.Clear();
                
                // 添加新的播放列表项
                if (message.Value != null)
                {
                    foreach (var mediaItem in message.Value)
                    {
                        Playlist.Add(mediaItem);
                    }
                }
                
                // 自动选择第一个媒体项（可选）
                if (Playlist.Count > 0)
                {
                    SelectedMediaItem = Playlist[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理播放列表更新消息失败: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 触发MediaPlayerChanged事件
        /// </summary>
        public void OnMediaPlayerChanged()
        {
            MediaPlayerChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 播放媒体文件
        /// </summary>
        public void PlayMedia(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 更新共享PlaybackState作为单一数据源
            PlaybackState.MediaPath = filePath;
            PlaybackState.IsPlaying = true;
            PlaybackState.ControlsEnabled = true;
            PlaybackState.WindowTitle = $"媒体播放器 - {System.IO.Path.GetFileName(filePath)}";
            
            // 更新当前视频路径
            CurrentVideoPath = filePath;
            
            // 不再需要启动进度更新定时器，现在使用VLC的TimeChanged事件
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        private static string FormatTime(long milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            return timeSpan.Hours > 0 
                ? timeSpan.ToString(@"h\:mm\:ss") 
                : timeSpan.ToString(@"m\:ss");
        }

        #endregion

        #region Property Changed Handlers

        /// <summary>
        /// 选中媒体项变更处理
        /// </summary>
        partial void OnSelectedMediaItemChanged(MediaItem? value)
        {
            if (value != null)
            {
                PlayMedia(value.Path);
            }
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            // 不再需要清理_progressTimer
            _messengerService?.UnregisterAll(this);
            
            // PlaybackState现在是纯数据模型，不再需要监听PropertyChanged事件
        }

        #endregion
    }
}