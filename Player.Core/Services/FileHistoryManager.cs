using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Player.Core.Services
{
    /// <summary>
    /// 文件历史记录管理器实现
    /// </summary>
    public class FileHistoryManager : IHistoryManager
    {
        private Dictionary<string, List<string>> _historyData = new();
        private readonly string _historyFilePath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        public FileHistoryManager()
        {
            // 设置历史记录文件路径
            string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON");
            Directory.CreateDirectory(appDataPath);
            _historyFilePath = Path.Combine(appDataPath, "history.json");
        }
        
        public void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string jsonContent = File.ReadAllText(_historyFilePath);
                    _historyData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent) ?? new Dictionary<string, List<string>>();
                }
                else
                {
                    _historyData = new Dictionary<string, List<string>>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载历史记录失败: {ex.Message}");
                _historyData = new Dictionary<string, List<string>>();
            }
        }
        
        public void SaveHistory()
        {
            try
            {
                string jsonContent = JsonSerializer.Serialize(_historyData, _jsonOptions);
                File.WriteAllText(_historyFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存历史记录失败: {ex.Message}");
            }
        }
        
        public List<DateTime> GetAllDates()
        {
            List<DateTime> dates = new();
            foreach (var dateKey in _historyData.Keys)
            {
                if (DateTime.TryParseExact(dateKey, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    dates.Add(date);
                }
            }
            return dates;
        }
        
        public List<string> GetFilesByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");
            if (_historyData.TryGetValue(dateKey, out List<string>? files))
            {
                return files;
            }
            return new List<string>();
        }
        
        public void AddHistoryItem(string filePath, DateTime playTime)
        {
            string dateKey = playTime.ToString("yyyy-MM-dd");
            
            // 确保该日期的列表存在
            if (!_historyData.ContainsKey(dateKey))
            {
                _historyData[dateKey] = new List<string>();
            }
            
            // 如果文件已经在列表中，则移除旧的条目
            if (_historyData[dateKey].Contains(filePath))
            {
                _historyData[dateKey].Remove(filePath);
            }
            
            // 添加到列表开头（最新的在前面）
            _historyData[dateKey].Insert(0, filePath);
            
            // 自动保存
            SaveHistory();
        }
        
        public void ClearHistoryByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");
            if (_historyData.ContainsKey(dateKey))
            {
                _historyData.Remove(dateKey);
                SaveHistory();
            }
        }
        
        public void ClearAllHistory()
        {
            _historyData.Clear();
            SaveHistory();
        }
    }
}