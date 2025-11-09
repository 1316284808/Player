using System;

namespace Player.Core.Models
{
    /// <summary>
    /// 媒体项数据模型 - 纯粹的数据容器
    /// 
    /// 重构说明：
    /// 1. 移除UI相关的命令和属性
    /// 2. 只包含纯数据属性
    /// 3. 遵循单一职责原则
    /// </summary>
    public class MediaItem
    {
        /// <summary>
        /// 媒体ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// 媒体名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 媒体文件路径
        /// </summary>
        public string Path { get; set; } = string.Empty;
        
        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; } = 0;
        
        /// <summary>
        /// 媒体时长
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// 视频宽度
        /// </summary>
        public int Width { get; set; } = 0;
        
        /// <summary>
        /// 视频高度
        /// </summary>
        public int Height { get; set; } = 0;
        
        /// <summary>
        /// 上次播放时间
        /// </summary>
        public DateTime LastPlayed { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// 上次播放位置（毫秒）
        /// </summary>
        public long LastPosition { get; set; } = 0;
        
        /// <summary>
        /// 媒体类型
        /// </summary>
        public string MediaType { get; set; } = string.Empty;
        
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string? ThumbnailPath { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public MediaItem() { }
        
        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public MediaItem(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}