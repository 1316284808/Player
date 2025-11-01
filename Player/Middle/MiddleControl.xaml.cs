using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Player.Helpers;
using Player.Core.Models;

namespace Player.Middle
{
    /// <summary>
    /// 中间区域控制，负责视频播放和显示逻辑
    /// </summary>
    public partial class MiddleControl : UserControl, IDisposable
    {
        // 窗口状态相关字段
        private bool _isWindowMaximized = false;
        private Window _mainWindow;
        private Thickness _originalMargin;
        
        // UI相关字段
        private Border _blackPlaceholder;
        
        // VLC播放器相关字段
        private LibVLC _libVlc;
        public MediaPlayer _mediaPlayer;
        private string _currentVideoPath;
        private long _currentPlaybackTime;
        private bool _wasPlaying;
        
        // 全屏相关字段
        private bool _isVideoFullscreen = false;
        private FullscreenWindow _fullscreenWindow;
        
        public MiddleControl()
        {
            InitializeComponent();
            
            // 创建黑色占位符
            _blackPlaceholder = new Border { Background = Brushes.Black };
            
            // 初始显示黑屏
            PlayerHost.Content = _blackPlaceholder;
            
            // 设置ContentControl背景为黑色
            PlayerHost.Background = Brushes.Black;
            
            // 获取主窗口引用并初始化事件
            Loaded += OnLoaded;
        }
        
        /// <summary>
        /// 控件加载完成时的初始化逻辑
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _mainWindow = Window.GetWindow(this);
            if (_mainWindow != null)
            {
                _mainWindow.StateChanged += MainWindow_StateChanged;
                _originalMargin = this.Margin;
            }
            
            // 延迟VLC初始化直到布局完成
            Task.Run(async () =>
            {
                await Task.Delay(100); // 确保布局稳定
                Dispatcher.Invoke(() => InitializeVlc());
            });
            
            // 监听大小变更
            SizeChanged += OnSizeChanged;
            
            // 移除事件订阅避免内存泄漏
            Loaded -= OnLoaded;
        }

        ~MiddleControl()
        {
            Dispose(false);
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (_mainWindow == null || IsFullscreen) return;
            
            // 处理窗口最大化/恢复逻辑
            if (_mainWindow.WindowState == WindowState.Maximized && !_isWindowMaximized)
            {
                _isWindowMaximized = true;
                MaximizeVideoViewLayout();
            }
            else if (_mainWindow.WindowState != WindowState.Maximized && _isWindowMaximized)
            {
                _isWindowMaximized = false;
                RestoreVideoViewLayout();
            }
            
            // 窗口状态变化时刷新视频显示
            RefreshVideoOnWindowStateChange();
        }
        
