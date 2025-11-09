using System;
using LibVLCSharp.Shared;
using Player.Core.Events;

namespace Player.Core.Services
{
    /// <summary>
    /// Mock VLC播放器服务 - 用于单元测试
    /// </summary>
    public class MockVlcPlayerService : IVlcPlayerService
    {
        private bool _isPlaying = false;
        private int _volume = 50;
        private bool _isMuted = false;
        private TimeSpan _currentTime = TimeSpan.Zero;
        private TimeSpan _totalDuration = TimeSpan.FromMinutes(120);

        public bool IsInitialized { get; private set; }
        public MediaPlayer? MediaPlayer => null; // Mock实现不返回真实MediaPlayer

        // 事件 - 使用自定义事件参数替代LibVLCSharp的事件参数
        public event EventHandler? Playing;
        public event EventHandler? Paused;
        public event EventHandler? Stopped;
        public event EventHandler? EndReached;
        public event EventHandler<MockMediaPlayerTimeChangedEventArgs>? TimeChanged;
        public event EventHandler<MockMediaPlayerLengthChangedEventArgs>? LengthChanged;
        public event EventHandler<MockMediaPlayerVolumeChangedEventArgs>? VolumeChanged;

        // 显式实现接口事件（为了兼容性）
        event EventHandler<MediaPlayerTimeChangedEventArgs>? IVlcPlayerService.TimeChanged
        {
            add { }
            remove { }
        }
        
        event EventHandler<MediaPlayerLengthChangedEventArgs>? IVlcPlayerService.LengthChanged
        {
            add { }
            remove { }
        }
        
        event EventHandler<MediaPlayerVolumeChangedEventArgs>? IVlcPlayerService.VolumeChanged
        {
            add { }
            remove { }
        }
        


        public void Initialize()
        {
            IsInitialized = true;
            // 触发长度变更事件
            LengthChanged?.Invoke(this, new MockMediaPlayerLengthChangedEventArgs((long)_totalDuration.TotalMilliseconds));
        }

        public void LoadMedia(string filePath)
        {
            if (!IsInitialized) return;
            
            // 模拟加载媒体
            _isPlaying = true;
            _currentTime = TimeSpan.Zero;
            
            Playing?.Invoke(this, EventArgs.Empty);
            
            // 模拟时间更新
            SimulateTimeUpdate();
        }

        public void TogglePlayPause()
        {
            if (!IsInitialized) return;
            
            _isPlaying = !_isPlaying;
            if (_isPlaying)
            {
                Playing?.Invoke(this, EventArgs.Empty);
                SimulateTimeUpdate();
            }
            else
            {
                Paused?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (!IsInitialized) return;
            
            _isPlaying = false;
            _currentTime = TimeSpan.Zero;
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Seek(TimeSpan time)
        {
            if (!IsInitialized) return;
            
            _currentTime = time;
            TimeChanged?.Invoke(this, new MockMediaPlayerTimeChangedEventArgs((long)_currentTime.TotalMilliseconds));
        }

        public void SetVolume(int volume)
        {
            _volume = Math.Clamp(volume, 0, 100);
            VolumeChanged?.Invoke(this, new MockMediaPlayerVolumeChangedEventArgs(_volume));
        }

        public void SetMute(bool muted)
        {
            _isMuted = muted;
            VolumeChanged?.Invoke(this, new MockMediaPlayerVolumeChangedEventArgs(_isMuted ? 0 : _volume));
        }

        public void SetPlaybackSpeed(float speed)
        {
            // Mock实现，不处理实际速度
        }
        


        /// <summary>
        /// 设置MediaPlayer实例（用于全屏传输）
        /// </summary>
        /// <param name="mediaPlayer">MediaPlayer实例</param>
        public void SetMediaPlayer(MediaPlayer mediaPlayer)
        {
            // Mock实现，不处理实际的MediaPlayer传输
            // 在真实环境中，这里会处理MediaPlayer的传输逻辑
            System.Diagnostics.Debug.WriteLine("MockVlcPlayerService: SetMediaPlayer called (Mock implementation)");
        }

        private async void SimulateTimeUpdate()
        {
            while (_isPlaying && _currentTime < _totalDuration)
            {
                await Task.Delay(1000); // 每秒更新一次
                _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));
                TimeChanged?.Invoke(this, new MockMediaPlayerTimeChangedEventArgs((long)_currentTime.TotalMilliseconds));
                
                if (_currentTime >= _totalDuration)
                {
                    EndReached?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }

        public void Dispose()
        {
            // Mock清理逻辑
            _isPlaying = false;
        }
    }
}