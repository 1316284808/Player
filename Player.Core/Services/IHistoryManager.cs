using System;
using System.Collections.Generic;

namespace Player.Core.Services
{
    /// <summary>
    /// 历史记录管理器接口
    /// </summary>
    public interface IHistoryManager
    {
        /// <summary>
        /// 加载历史记录
        /// </summary>
        void LoadHistory();
        
        /// <summary>
        /// 保存历史记录
        /// </summary>
        void SaveHistory();
        
        /// <summary>
        /// 获取所有历史记录日期
        /// </summary>
        /// <returns>日期集合</returns>
        List<DateTime> GetAllDates();
        
        /// <summary>
        /// 根据日期获取文件列表
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>文件路径集合</returns>
        List<string> GetFilesByDate(DateTime date);
        
        /// <summary>
        /// 添加历史记录项
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="playTime">播放时间</param>
        void AddHistoryItem(string filePath, DateTime playTime);
        
        /// <summary>
        /// 清除指定日期的历史记录
        /// </summary>
        /// <param name="date">目标日期</param>
        void ClearHistoryByDate(DateTime date);
        
        /// <summary>
        /// 清除所有历史记录
        /// </summary>
        void ClearAllHistory();
    }
}