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
        
        /// <summary>
        /// 播放速率（1.0为正常速度）
        /// </summary>
        public float PlaybackRate { get; set; } = 1.0f;
        
        /// <summary>
        /// 音量（0-100）
        /// </summary>
        public int Volume { get; set; } = 80;
        
        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMuted { get; set; } = false;
        
        /// <summary>
        /// 静音前的音量
        /// </summary>
        public int VolumeBeforeMute { get; set; } = 80;
        
        /// <summary>
        /// 是否循环播放
        /// </summary>
        public bool IsLooping { get; set; } = false;
        
        /// <summary>
        /// 媒体文件大小（字节）
        /// </summary>
        public long FileSize { get; set; } = 0;
        
        /// <summary>
        /// 媒体格式（如mp4, avi等）
        /// </summary>
        public string MediaFormat { get; set; } = string.Empty;
        
        /// <summary>
        /// 媒体编码信息
        /// </summary>
        public string MediaCodec { get; set; } = string.Empty;
        
        /// <summary>
        /// 视频分辨率宽度
        /// </summary>
        public int VideoWidth { get; set; } = 0;
        
        /// <summary>
        /// 视频分辨率高度
        /// </summary>
        public int VideoHeight { get; set; } = 0;
        
        /// <summary>
        /// 视频帧率
        /// </summary>
        public double FrameRate { get; set; } = 0;
        
        /// <summary>
        /// 音频采样率
        /// </summary>
        public int AudioSampleRate { get; set; } = 0;
        
        /// <summary>
        /// 音频通道数
        /// </summary>
        public int AudioChannels { get; set; } = 0;
        
        /// <summary>
        /// 媒体创建时间
        /// </summary>
        public DateTime MediaCreationTime { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// 最后播放时间
        /// </summary>
        public DateTime LastPlayedTime { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// 播放次数
        /// </summary>
        public int PlayCount { get; set; } = 0;
        
        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool HardwareAccelerationEnabled { get; set; } = true;
        
        /// <summary>
        /// 是否启用超分辨率
        /// </summary>
        public bool SuperResolutionEnabled { get; set; } = false;
        
        /// <summary>
        /// 播放器窗口状态（正常、最大化、全屏）
        /// </summary>
        public string WindowState { get; set; } = "Normal";
        
        /// <summary>
        /// 播放器窗口位置X坐标
        /// </summary>
        public double WindowPositionX { get; set; } = 0;
        
        /// <summary>
        /// 播放器窗口位置Y坐标
        /// </summary>
        public double WindowPositionY { get; set; } = 0;
        
        /// <summary>
        /// 播放器窗口宽度
        /// </summary>
        public double WindowWidth { get; set; } = 800;
        
        /// <summary>
        /// 播放器窗口高度
        /// </summary>
        public double WindowHeight { get; set; } = 600;
        
        /// <summary>
        /// 播放器主题设置
        /// </summary>
        public string Theme { get; set; } = "Dark";
        
        /// <summary>
        /// 字幕文件路径
        /// </summary>
        public string SubtitlePath { get; set; } = string.Empty;
        
        /// <summary>
        /// 字幕是否启用
        /// </summary>
        public bool SubtitleEnabled { get; set; } = false;
        
        /// <summary>
        /// 字幕延迟（毫秒）
        /// </summary>
        public int SubtitleDelay { get; set; } = 0;
        
        /// <summary>
        /// 字幕编码
        /// </summary>
        public string SubtitleEncoding { get; set; } = "UTF-8";
        
        /// <summary>
        /// 字幕字体大小
        /// </summary>
        public int SubtitleFontSize { get; set; } = 16;
        
        /// <summary>
        /// 字幕字体颜色
        /// </summary>
        public string SubtitleFontColor { get; set; } = "#FFFFFF";
        
        /// <summary>
        /// 字幕背景颜色
        /// </summary>
        public string SubtitleBackgroundColor { get; set; } = "#00000080";
        
        /// <summary>
        /// 播放器配置版本
        /// </summary>
        public string ConfigVersion { get; set; } = "1.0";
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 创建播放状态实例
        /// </summary>
        public PlaybackState() { }
        
        /// <summary>
        /// 创建播放状态实例
        /// </summary>
        public PlaybackState(string mediaPath, long playbackTime = 0, bool isPlaying = false)
        {
            MediaPath = mediaPath;
            PlaybackTime = playbackTime;
            IsPlaying = isPlaying;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 更新播放时间
        /// </summary>
        public void UpdatePlaybackTime(long time)
        {
            PlaybackTime = time;
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 更新播放状态
        /// </summary>
        public void UpdatePlaybackStatus(bool playing)
        {
            IsPlaying = playing;
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 更新媒体信息
        /// </summary>
        public void UpdateMediaInfo(long totalDuration, long fileSize = 0)
        {
            TotalDuration = totalDuration;
            FileSize = fileSize;
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 重置播放状态
        /// </summary>
        public void Reset()
        {
            PlaybackTime = 0;
            IsPlaying = false;
            PlaybackRate = 1.0f;
            Volume = 80;
            IsMuted = false;
            VolumeBeforeMute = 80;
            IsLooping = false;
            SubtitleEnabled = false;
            SubtitleDelay = 0;
            UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// 复制播放状态
        /// </summary>
        public PlaybackState Clone()
        {
            return new PlaybackState
            {
                PlaybackTime = this.PlaybackTime,
                IsPlaying = this.IsPlaying,
                MediaPath = this.MediaPath,
                TotalDuration = this.TotalDuration,
                PlaybackRate = this.PlaybackRate,
                Volume = this.Volume,
                IsMuted = this.IsMuted,
                VolumeBeforeMute = this.VolumeBeforeMute,
                IsLooping = this.IsLooping,
                FileSize = this.FileSize,
                MediaFormat = this.MediaFormat,
                MediaCodec = this.MediaCodec,
                VideoWidth = this.VideoWidth,
                VideoHeight = this.VideoHeight,
                FrameRate = this.FrameRate,
                AudioSampleRate = this.AudioSampleRate,
                AudioChannels = this.AudioChannels,
                MediaCreationTime = this.MediaCreationTime,
                LastPlayedTime = this.LastPlayedTime,
                PlayCount = this.PlayCount,
                HardwareAccelerationEnabled = this.HardwareAccelerationEnabled,
                SuperResolutionEnabled = this.SuperResolutionEnabled,
                WindowState = this.WindowState,
                WindowPositionX = this.WindowPositionX,
                WindowPositionY = this.WindowPositionY,
                WindowWidth = this.WindowWidth,
                WindowHeight = this.WindowHeight,
                Theme = this.Theme,
                SubtitlePath = this.SubtitlePath,
                SubtitleEnabled = this.SubtitleEnabled,
                SubtitleDelay = this.SubtitleDelay,
                SubtitleEncoding = this.SubtitleEncoding,
                SubtitleFontSize = this.SubtitleFontSize,
                SubtitleFontColor = this.SubtitleFontColor,
                SubtitleBackgroundColor = this.SubtitleBackgroundColor,
                ConfigVersion = this.ConfigVersion,
                CreatedAt = this.CreatedAt,
                UpdatedAt = DateTime.Now
            };
        }
    }
}