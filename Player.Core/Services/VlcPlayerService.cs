using System;
using System.Collections.Generic;
using System.Windows;
using LibVLCSharp.Shared;
using Player.Core.Models;

namespace Player.Core.Services
{
    /// <summary>
    /// VLC播放器服务实现
    /// </summary>
    public class VlcPlayerService : IVlcPlayerService
    {
        private LibVLC? _libVlc;
        private MediaPlayer? _mediaPlayer;
        private bool _disposed = false;
        private readonly INotificationService? _notificationService;

        public bool IsInitialized { get; private set; }
        public MediaPlayer? MediaPlayer => _mediaPlayer;

        public VlcPlayerService(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        // 事件定义
        public event EventHandler? Playing;
        public event EventHandler? Paused;
        public event EventHandler? Stopped;
        public event EventHandler? EndReached;
        public event EventHandler<MediaPlayerTimeChangedEventArgs>? TimeChanged;
        public event EventHandler<MediaPlayerLengthChangedEventArgs>? LengthChanged;
        public event EventHandler<MediaPlayerVolumeChangedEventArgs>? VolumeChanged;

        public void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                // 从配置文件中加载硬件设置
                var hardwareSettings = ConfigManager.LoadHardwareSettings();

                // 创建LibVLC实例，应用所有配置项
                var libVlcOptions = new List<string>
{
    $"--file-caching={hardwareSettings.FileCaching}",
    $"--network-caching={hardwareSettings.NetworkCaching}",
    "--live-caching=1000",
    "--no-video-title-show",
    "--no-osd",
    "--no-fullscreen"
    
};

                // GPU 加速（谨慎使用，确保 VideoRenderer 值合法）
                if (hardwareSettings.EnableGPUAcceleration)
                {
                    if (!string.IsNullOrEmpty(hardwareSettings.VideoRenderer))
                    {
                        // 合法值示例： s: "dxva2", "d3d11va" 
                        libVlcOptions.Add($"--avcodec-hw={hardwareSettings.VideoRenderer}");
                    }
 
                }

                // Deinterlace（有效）
                if (hardwareSettings.EnableDeinterlace && !string.IsNullOrEmpty(hardwareSettings.DeinterlaceMode))
                {
                    libVlcOptions.Add($"--deinterlace={hardwareSettings.DeinterlaceMode}");
                }

                // 线程与快速解码（有效）
                if (hardwareSettings.AutoThreads)
                {
                    libVlcOptions.Add("--avcodec-threads=0");
                }

                if (hardwareSettings.EnableFastDecoding)
                {
                    libVlcOptions.Add("--avcodec-fast");
                }

                if (hardwareSettings.EnableHurryUpDecoding)
                {
                    libVlcOptions.Add("--avcodec-hurry-up");
                }

                if (hardwareSettings.NoDropLateFrames)
                {
                    libVlcOptions.Add("--no-drop-late-frames");
                }

                if (hardwareSettings.NoSkipFrames)
                {
                    libVlcOptions.Add("--no-skip-frames");
                }

                // 仅在无 GPU 时启用：
                if (hardwareSettings.SkipLoopFilter && !hardwareSettings.EnableGPUAcceleration)
                {
                    libVlcOptions.Add("--avcodec-skip-loop-filter=all");
                }

                _libVlc = new LibVLC(libVlcOptions.ToArray());
                
                // 创建MediaPlayer实例
                _mediaPlayer = new MediaPlayer(_libVlc);
                
                // 订阅MediaPlayer事件
                _mediaPlayer.Playing += OnMediaPlayerPlaying;
                _mediaPlayer.Paused += OnMediaPlayerPaused;
                _mediaPlayer.Stopped += OnMediaPlayerStopped;
                _mediaPlayer.EndReached += OnMediaPlayerEndReached;
                _mediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
                _mediaPlayer.LengthChanged += OnMediaPlayerLengthChanged;
                _mediaPlayer.VolumeChanged += OnMediaPlayerVolumeChanged;
                
                IsInitialized = true;
                System.Diagnostics.Debug.WriteLine("VLC播放器初始化成功");
                _notificationService?.ShowSuccess("VLC播放器初始化成功");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VLC初始化失败: {ex.Message}");
                _notificationService?.ShowError($"VLC播放器初始化失败: {ex.Message}");
                
                // 尝试使用更简单的配置
                try
                {
                // 使用基本配置作为备选方案
                var basicOptions = new List<string>
                {
                    "--file-caching=1000",
                    "--network-caching=1000",
                    "--live-caching=1000",
                    // UI相关参数
                    "--no-video-title-show",
                    "--no-osd",
                    "--no-fullscreen"
                };
                    
                    _libVlc = new LibVLC(basicOptions.ToArray());
                    _mediaPlayer = new MediaPlayer(_libVlc);
                    
                    // 订阅MediaPlayer事件
                    _mediaPlayer.Playing += OnMediaPlayerPlaying;
                    _mediaPlayer.Paused += OnMediaPlayerPaused;
                    _mediaPlayer.Stopped += OnMediaPlayerStopped;
                    _mediaPlayer.EndReached += OnMediaPlayerEndReached;
                    _mediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
                    _mediaPlayer.LengthChanged += OnMediaPlayerLengthChanged;
                    _mediaPlayer.VolumeChanged += OnMediaPlayerVolumeChanged;
                    
                    IsInitialized = true;
                    System.Diagnostics.Debug.WriteLine("VLC播放器使用基本配置初始化成功");
                    _notificationService?.ShowInfo("VLC播放器使用基本配置初始化成功");
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"VLC基本配置初始化也失败: {ex2.Message}");
                    _notificationService?.ShowError($"VLC播放器完全初始化失败: {ex2.Message}");
                    throw;
                }
            }
        }

        public void LoadMedia(string filePath)
        {
            // 添加更多的错误处理和日志
            if (!IsInitialized)
            {
                System.Diagnostics.Debug.WriteLine("VlcPlayerService.LoadMedia: VLC播放器未初始化");
                return;
            }

            if (_mediaPlayer == null)
            {
                System.Diagnostics.Debug.WriteLine("VlcPlayerService.LoadMedia: 媒体播放器实例为null");
                return;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                System.Diagnostics.Debug.WriteLine("VlcPlayerService.LoadMedia: 文件路径为空");
                return;
            }

            try
            {
                // 验证文件是否存在
                if (!System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"VlcPlayerService.LoadMedia: 文件不存在: {filePath}");
                    return;
                }

                // 先停止当前播放
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                
                // 创建新的媒体对象
                System.Diagnostics.Debug.WriteLine($"VlcPlayerService.LoadMedia: 正在加载媒体: {filePath}");
                var media = new Media(_libVlc, new Uri(filePath));
                
                // 确保媒体对象创建成功
                if (media == null)
                {
                    System.Diagnostics.Debug.WriteLine("VlcPlayerService.LoadMedia: 无法创建媒体对象");
                    return;
                }
                
                // 设置媒体
                _mediaPlayer.Media = media;
                
                // 直接播放媒体
                System.Diagnostics.Debug.WriteLine("VlcPlayerService.LoadMedia: 开始播放媒体");
                _mediaPlayer.Play();
                
                // 立即检查播放状态
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                {
                    if (_mediaPlayer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"VlcPlayerService.LoadMedia: 播放状态检查 - 正在播放: {_mediaPlayer.IsPlaying}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VlcPlayerService.LoadMedia: 加载媒体失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"异常详情: {ex.StackTrace}");
                // 不抛出异常，避免应用崩溃
            }
        }

        public void TogglePlayPause()
        {
            if (_mediaPlayer == null) return;

            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Pause();
            else
                _mediaPlayer.Play();
        }

        public void Stop()
        {
            _mediaPlayer?.Stop();
        }

        public void Seek(TimeSpan time)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Time = Convert.ToInt64(time.TotalMilliseconds);
        }

        public void SetVolume(int volume)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = Math.Clamp(volume, 0, 100);
            }
        }

        public void SetMute(bool muted)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Mute = muted;
            }
        }

        public void SetPlaybackSpeed(float speed)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SetRate(speed);
            }
        }



        /// <summary>
        /// 设置MediaPlayer实例（用于全屏传输）
        /// </summary>
        /// <param name="mediaPlayer">MediaPlayer实例</param>
        public void SetMediaPlayer(MediaPlayer mediaPlayer)
        {
            try
            {
                if (mediaPlayer == null)
                {
                    System.Diagnostics.Debug.WriteLine("SetMediaPlayer: MediaPlayer is null");
                    return;
                }
                
                // 先清理当前的MediaPlayer，但不要Dispose，因为它可能在其他地方使用
                if (_mediaPlayer != null && _mediaPlayer != mediaPlayer)
                {
                    // 停止播放
                    try
                    {
                        _mediaPlayer.Stop();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping media player: {ex.Message}");
                    }
                    
                    // 取消事件订阅
                    _mediaPlayer.Playing -= OnMediaPlayerPlaying;
                    _mediaPlayer.Paused -= OnMediaPlayerPaused;
                    _mediaPlayer.Stopped -= OnMediaPlayerStopped;
                    _mediaPlayer.EndReached -= OnMediaPlayerEndReached;
                    _mediaPlayer.TimeChanged -= OnMediaPlayerTimeChanged;
                    _mediaPlayer.LengthChanged -= OnMediaPlayerLengthChanged;
                    _mediaPlayer.VolumeChanged -= OnMediaPlayerVolumeChanged;
                }
                
                // 设置新的MediaPlayer
                _mediaPlayer = mediaPlayer;

                // 重新订阅事件
                _mediaPlayer.Playing += OnMediaPlayerPlaying;
                _mediaPlayer.Paused += OnMediaPlayerPaused;
                _mediaPlayer.Stopped += OnMediaPlayerStopped;
                _mediaPlayer.EndReached += OnMediaPlayerEndReached;
                _mediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
                _mediaPlayer.LengthChanged += OnMediaPlayerLengthChanged;
                _mediaPlayer.VolumeChanged += OnMediaPlayerVolumeChanged;
                
                System.Diagnostics.Debug.WriteLine("SetMediaPlayer: MediaPlayer successfully set and events subscribed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SetMediaPlayer: {ex.Message}");
                // 确保状态一致性
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Playing -= OnMediaPlayerPlaying;
                    _mediaPlayer.Paused -= OnMediaPlayerPaused;
                    _mediaPlayer.Stopped -= OnMediaPlayerStopped;
                    _mediaPlayer.EndReached -= OnMediaPlayerEndReached;
                    _mediaPlayer.TimeChanged -= OnMediaPlayerTimeChanged;
                    _mediaPlayer.LengthChanged -= OnMediaPlayerLengthChanged;
                    _mediaPlayer.VolumeChanged -= OnMediaPlayerVolumeChanged;
                }
                throw; // 重新抛出异常以便上层处理
            }
        }

        #region 事件处理器

        private void OnMediaPlayerPlaying(object? sender, EventArgs e)
        {
            Playing?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaPlayerPaused(object? sender, EventArgs e)
        {
            Paused?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaPlayerStopped(object? sender, EventArgs e)
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaPlayerEndReached(object? sender, EventArgs e)
        {
            EndReached?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaPlayerTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            TimeChanged?.Invoke(this, e);
        }

        private void OnMediaPlayerLengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
        {
            LengthChanged?.Invoke(this, e);
        }

        private void OnMediaPlayerVolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
        {
            VolumeChanged?.Invoke(this, e);
        }



        #endregion

        #region Dispose模式

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
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.Stop();
                        _mediaPlayer.Playing -= OnMediaPlayerPlaying;
                        _mediaPlayer.Paused -= OnMediaPlayerPaused;
                        _mediaPlayer.Stopped -= OnMediaPlayerStopped;
                        _mediaPlayer.EndReached -= OnMediaPlayerEndReached;
                        _mediaPlayer.TimeChanged -= OnMediaPlayerTimeChanged;
                        _mediaPlayer.LengthChanged -= OnMediaPlayerLengthChanged;
                        _mediaPlayer.VolumeChanged -= OnMediaPlayerVolumeChanged;
                        _mediaPlayer.Dispose();
                    }

                    if (_libVlc != null)
                    {
                        _libVlc.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~VlcPlayerService()
        {
            Dispose(false);
        }

        #endregion
    }
}