using System;

namespace Player.Core.Models
{
    /// <summary>
    /// 播放状态数据模型 - 纯粹的数据容器
    /// 
    /// 重构说明：
    /// 1. 移除ObservableObject继承和ObservableProperty特性
    /// 2. 移除业务方法，只保留数据属性
    /// 3. 遵循单一职责原则
    /// </summary>
    public class PlaybackState
    {
        // 核心播放控制属性 - 纯数据属性
        public double Position { get; set; } = 0; // 播放进度（0-100）
        public int Volume { get; set; } = 80;      // 音量（0-100）
        public bool IsPlaying { get; set; } = false; // 是否正在播放
        public bool IsMuted { get; set; } = false;   // 是否静音
        public string MediaPath { get; set; } = string.Empty; // 当前播放的媒体路径
        public long TotalDuration { get; set; } = 0; // 总时长（毫秒）
        public long CurrentTime { get; set; } = 0;   // 当前播放时间（毫秒）
        public bool IsFullscreen { get; set; } = false; // 是否全屏
        public int PlaybackRate { get; set; } = 1; // 播放速度索引（0=0.5x, 1=1.0x, 2=1.5x, 3=2.0x）
        public long PlaybackTime { get; set; } = 0; // 播放时间（毫秒）

        // 辅助属性
        public string WindowTitle { get; set; } = "FUXIPlayer";
        public bool ControlsEnabled { get; set; } = false;
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PlaybackState() { }
        
        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public PlaybackState(string mediaPath, long currentTime = 0, bool isPlaying = false)
        {
            MediaPath = mediaPath;
            CurrentTime = currentTime;
            IsPlaying = isPlaying;
        }
        
        /// <summary>
        /// 重置播放状态
        /// </summary>
        public void Reset()
        {
            Position = 0;
            Volume = 80;
            IsPlaying = false;
            IsMuted = false;
            MediaPath = string.Empty;
            TotalDuration = 0;
            CurrentTime = 0;
            ControlsEnabled = false;
            WindowTitle = "媒体播放器";
        }
    }
}