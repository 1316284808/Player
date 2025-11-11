using System.Collections.Generic;
using System.Threading.Tasks;
using Player.Core.Models;

namespace Player.Core.Repositories
{
    /// <summary>
    /// 媒体数据访问接口
    /// 
    /// 重构说明：
    /// 1. 引入Repository模式，分离数据访问逻辑
    /// 2. 提供统一的媒体数据访问接口
    /// 3. 支持异步操作
    /// </summary>
    public interface IMediaRepository
    {
        /// <summary>
        /// 获取指定目录下的媒体文件列表
        /// </summary>
        /// <param name="directory">目录路径</param>
        /// <returns>媒体项列表</returns>
        Task<IEnumerable<MediaItem>> GetMediaItemsAsync(string directory);
        
        /// <summary>
        /// 获取媒体项详细信息
        /// </summary>
        /// <param name="path">媒体文件路径</param>
        /// <returns>媒体项详细信息</returns>
        Task<MediaItem> GetMediaItemDetailsAsync(string path);
        
        /// <summary>
        /// 保存播放历史记录
        /// </summary>
        /// <param name="items">媒体项列表</param>
        /// <returns>保存任务</returns>
        Task SaveMediaHistoryAsync(IEnumerable<MediaItem> items);
        
        /// <summary>
        /// 加载播放历史记录
        /// </summary>
        /// <returns>历史媒体项列表</returns>
        Task<IEnumerable<MediaItem>> LoadMediaHistoryAsync();
        
        /// <summary>
        /// 清空播放历史记录
        /// </summary>
        /// <returns>清空任务</returns>
        Task ClearMediaHistoryAsync();
        
        /// <summary>
        /// 获取所有历史记录日期
        /// </summary>
        /// <returns>历史日期列表</returns>
        Task<IEnumerable<DateTime>> GetAllHistoryDatesAsync();
        
        /// <summary>
        /// 根据日期获取媒体历史记录
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>媒体项列表</returns>
        Task<IEnumerable<MediaItem>> GetMediaHistoryByDateAsync(DateTime date);
        
        /// <summary>
        /// 删除单个历史记录项
        /// </summary>
        /// <param name="date">历史记录日期</param>
        /// <param name="filePath">要删除的文件路径</param>
        /// <returns>删除任务</returns>
        Task RemoveHistoryItemAsync(DateTime date, string filePath);
        
        /// <summary>
        /// 清除指定日期的所有历史记录
        /// </summary>
        /// <param name="date">历史记录日期</param>
        /// <returns>清除任务</returns>
        Task ClearHistoryByDateAsync(DateTime date);
        
        /// <summary>
        /// 获取支持的媒体文件扩展名
        /// </summary>
        /// <returns>支持的扩展名列表</returns>
        IEnumerable<string> GetSupportedExtensions();
        
        /// <summary>
        /// 检查文件是否为支持的媒体格式
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否支持</returns>
        bool IsSupportedMediaFile(string filePath);
    }
}