using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Player.Core.Models;

namespace Player.Core.Repositories
{
    /// <summary>
    /// 媒体数据访问实现
    /// 
    /// 重构说明：
    /// 1. 实现IMediaRepository接口
    /// 2. 处理媒体文件的扫描和元数据获取
    /// 3. 管理播放历史记录的持久化
    /// </summary>
    public class MediaRepository : IMediaRepository
    {
        private readonly string _historyFilePath;
        private readonly HashSet<string> _supportedExtensions;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MediaRepository()
        {
            // 使用SettingPath中的历史记录路径
            Directory.CreateDirectory(SettingPath.JsonDirectory);
            _historyFilePath = SettingPath.HistoryPath;
            
            // 支持的媒体文件扩展名
            _supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".m4v", ".webm",
                ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma"
            };
        }

        /// <summary>
        /// 获取指定目录下的媒体文件列表
        /// </summary>
        public async Task<IEnumerable<MediaItem>> GetMediaItemsAsync(string directory)
        {
            if (!Directory.Exists(directory))
                return Enumerable.Empty<MediaItem>();

            try
            {
                var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(IsSupportedMediaFile);

                var mediaItems = new List<MediaItem>();
                
                foreach (var filePath in files)
                {
                    var mediaItem = await GetMediaItemDetailsAsync(filePath);
                    mediaItems.Add(mediaItem);
                }

                return mediaItems.OrderBy(m => m.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取媒体文件列表失败: {ex.Message}");
                return Enumerable.Empty<MediaItem>();
            }
        }

        /// <summary>
        /// 获取媒体项详细信息
        /// </summary>
        public async Task<MediaItem> GetMediaItemDetailsAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"文件不存在: {path}");

            var fileInfo = new FileInfo(path);
            
            var mediaItem = new MediaItem
            {
                Name = Path.GetFileName(path),
                Path = path,
                FileSize = fileInfo.Length,
                MediaType = GetMediaType(path),
                LastPlayed = DateTime.MinValue,
                LastPosition = 0
            };