        /// <summary>
        /// 窗口状态变化时刷新视频显示
        /// </summary>
        private void RefreshVideoOnWindowStateChange()
        {
            TransitionOverlay.Visibility = Visibility.Visible;
            
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(async () =>
            {
                await Task.Delay(10); // 等待布局完成
                
                // 刷新视频播放
                if (_mediaPlayer != null)
                {
                    try
                    {
                        RefreshMediaPlayer();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"刷新VLC输出失败: {ex.Message}");
                    }
                }
                
                // 延迟隐藏覆盖层，确保渲染完成
                await Task.Delay(50);
                TransitionOverlay.Visibility = Visibility.Collapsed;
            }));
        }
        
        /// <summary>
        /// 刷新MediaPlayer（使用暂停/播放方式）
        /// </summary>
        private void RefreshMediaPlayer()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _mediaPlayer.Play();
            }
        }

     

        /// <summary>
        /// 最大化时调整布局
        /// </summary>
        private void MaximizeVideoViewLayout()
        {
            try
            {
                // 保存原始边距（如果尚未保存）
                if (_originalMargin == default(Thickness))
                {
                    _originalMargin = this.Margin;
                }
                
                // 调整边距以适应最大化窗口
                this.Margin = new Thickness(0);
                
                // 确保控件正确拉伸显示
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                
                // 刷新视频显示
                RefreshVideoDisplayImmediately();
                
                // 延迟再次刷新，确保布局完全更新
                Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    try
                    {
                        RefreshVideoDisplay();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"最大化时刷新视频显示失败: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"最大化调整videoView布局失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复原始布局
        /// </summary>
        private void RestoreVideoViewLayout()
        {
            try
            {
                // 恢复原始边距
                if (_originalMargin != default(Thickness))
                {
                    this.Margin = _originalMargin;
                }
                
                // 确保控件正确拉伸显示
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                
                // 立即刷新视频显示，确保VLC渲染器正确响应窗口变化
                RefreshVideoDisplayImmediately();
                
                // 延迟再次刷新，确保布局完全恢复
                Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    try
                    {
                        RefreshVideoDisplay();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"恢复时刷新视频显示失败: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"恢复videoView布局失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新视频显示
        /// </summary>
        private void RefreshVideoDisplay()
        {            
            if (!HasActiveVideoView()) return;
            
            var videoViewControl = GetVideoViewControl();
            if (videoViewControl == null) return;
            
            // 保存当前播放状态
            long currentTime = _mediaPlayer.Time;
            bool isPlaying = _mediaPlayer.IsPlaying;
            
            // 临时解绑并重新绑定MediaPlayer以强制刷新视频渲染器
            videoViewControl.MediaPlayer = null;
            videoViewControl.MediaPlayer = _mediaPlayer;
            
            // 恢复播放位置和状态
            try
            {
                _mediaPlayer.Time = currentTime;
                if (isPlaying)
                {
                    _mediaPlayer.Play();
                }
            }
            catch { /* 忽略播放状态恢复时可能的错误 */ }
            
            // 强制更新布局
            videoViewControl.InvalidateVisual();
            this.InvalidateVisual();
        }

        /// <summary>
        /// 立即刷新视频显示（用于窗口状态变化时的快速响应）
        /// </summary>
        private void RefreshVideoDisplayImmediately()
        {            
            if (!HasActiveVideoView()) return;
            
            var videoViewControl = GetVideoViewControl();
            if (videoViewControl == null) return;
            
            // 快速刷新：仅重新绑定MediaPlayer
            videoViewControl.MediaPlayer = null;
            videoViewControl.MediaPlayer = _mediaPlayer;
            
            // 立即强制重绘和布局更新
            videoViewControl.InvalidateVisual();
            this.InvalidateVisual();
            this.UpdateLayout();
            videoViewControl.UpdateLayout();
            
            // 触发内部刷新
            if (_mediaPlayer.IsPlaying)
            {
                // 使用音量调整触发内部刷新（轻量级操作）
                var vol = _mediaPlayer.Volume;
                _mediaPlayer.Volume = vol;
            }
        }
        
        /// <summary>
        /// 检查是否有活跃的VideoView控件
        /// </summary>
        private bool HasActiveVideoView()
        {
            return _mediaPlayer != null && PlayerHost != null && PlayerHost.Content is LibVLCSharp.WPF.VideoView;
        }
        
        /// <summary>
        /// 获取当前的VideoView控件
        /// </summary>
        private LibVLCSharp.WPF.VideoView GetVideoViewControl()
        {
            if (PlayerHost == null || PlayerHost.Content == null)
                return null;
            return PlayerHost.Content as LibVLCSharp.WPF.VideoView;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 移除事件订阅
                if (_mainWindow != null)
                {
                    _mainWindow.StateChanged -= MainWindow_StateChanged;
                    _mainWindow = null!;
                }
                
                SizeChanged -= OnSizeChanged;
                
                // 如果在视频全屏模式，先退出
                if (IsFullscreen)
                {
                    ExitFullscreen();
                }
                
                // 释放VLC资源
                DisposeMediaPlayer();
                
                if (_libVlc != null)
                {
                    _libVlc.Dispose();
                    _libVlc = null!;
                }
            }
        }

        /// <summary>
        /// 更新播放状态（从全屏窗口恢复时使用）
        /// </summary>
        /// <param name="playbackTime">播放时间（毫秒）</param>
        /// <param name="isPlaying">是否正在播放</param>
        public void UpdatePlaybackState(long playbackTime, bool isPlaying)
        {
            _currentPlaybackTime = playbackTime;
            _wasPlaying = isPlaying;
        }
        
        /// <summary>
        /// 获取当前播放状态
        /// </summary>
        /// <returns>播放状态对象</returns>
        public PlaybackState GetCurrentPlaybackState()
        {
            return new PlaybackState
            {
                PlaybackTime = _mediaPlayer?.Time ?? 0,
                IsPlaying = _mediaPlayer?.IsPlaying ?? false,
                MediaPath = _currentVideoPath,
                TotalDuration = _mediaPlayer?.Length ?? 0
            };
        }
        
        /// <summary>
        /// 设置播放状态
        /// </summary>
        /// <param name="playbackState">播放状态对象</param>
        public void SetPlaybackState(PlaybackState playbackState)
        {
            if (playbackState == null || _mediaPlayer == null) return;
            
            _currentPlaybackTime = playbackState.PlaybackTime;
            _wasPlaying = playbackState.IsPlaying;
            
            // 如果提供了媒体路径且与当前不同，则加载新媒体
            if (!string.IsNullOrEmpty(playbackState.MediaPath) && playbackState.MediaPath != _currentVideoPath)
            {
                _currentVideoPath = playbackState.MediaPath;
                // 这里可以添加加载媒体的逻辑
            }
        }


        /// <summary>
        /// 初始化 VLC 播放器
        /// </summary>
        private void InitializeVlc()
        {
            try
            {
                // 1. 释放旧资源（避免内存泄漏）
                _mediaPlayer?.Stop();
                _mediaPlayer?.Dispose();
                _mediaPlayer = null!;
                
                _libVlc?.Dispose();
                _libVlc = null!;

                // 2. 初始化 LibVLC 核心
                LibVLCSharp.Shared.Core.Initialize();

                // 3. 获取硬件设置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();
                
                // 确保容器背景为黑色（防止透明变白）已在构造函数中设置

                // 4. 构建 VLC 选项（根据配置动态生成）
                var vlcOptions = new List<string>
                {
                    // "--no-osd",     // 隐藏默认进度条
                    "--quiet",       // 减少日志输出
                    
                    // 防止画面撕裂的优化设置
                    $"--vout={hardwareSettings.VideoRenderer}",           // 使用指定的视频渲染器
                    hardwareSettings.UseSystemMemory ? "--directx-use-sysmem" : "",        // 使用系统内存避免显存问题
                    hardwareSettings.EnableHardwareYUV ? "--directx-hw-yuv" : "",           // 启用硬件YUV转换
                    "--directx-device=auto",       // 自动选择最佳显卡设备
                    
                    // 视频同步和渲染优化
                    hardwareSettings.SkipLoopFilter ? "--avcodec-skiploopfilter=all" : "", // 跳过环路滤波提高性能
                    hardwareSettings.EnableFastDecoding ? "--avcodec-fast" : "",              // 启用快速解码模式
                    hardwareSettings.NoDropLateFrames ? "--no-drop-late-frames" : "",       // 不丢弃延迟的帧
                    hardwareSettings.NoSkipFrames ? "--no-skip-frames" : "",            // 不跳过帧
                    
                    // 缓存设置优化
                    $"--file-caching={hardwareSettings.FileCaching}",          // 文件缓存
                    $"--network-caching={hardwareSettings.NetworkCaching}",        // 网络缓存
                    
                    // 线程优化
                    hardwareSettings.AutoThreads ? "--avcodec-threads=0" : "",         // 自动选择线程数
                    hardwareSettings.EnableHurryUpDecoding ? "--avcodec-hurry-up" : "",          // 启用快速解码
                    
                    // 去隔行扫描优化
                    hardwareSettings.EnableDeinterlace ? $"--deinterlace={hardwareSettings.DeinterlaceMode}" : "",         // 使用指定的去隔行模式
                    hardwareSettings.EnableDeinterlace ? $"--deinterlace-mode={hardwareSettings.DeinterlaceMode}" : ""     // 指定去隔行模式
                };
                
                // 移除空字符串选项
                vlcOptions = vlcOptions.Where(opt => !string.IsNullOrEmpty(opt)).ToList();

                // 动态添加 GPU 加速选项 - 使用配置
                if (hardwareSettings.EnableGPUAcceleration)
                {
                    // 根据硬件平台自动选择最佳加速方式
                    vlcOptions.Add("--avcodec-hw=auto");         // 自动检测并使用最佳硬件加速
                    vlcOptions.Add("--hwdec=auto");              // 硬件解码自动选择
                    vlcOptions.Add("--avcodec-hw-device=any");    // 任意可用的硬件设备
                    vlcOptions.Add("--no-avcodec-dr");            // 禁用直接渲染（避免某些兼容性问题）
                    vlcOptions.Add("--demux-filter=avcodec");     // 启用AVCodec解复用滤镜
                   
                }
                else
                {
                    vlcOptions.Add("--avcodec-hw=none");          // 禁用硬件加速
                    
                }

                // 动态添加视频增强选项 - 使用配置
                if (hardwareSettings.EnableSuperResolution)
                {
                    // 启用高级视频滤镜链
                    vlcOptions.Add("--video-filter=deinterlace,sharpness");
                    
                    // 锐化设置
                    vlcOptions.Add("--sharpness-sharpen=0.6");    // 增强锐化效果
                    
                    // 去块滤镜（减少压缩失真）
                    vlcOptions.Add("--avcodec-skiploopfilter=0"); // 完全解码所有宏块
                    vlcOptions.Add("--avcodec-skip-frame=0");     // 不跳过任何帧
                    
                    // 启用HDR支持（如果有）
                    vlcOptions.Add("--no-hd1080i-deinterlace");   // 避免1080i内容的不必要去隔行
                    
                    // 性能优化
                    vlcOptions.Add("--avcodec-threads=0");        // 自动线程数

                }

                // 4. 创建新 LibVLC 实例和播放器
                _libVlc = new LibVLC(vlcOptions.ToArray());
                _mediaPlayer = new MediaPlayer(_libVlc);

                // 5. 不再直接绑定到UI控件，而是通过PlayMedia方法动态创建

                // 6. 初始化后保持黑屏状态，不自动恢复播放
                
                // 初始化完成后，保持黑屏状态，等待用户选择视频

                // 7. 启用控件
                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                  
                    viewModel.ControlsEnabled = true;
                    viewModel.IsUserInitiated = false;
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"初始化 VLC 播放器时出错: {ex.Message}\n请确保 LibVLC 库正确安装。");
            }
        }



        /// <summary>
        /// 保存当前播放状态
        /// </summary>
        private void SavePlaybackState()
        {
            if (_mediaPlayer?.Media != null)
            {
                _currentVideoPath = _mediaPlayer.Media.Mrl;
                _currentPlaybackTime = _mediaPlayer.Time;
                _wasPlaying = _mediaPlayer.IsPlaying;
                // 播放状态保存为内部操作，无需通知用户
            }
        }

        /// <summary>
        /// 停止播放并释放资源
        /// </summary>
        public void StopPlayback()
        {
            if (PlayerHost.Content is LibVLCSharp.WPF.VideoView)
            {
                DisposeMediaPlayer();
                PlayerHost.Content = _blackPlaceholder; // 回到黑屏
            }
        }
        
        /// <summary>
        /// 释放媒体播放器资源
        /// </summary>
        private void DisposeMediaPlayer()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _mediaPlayer = null!;
            }
        }
        
        /// <summary>
        /// 播放媒体文件
        /// </summary>
        public void PlayMedia(string filePath)
        {
            try
            {
                // 1. 如果已有播放器，先释放
                DisposeMediaPlayer();

                // 2. 确保LibVLC已初始化
                if (_libVlc == null)
                {
                    InitializeVlc();
                    if (_libVlc == null)
                    {
                        SystemNotificationHelper.ShowError("VLC播放器初始化失败");
                        return;
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    SystemNotificationHelper.ShowWarning($"文件不存在: {filePath}");
                    return;
                }

                // 3. 创建新的MediaPlayer
                _mediaPlayer = new MediaPlayer(_libVlc);
                
                // 4. 创建VideoView并替换占位符
                var videoViewControl = new LibVLCSharp.WPF.VideoView
                {
                    MediaPlayer = _mediaPlayer,
                    Background = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // 添加SizeChanged事件监听，实现DPI适配和强制VLC更新输出尺寸
                videoViewControl.SizeChanged += HandleVideoViewSizeChanged;

                // 6. 替换UI
                PlayerHost.Content = videoViewControl;

                // 6. 保存当前播放路径
                _currentVideoPath = filePath;
                
                // 7. 播放
                var media = new Media(_libVlc, new Uri(filePath));
                _mediaPlayer.Play(media);

                // 8. 更新ViewModel状态
                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                    viewModel.IsPlaying = true;
                    viewModel.IsUserInitiated = true;
                }

            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"播放媒体失败: {ex.Message}");
            }
        }
        
  
       
        // 暂停/恢复播放
        public void TogglePlayPause()
        {
            if (_mediaPlayer != null)
            {
                // 检查是否播放完毕（进度接近100%）
                bool isAtEnd = false;
                if (_mediaPlayer.Length > 0)
                {
                    double progress = (double)_mediaPlayer.Time / _mediaPlayer.Length;
                    isAtEnd = progress >= 0.99; // 进度超过99%视为播放完毕
                }
                
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                }
                else
                {
                    // 如果已经播放完毕，重置到开始位置
                    if (isAtEnd)
                    {
                        _mediaPlayer.Time = 0;
                    }
                    _mediaPlayer.Play();
                }

                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                    viewModel.IsPlaying = _mediaPlayer.IsPlaying;
                    viewModel.IsUserInitiated = true;
                }
            }
        }

        // 设置音量
        public void SetVolume(int volume)
        {
            if (_mediaPlayer != null)
            {
                // 确保音量在0-100范围内
                volume = Math.Max(0, Math.Min(100, volume));
                _mediaPlayer.Volume = volume;

                // 更新IsUserInitiated
                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                    viewModel.IsUserInitiated = true;
                }
            }
        }

        // 设置进度
        public void SetPosition(double position)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Time = (long)position;

                // 更新IsUserInitiated
                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                    viewModel.IsUserInitiated = true;
                }
            }
        }

        // 设置播放速度
        public void SetPlaybackRate(float rate)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SetRate(rate);

                // 更新IsUserInitiated
                var window = Window.GetWindow(this);
                if (window?.DataContext is Player.ViewModel.PlayerViewModel viewModel)
                {
                    viewModel.IsUserInitiated = true;
                }
            }
        }

        // 格式化时间
        private string FormatTime(long milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            if (timeSpan.Hours > 0)
                return timeSpan.ToString(@"h\:mm\:ss");
            else
                return timeSpan.ToString(@"m\:ss");
        }

        // 提供IsPlaying属性供外部访问
        public bool IsPlaying
        {
            get { return _mediaPlayer?.IsPlaying ?? false; }
        }
        
        // 提供当前播放时间（毫秒）
        public long CurrentTime
        {
            get { return _mediaPlayer?.Time ?? 0; }
        }
        
        // 提供总时长（毫秒）
        public long Length
        {
            get { return _mediaPlayer?.Length ?? 0; }
        }
        
        // 提供当前播放的视频路径
        public string CurrentVideoPath
        {
            get { return _currentVideoPath; }
        }
        
        // 方案5：监听大小变更，重置MediaPlayer（终极手段）
        private Size _lastSize;
        
        /// <summary>
        /// 处理VideoView尺寸变化事件
        /// </summary>
        private void HandleVideoViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_mediaPlayer != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                try
                {
                    // 显示过渡覆盖层防止白屏
                    TransitionOverlay.Visibility = Visibility.Visible;
                    
                    // 使用暂停/播放的方式强制VLC刷新画面
                    RefreshMediaPlayer();
                    
                    // 延迟隐藏覆盖层，确保渲染完成
                    Dispatcher.BeginInvoke(DispatcherPriority.Render, async () => 
                    {
                        await Task.Delay(50);
                        TransitionOverlay.Visibility = Visibility.Collapsed;
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"更新VLC输出尺寸失败: {ex.Message}");
                    // 确保覆盖层最终会被隐藏
                    TransitionOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        /// <summary>
        /// 处理控件尺寸变化
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 防止频繁触发
            if (_lastSize.Width == e.NewSize.Width && _lastSize.Height == e.NewSize.Height)
            {
                return;
            }
            
            _lastSize = e.NewSize;
            
            if (_mediaPlayer != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                HandleSizeChangedWithTransition();
            }
        }
        
        /// <summary>
        /// 使用过渡覆盖层处理尺寸变化
        /// </summary>
        private void HandleSizeChangedWithTransition()
        {
            TransitionOverlay.Visibility = Visibility.Visible;
            
            try
            {
                // 刷新媒体播放器
                RefreshMediaPlayer();
                
                // 延迟隐藏覆盖层，确保渲染完成
                Dispatcher.BeginInvoke(DispatcherPriority.Render, async () =>
                {
                    await Task.Delay(50);
                    TransitionOverlay.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新VLC输出尺寸失败: {ex.Message}");
                // 确保覆盖层最终会被隐藏
                TransitionOverlay.Visibility = Visibility.Collapsed;
            }
        }
        



        /// <summary>
        /// 切换全屏模式
        /// </summary>
        public void ToggleFullscreen()
        {
            if (_isVideoFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                EnterFullscreen();
            }
        }

        /// <summary>
        /// 进入全屏模式
        /// </summary>
        private void EnterFullscreen()
        {
            try
            {
                // 检查是否有媒体在播放
                if (string.IsNullOrEmpty(_currentVideoPath) || _mediaPlayer == null || _libVlc == null)
                {
                    SystemNotificationHelper.ShowWarning ("没有可播放的媒体，无法进入全屏");
                    return;
                }

                // 保存当前播放状态
                bool wasPlaying = _mediaPlayer.IsPlaying;
                long playbackTime = _mediaPlayer.Time;
                
                // 注意：不要暂停播放器，全屏窗口会接管控制
                // 全屏窗口会处理播放状态的恢复
                
                // 创建全屏窗口
                _fullscreenWindow = new FullscreenWindow(this, _currentVideoPath, playbackTime, wasPlaying, _libVlc);
                
                // 设置全屏标志
                _isVideoFullscreen = true;
                
                // 显示全屏窗口
                _fullscreenWindow.Show();
                
                // 订阅全屏窗口关闭事件
                _fullscreenWindow.Closed += FullscreenWindow_Closed;

               
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"进入全屏模式失败: {ex.Message}");
                _isVideoFullscreen = false;
            }
        }

        /// <summary>
        /// 退出全屏模式
        /// </summary>
        private void ExitFullscreen()
        {
            try
            {
                if (_fullscreenWindow != null)
                {
                    // 关闭全屏窗口
                    _fullscreenWindow.Close();
                    _fullscreenWindow = null!;
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"退出全屏模式失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 全屏窗口关闭事件处理
        /// </summary>
        private void FullscreenWindow_Closed(object? sender, EventArgs e)
        {
            try
            {
                // 如果是从关闭事件触发的，则更新状态
                if (_isVideoFullscreen)
                {
                    _isVideoFullscreen = false;
                    _fullscreenWindow = null!;
                    
                    // 重新播放当前视频（如果有）
                    RestorePlaybackAfterFullscreen();

                    // 全屏模式切换为常见操作，无需通知用户
                    System.Diagnostics.Debug.WriteLine("已退出全屏模式");
                }
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"处理全屏窗口关闭事件失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 从全屏模式退出后恢复播放
        /// </summary>
        internal void RestorePlaybackAfterFullscreen()
        {
            if (!string.IsNullOrEmpty(_currentVideoPath) && _libVlc != null)
            {
                // 保存恢复前的播放状态
                long savedTime = _currentPlaybackTime;
                bool savedPlayingState = _wasPlaying;
                
                // 重新创建MediaPlayer实例，因为全屏窗口使用了独立的MediaPlayer
                DisposeMediaPlayer();
                _mediaPlayer = new MediaPlayer(_libVlc);
                
                // 重新创建VideoView控件
                var videoViewControl = new LibVLCSharp.WPF.VideoView
                {
                    MediaPlayer = _mediaPlayer,
                    Background = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                
                // 添加SizeChanged事件监听
                videoViewControl.SizeChanged += HandleVideoViewSizeChanged;
                
                // 替换UI
                PlayerHost.Content = videoViewControl;
                
                // 重新加载视频
                var media = new Media(_libVlc, new Uri(_currentVideoPath));
                _mediaPlayer.Play(media);
                
                // 延迟设置精确的播放位置，确保媒体已加载
                Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                {
                    try
                    {
                        if (_mediaPlayer != null)
                        {
                            // 恢复播放位置
                            if (savedTime > 0)
                            {
                                _mediaPlayer.Time = savedTime;
                            }
                            
                            // 恢复播放状态
                            if (!savedPlayingState)
                            {
                                _mediaPlayer.Pause();
                            }
                            
                            // 强制刷新视频显示
                            videoViewControl.InvalidateVisual();
                            videoViewControl.UpdateLayout();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"恢复播放状态失败: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// 获取当前是否为全屏模式
        /// </summary>
        public bool IsFullscreen
        {
            get { return _isVideoFullscreen; }
        }

        /// <summary>
        /// 启用/禁用 GPU 硬件加速
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetGpuAcceleration(bool enabled)
        {
            // 获取硬件设置
            var hardwareSettings = ConfigManager.LoadHardwareSettings();
            
            // 若状态未变，无需重新初始化
            if (hardwareSettings.EnableGPUAcceleration == enabled) return;

            // 更新配置
            hardwareSettings.EnableGPUAcceleration = enabled;
            ConfigManager.SaveHardwareSettings(hardwareSettings);
            
            // 应用新设置并重启VLC
            ReinitializeVlcWithSettings();

           
        }

        /// <summary>
        /// 启用/禁用视频超分辨率
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetSuperResolution(bool enabled)
        {
            // 获取硬件设置
            var hardwareSettings = ConfigManager.LoadHardwareSettings();
            
            // 若状态未变，无需重新初始化
            if (hardwareSettings.EnableSuperResolution == enabled) return;

            // 更新配置
            hardwareSettings.EnableSuperResolution = enabled;
            ConfigManager.SaveHardwareSettings(hardwareSettings);
            
            // 应用新设置并重启VLC
            ReinitializeVlcWithSettings();
        }
        
        /// <summary>
        /// 应用新设置并重新初始化VLC
        /// </summary>
        private void ReinitializeVlcWithSettings()
        {            
            // 保存当前播放状态
            SavePlaybackState();
            
            // 保存全屏状态
            bool wasFullscreen = _isVideoFullscreen;
            
            // 如果在全屏模式，先退出
            if (wasFullscreen)
            {
                ExitFullscreen();
            }

            // 重新初始化 VLC 实例（应用新配置）
            InitializeVlc();
            
            // 恢复全屏状态
            if (wasFullscreen)
            {
                // 延迟恢复全屏，确保VLC实例完全初始化
                Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                {
                    EnterFullscreen();
                });
            }

             
        }


        /// <summary>
        /// 更新播放状态到UI，确保播放/暂停图标正确显示
        /// </summary>
        /// <param name="isPlaying">是否正在播放</param>
        private void UpdatePlaybackStateToUI(bool isPlaying)
        {
            try
            {
                // 通过主窗口查找BottomControl并更新播放图标
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    var bottomControl = VisualTreeHelperExtensions.FindVisualChild<Player.Bottom.BottomControl>(mainWindow);
                    if (bottomControl != null)
                    {
                        bottomControl.UpdatePlayIcon(isPlaying);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新播放状态到UI失败: {ex.Message}");
            }
        }

    }
}