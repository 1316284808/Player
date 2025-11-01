using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Player.Core.Models;
using Player.Core.Commands;

namespace Player.ViewModel
{
    public class PlayerViewModel : INotifyPropertyChanged
    {
        private PlaybackState _playbackState = new PlaybackState();
        private string _currentTime = "0:00";
        private string _totalTime = "0:00";
        private double _progress;
        private bool _controlsEnabled = false;
        private bool _isUserInitiated = false;
        
        // 视频文件扩展名列表
        private readonly string[] _videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".m4v", ".webm" };
        
        // 播放列表集合
        private ObservableCollection<MediaItem> _playlist = new ObservableCollection<MediaItem>();
        public ObservableCollection<MediaItem> Playlist
        { 
            get => _playlist; 
            set { _playlist = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        // 历史记录保存完成事件
        public event EventHandler HistorySaved;
        
        // 触发历史记录保存完成事件
        protected virtual void OnHistorySaved()
        {
            HistorySaved?.Invoke(this, EventArgs.Empty);
        }

        // 属性
        public PlaybackState PlaybackState
        {
            get => _playbackState;
            set { _playbackState = value; OnPropertyChanged(); }
        }

        public bool IsPlaying
        {
            get => _playbackState.IsPlaying;
            set { 
                if (_playbackState.IsPlaying != value) {
                    _playbackState.IsPlaying = value; 
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlaybackState));
                }
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public string TotalTime
        {
            get => _totalTime;
            set { _totalTime = value; OnPropertyChanged(); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        public double Volume
        {
            get => _playbackState.Volume;
            set { 
                if (_playbackState.Volume != value) {
                    _playbackState.Volume = (int)value; 
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlaybackState));
                }
            }
        }

        public bool ControlsEnabled
        {
            get => _controlsEnabled;
            set { _controlsEnabled = value; OnPropertyChanged(); }
        }

        public bool IsUserInitiated
        {
            get => _isUserInitiated;
            set { _isUserInitiated = value; OnPropertyChanged(); }
        }

        // 命令
        public ICommand PlayPauseCommand { get; }
        public ICommand FullscreenCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }

        // 构造函数
        public PlayerViewModel()
        {
            PlayPauseCommand = new RelayCommand(PlayPause);
            FullscreenCommand = new RelayCommand(Fullscreen);
            OpenFileCommand = new RelayCommand(OpenFile);
            OpenFolderCommand = new RelayCommand(OpenFolder);
        }

        // 方法
        private void PlayPause()
        {
            // 播放/暂停逻辑将在MainWindow中实现
        }

        private void Fullscreen()
        {
            // 全屏逻辑将在MainWindow中实现
        }

        private void OpenFile()
        {
            // 打开文件逻辑将在MainWindow中实现
        }
        
        private void OpenFolder()
        {
            // 使用系统文件夹选择器（通过Windows API），支持预览功能
            string folderPath = ShowFolderBrowserDialog();
            if (!string.IsNullOrEmpty(folderPath))
            {
                // 清空当前播放列表
                Playlist.Clear();
                
                // 获取文件夹中的所有视频文件
                var videoFiles = AddVideoFilesFromFolder(folderPath);
                
                // 如果找到了视频文件，保存到历史记录
                if (videoFiles.Count > 0)
                {
                    SaveToHistory(videoFiles);
                }
            }
        }
        
        /// <summary>
        /// 显示系统文件选择器对话框（支持视频预览功能）
        /// </summary>
        /// <returns>选择的文件夹路径，如果取消则返回null</returns>
        private string? ShowFolderBrowserDialog()
        {
            // 使用OpenFileDialog模拟文件夹选择，并启用预览功能
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择文件夹",
                Filter = "媒体文件|*.mp4;*.avi;*.mkv;*.wmv;*.mp3;*.wav;*.flac;*.m4a|所有文件|*.*",
                CheckFileExists = false, // 允许选择文件夹
                FileName = "选择文件夹", // 占位文本
                ValidateNames = false // 允许输入非标准文件名（文件夹路径）
            };
            
            // 显示对话框
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                // 获取选择的文件或文件夹路径
                string path = dialog.FileName;
                
                // 如果选择的是文件，返回其所在文件夹
                if (System.IO.File.Exists(path))
                {
                    return System.IO.Path.GetDirectoryName(path);
                }
                // 如果选择的是文件夹（通过取消选中文件），返回文件夹路径
                else if (System.IO.Directory.Exists(path))
                {
                    return path;
                }
                // 如果路径既不是文件也不是文件夹，尝试提取文件夹部分
                else
                {
                    string? directoryPath = System.IO.Path.GetDirectoryName(path);
                    return !string.IsNullOrEmpty(directoryPath) && System.IO.Directory.Exists(directoryPath) ? 
                        directoryPath : null;
                }
            }
            
            return null;
        }
        
        private List<string> AddVideoFilesFromFolder(string folderPath)
        {
            var videoFilePaths = new List<string>();
            
            try
            {
                // 获取文件夹中的所有文件
                var files = Directory.GetFiles(folderPath)
                    .Where(file => _videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .OrderBy(file => Path.GetFileName(file));
                
                foreach (var file in files)
                {
                    Playlist.Add(new MediaItem
                    {
                        Name = Path.GetFileName(file),
                        Path = file
                    });
                    
                    // 添加到返回列表
                    videoFilePaths.Add(file);
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"添加视频文件时出错: {ex.Message}");
            }
            
            return videoFilePaths;
        }
        
        /// <summary>
        /// 保存文件列表到历史记录
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        private void SaveToHistory(List<string> filePaths)
        {
            try
            {
                // 获取当前日期
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                
                // 构建JSON文件路径
                string jsonDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON");
                string jsonPath = Path.Combine(jsonDirectory, "history.json");
                
                // 确保JSON目录存在
                if (!Directory.Exists(jsonDirectory))
                {
                    Directory.CreateDirectory(jsonDirectory);
                }
                
                // 读取现有的历史记录
                Dictionary<string, List<string>> historyData = new Dictionary<string, List<string>>();
                
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        historyData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent) ?? new Dictionary<string, List<string>>();
                    }
                }
                
                // 更新当前日期的文件列表
                historyData[currentDate] = filePaths;
                
                // 序列化并保存到文件
                var options = new JsonSerializerOptions { WriteIndented = true };
                string updatedJson = JsonSerializer.Serialize(historyData, options);
                File.WriteAllText(jsonPath, updatedJson);
                
                Console.WriteLine($"已保存 {filePaths.Count} 个文件到历史记录");
                
                // 触发历史记录保存完成事件
                OnHistorySaved();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存历史记录失败: {ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    
}