            // 异步获取媒体时长（如果需要的话）
            await Task.Run(() =>
            {
                try
                {
                    // 这里可以集成媒体信息库来获取准确的时长
                    // 目前先使用默认值
                    mediaItem.Duration = TimeSpan.Zero;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"获取媒体时长失败: {ex.Message}");
                    mediaItem.Duration = TimeSpan.Zero;
                }
            });

            return mediaItem;
        }

        /// <summary>
        /// 保存播放历史记录 - 使用与FileHistoryManager相同的格式
        /// </summary>
        public async Task SaveMediaHistoryAsync(IEnumerable<MediaItem> items)
        {
            try
            {
                // 使用Dictionary<string, List<string>>格式，与FileHistoryManager保持一致
                Dictionary<string, List<string>> historyData = new Dictionary<string, List<string>>();
                
                // 读取现有历史记录
                if (File.Exists(_historyFilePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(_historyFilePath);
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent) ?? new Dictionary<string, List<string>>();
                    }
                }
                
                // 获取当前日期作为键
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                
                // 提取文件路径列表
                List<string> filePaths = items.Select(item => item.Path).ToList();
                
                // 更新当前日期的文件列表 - 先获取现有记录，然后合并新记录
                List<string> updatedFiles = new List<string>();
                
                if (historyData.ContainsKey(currentDate))
                {
                    // 获取现有文件列表
                    var existingFiles = historyData[currentDate];
                    
                    // 合并新旧文件，去重并保持顺序
                    // 先添加新文件（最新的在前面），然后添加已有的、不在新文件列表中的文件
                    foreach (var newFile in filePaths)
                    {
                        if (!updatedFiles.Contains(newFile))
                        {
                            updatedFiles.Add(newFile);
                        }
                    }
                    
                    foreach (var existingFile in existingFiles)
                    {
                        if (!updatedFiles.Contains(existingFile))
                        {
                            updatedFiles.Add(existingFile);
                        }
                    }
                }
                else
                {
                    // 如果日期不存在，直接使用新文件列表
                    updatedFiles = filePaths;
                }
                
                // 更新该日期的文件列表
                historyData[currentDate] = updatedFiles;
                
                // 序列化并保存 - 完整覆盖文件，而不是追加
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string updatedJson = System.Text.Json.JsonSerializer.Serialize(historyData, options);
                await File.WriteAllTextAsync(_historyFilePath, updatedJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存播放历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载播放历史记录 - 支持新的格式
        /// </summary>
        public async Task<IEnumerable<MediaItem>> LoadMediaHistoryAsync()
        {
            if (!File.Exists(_historyFilePath))
                return Enumerable.Empty<MediaItem>();

            try
            {
                var json = await File.ReadAllTextAsync(_historyFilePath);
                var mediaItems = new List<MediaItem>();
                
                // 尝试使用Dictionary<string, List<string>>格式解析
                var historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                
                if (historyData != null)
                {
                    // 从所有日期中收集文件路径
                    var allPaths = historyData.Values.SelectMany(paths => paths).Distinct();
                    
                    foreach (var path in allPaths)
                    {
                        if (File.Exists(path))
                        {
                            var mediaItem = new MediaItem
                            {
                                Name = Path.GetFileName(path),
                                Path = path,
                                FileSize = new FileInfo(path).Length,
                                MediaType = GetMediaType(path),
                                LastPlayed = DateTime.Now, // 简化处理，实际应该从日期键中解析
                                LastPosition = 0,
                                Duration = TimeSpan.Zero
                            };
                            mediaItems.Add(mediaItem);
                        }
                    }
                }
                else
                {
                    // 尝试向后兼容旧格式
                    var oldFormatHistory = System.Text.Json.JsonSerializer.Deserialize<List<MediaItem>>(json);
                    if (oldFormatHistory != null)
                    {
                        mediaItems.AddRange(oldFormatHistory.Where(item => File.Exists(item.Path)));
                    }
                }
                
                return mediaItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载播放历史失败: {ex.Message}");
                return Enumerable.Empty<MediaItem>();
            }
        }

        /// <summary>
        /// 清空播放历史记录
        /// </summary>
        public async Task ClearMediaHistoryAsync()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    File.Delete(_historyFilePath);
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空播放历史失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取支持的媒体文件扩展名
        /// </summary>
        public IEnumerable<string> GetSupportedExtensions()
        {
            return _supportedExtensions;
        }

        /// <summary>
        /// 检查文件是否为支持的媒体格式
        /// </summary>
        public bool IsSupportedMediaFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return _supportedExtensions.Contains(extension);
        }

        /// <summary>
        /// 根据文件扩展名获取媒体类型
        /// </summary>
        private string GetMediaType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".flv" or ".m4v" or ".webm" => "视频",
                ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" or ".wma" => "音频",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取所有历史记录日期 - 直接从history.json中读取
        /// </summary>
        public async Task<IEnumerable<DateTime>> GetAllHistoryDatesAsync()
        {
            var dates = new List<DateTime>();
            
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    var historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    
                    if (historyData != null)
                    {
                        foreach (var dateKey in historyData.Keys)
                        {
                            if (DateTime.TryParseExact(dateKey, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                            {
                                dates.Add(date);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取历史记录日期失败: {ex.Message}");
            }
            
            return dates.OrderByDescending(d => d);
        }

        /// <summary>
        /// 根据日期获取媒体历史记录 - 直接从history.json中读取特定日期的文件
        /// </summary>
        public async Task<IEnumerable<MediaItem>> GetMediaHistoryByDateAsync(DateTime date)
        {
            var mediaItems = new List<MediaItem>();
            
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    var historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    
                    if (historyData != null)
                    {
                        string dateKey = date.ToString("yyyy-MM-dd");
                        if (historyData.TryGetValue(dateKey, out List<string>? filePaths))
                        {
                            foreach (var path in filePaths)
                            {
                                if (File.Exists(path))
                                {
                                    var mediaItem = new MediaItem
                                    {
                                        Name = Path.GetFileName(path),
                                        Path = path,
                                        FileSize = new FileInfo(path).Length,
                                        MediaType = GetMediaType(path),
                                        LastPlayed = date,
                                        LastPosition = 0,
                                        Duration = TimeSpan.Zero
                                    };
                                    mediaItems.Add(mediaItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取特定日期历史记录失败: {ex.Message}");
            }
            
            return mediaItems;
        }
        
        /// <summary>
        /// 删除单个历史记录项
        /// </summary>
        public async Task RemoveHistoryItemAsync(DateTime date, string filePath)
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    var historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    
                    if (historyData != null)
                    {
                        string dateKey = date.ToString("yyyy-MM-dd");
                        if (historyData.TryGetValue(dateKey, out List<string>? filePaths))
                        {
                            filePaths.Remove(filePath);
                            // 如果该日期下没有文件了，则删除该日期条目
                            if (filePaths.Count == 0)
                            {
                                historyData.Remove(dateKey);
                            }
                            
                            // 保存更新后的历史记录
                            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                            string updatedJson = System.Text.Json.JsonSerializer.Serialize(historyData, options);
                            await File.WriteAllTextAsync(_historyFilePath, updatedJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除历史记录项失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清除指定日期的所有历史记录
        /// </summary>
        public async Task ClearHistoryByDateAsync(DateTime date)
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var json = await File.ReadAllTextAsync(_historyFilePath);
                    var historyData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    
                    if (historyData != null)
                    {
                        string dateKey = date.ToString("yyyy-MM-dd");
                        if (historyData.ContainsKey(dateKey))
                        {
                            // 删除该日期的所有历史记录
                            historyData.Remove(dateKey);
                            
                            // 保存更新后的历史记录
                            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                            string updatedJson = System.Text.Json.JsonSerializer.Serialize(historyData, options);
                            await File.WriteAllTextAsync(_historyFilePath, updatedJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清除历史记录失败: {ex.Message}");
            }
        }
    }
}