using System;
using LibVLCSharp.Shared;

namespace Player.Core.Services
{
    /// <summary>
    /// VLC播放器服务接口 - 增强可测试性
    /// </summary>
    public interface IVlcPlayerService : IDisposable
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// MediaPlayer实例
        /// </summary>
        MediaPlayer? MediaPlayer { get; }



        /// <summary>
        /// 初始化VLC播放器
        /// </summary>
        void Initialize();

        /// <summary>
        /// 加载媒体文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        void LoadMedia(string filePath);

        /// <summary>
        /// 切换播放/暂停
        /// </summary>
        void TogglePlayPause();

        /// <summary>
        /// 停止播放
        /// </summary>
        void Stop();

        /// <summary>
        /// 跳转到指定时间
        /// </summary>
        /// <param name="time">时间</param>
        void Seek(TimeSpan time);

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume">音量值 (0-100)</param>
        void SetVolume(int volume);

        /// <summary>
        /// 设置静音
        /// </summary>
        /// <param name="muted">是否静音</param>
        void SetMute(bool muted);

        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="speed">播放速度</param>
        void SetPlaybackSpeed(float speed);

        /// <summary>
        /// 设置MediaPlayer实例（用于全屏传输）
        /// </summary>
        /// <param name="mediaPlayer">MediaPlayer实例</param>
        void SetMediaPlayer(MediaPlayer mediaPlayer);



        // 事件定义
        event EventHandler? Playing;
        event EventHandler? Paused;
        event EventHandler? Stopped;
        event EventHandler? EndReached;
        event EventHandler<MediaPlayerTimeChangedEventArgs>? TimeChanged;
        event EventHandler<MediaPlayerLengthChangedEventArgs>? LengthChanged;
        event EventHandler<MediaPlayerVolumeChangedEventArgs>? VolumeChanged;
    }
}