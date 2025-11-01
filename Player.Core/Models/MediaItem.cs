namespace Player.Core.Models
{
    /// <summary>
    /// 媒体项数据模型
    /// </summary>
    public class MediaItem
    {
        /// <summary>
        /// 媒体名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 媒体文件路径
        /// </summary>
        public string Path { get; set; } = string.Empty;
    }
}