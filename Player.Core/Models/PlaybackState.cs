namespace Player.Core.Models
{
    /// <summary>
    /// 播放状态数据模型
    /// </summary>
    public class PlaybackState
    {
        /// <summary>
        /// 播放时间（毫秒）
        /// </summary>
        public long PlaybackTime { get; set; } = 0;
        
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying { get; set; } = false;
        
        /// <summary>
        /// 媒体文件路径
        /// </summary>
        public string MediaPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 媒体总时长（毫秒）
        /// </summary>
        public long TotalDuration { get; set; } = 0;
    }
}