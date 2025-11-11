using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Player.Core.Events;
using Player.Core.Services;
using Player.Core.Models;
using Player.Core.Repositories;
using System.Windows.Forms;

namespace Player.ViewModels
{
    /// <summary>
    /// 左侧面板视图模型 (LeftViewModel)
    /// 
    /// MVVM模式说明：
    /// - 继承自ViewModelBase，提供基础功能
    /// - 负责管理播放历史、文件夹选择和播放列表
    /// - 通过事件总线和消息系统与其他ViewModel通信
    /// 
    /// 主要职责：
    /// 1. 管理播放历史记录
    /// 2. 处理文件夹选择和文件扫描
    /// 3. 维护播放列表数据
    /// 4. 发送媒体选择消息给MainViewModel
    /// </summary>
    public partial class LeftViewModel : ObservableObject, IDisposable
    {
        // 依赖注入的服务接口
        private readonly IMediaRepository _mediaRepository;      // 媒体数据访问层
        private readonly IMessengerService _messengerService;    // 消息通信服务
        
        // MVVM数据绑定属性 - 这些属性会自动通知UI更新
        
        [ObservableProperty]
        private bool _isSidebarCollapsed = false;                    // 侧边栏是否折叠
        
        [ObservableProperty]
        private ObservableCollection<string> _historyDates = new();   // 历史记录日期列表
        
        [ObservableProperty]
        private ObservableCollection<MediaItem> _currentHistoryFiles = new();  // 当前日期对应的历史文件
        
        [ObservableProperty]
        private string? _selectedHistoryDate;                        // 选中的历史日期
        
        [ObservableProperty]
        private MediaItem? _selectedMediaItem;                       // 选中的媒体项
        
        partial void OnSelectedMediaItemChanged(MediaItem? value)
        {            
            if (value != null)            
            {
                HandleMediaSelection(value);
            }
        }
        
        [RelayCommand]
        private void SelectMedia(MediaItem? item)
        {
            if (item != null)
            {
                // 先更新SelectedMediaItem属性，这样会触发OnSelectedMediaItemChanged
                SelectedMediaItem = item;
            }
        }
        
        private void HandleMediaSelection(MediaItem item)
        {
             
            try
            {
                // 直接获取MiddleViewModel并调用LoadMedia方法，绕过消息传递
                var middleViewModel = Services.DependencyInjectionService.GetViewModel<MiddleViewModel>();
                if (middleViewModel != null)
                {
                    // 确保VLC已初始化
                    if (!middleViewModel.IsVlcInitialized)
                    {
                       middleViewModel.InitializeVlc();
                    }
                       middleViewModel.LoadMedia(item.Path);
                }
                
            }
            catch (Exception ex)
            {
               
            }
            
            // 同时更新播放历史
            // 这里不需要手动添加历史记录，因为选择文件时已经通过播放列表更新了历史记录
        }
        
        /// <summary>
        /// 构造函数 - 依赖注入模式
        /// </summary>
        /// <param name="mediaRepository">媒体数据访问层</param>
        /// <param name="messengerService">消息通信服务</param>
        public LeftViewModel(IMediaRepository mediaRepository, IMessengerService messengerService)
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _messengerService = messengerService ?? throw new ArgumentNullException(nameof(messengerService));
            
            LoadHistoryDates();                      // 加载历史日期
            
            // 订阅消息 - 使用消息服务注册消息处理器
            _messengerService.Register<MediaSelectedMessage>(this, OnMediaSelectedMessage);
        }
        
        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            // 侧边栏状态变更可以通过其他ViewModel监听PlaybackState来实现
        }

        /// <summary>
        /// 打开文件夹命令 - [RelayCommand]特性自动生成命令绑定
        /// 
        /// MVVM命令模式说明：
        /// - 命令是用户操作的抽象表示
        /// - 在XAML中通过Command属性绑定到按钮等控件
        /// - 当用户点击按钮时，自动调用此方法
        /// </summary>
        [RelayCommand]
        private async Task OpenFolderAsync()
        {
            // 参考PlayerViewModel的实现，使用OpenFileDialog模拟文件夹选择，并启用预览功能
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择文件夹",
                Filter = "媒体文件|*.mp4;*.avi;*.mkv;*.wmv;*.mp3;*.wav;*.flac;*.m4a|所有文件|*.*",
                CheckFileExists = false, // 允许选择文件夹
                FileName = "选择文件夹", // 占位文本
                ValidateNames = false, // 允许输入非标准文件名（文件夹路径）
                Multiselect = true // 启用多选，支持批量选择文件
            };
            
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    // 获取选择的文件夹路径（参考PlayerViewModel的逻辑）
                    string? folderPath = GetSelectedFolderPath(dialog.FileName);
                    
                    if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                    {
                        // 使用Repository模式获取媒体文件
                        var mediaItems = await _mediaRepository.GetMediaItemsAsync(folderPath);
                        
                        if (mediaItems.Any())
                        {
                            // 保存播放历史记录
                            await _mediaRepository.SaveMediaHistoryAsync(mediaItems);
                            
                            // 重新加载历史记录和播放列表
                            LoadHistoryDates();
                            
                            // 注意：这里不自动播放第一个文件，只是更新播放列表
                            // 用户需要手动选择要播放的文件
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("所选文件夹中没有找到支持的媒体文件", "提示", 
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("选择的路径无效或不存在", "错误", 
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"打开文件夹失败: {ex.Message}", "错误", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        
        /// <summary>
        /// 从OpenFileDialog的文件名中提取文件夹路径（参考PlayerViewModel的实现）
        /// </summary>
        /// <param name="fileName">OpenFileDialog返回的文件名</param>
        /// <returns>文件夹路径，如果无效则返回null</returns>
        private string? GetSelectedFolderPath(string fileName)
        {
            // 如果选择的是文件，返回其所在文件夹
            if (File.Exists(fileName))
            {
                return Path.GetDirectoryName(fileName);
            }
            // 如果选择的是文件夹（通过取消选中文件），返回文件夹路径
            else if (Directory.Exists(fileName))
            {
                return fileName;
            }
            // 如果路径既不是文件也不是文件夹，尝试提取文件夹部分
            else
            {
                string? directoryPath = Path.GetDirectoryName(fileName);
                return !string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath) ? 
                    directoryPath : null;
            }
        }
        
        /// <summary>
        /// 加载历史记录数据
        /// </summary>
        public async void LoadHistoryDates()
        {
            try
            {
                HistoryDates.Clear();
                var dates = await _mediaRepository.GetAllHistoryDatesAsync();
                foreach (var date in dates.OrderByDescending(d => d))
                {
                    HistoryDates.Add(date.ToString("yyyy-MM-dd"));
                }
                
                // 默认选择第一个日期（最新日期）并加载对应的播放列表
                if (HistoryDates.Any())
                {
                    SelectedHistoryDate = HistoryDates.First();
                    // 注意：不需要手动调用ShowHistoryByDateAsync，因为设置SelectedHistoryDate会自动触发OnSelectedHistoryDateChanged
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载历史记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理历史日期选择变更
        /// </summary>
        partial void OnSelectedHistoryDateChanged(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                CurrentHistoryFiles.Clear();
                // 异步方法调用，但不等待结果，因为这是属性变更处理程序
                ShowHistoryByDateAsync(value);
            }
        }
        
        /// <summary>
        /// 删除历史记录项命令
        /// </summary>
        [RelayCommand]
        private async Task DeleteHistoryItem(MediaItem? item)
        {
            if (item != null && !string.IsNullOrEmpty(SelectedHistoryDate))
            {
                try
                {
                    if (DateTime.TryParseExact(SelectedHistoryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                    {
                        // 调用MediaRepository删除历史记录项
                        await _mediaRepository.RemoveHistoryItemAsync(date, item.Path);
                        
                        // 从当前显示的列表中移除该项
                        CurrentHistoryFiles.Remove(item);
                        
                        // 如果当前日期没有记录了，重新加载历史日期列表
                        if (!CurrentHistoryFiles.Any())
                        {
                            LoadHistoryDates();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"删除历史记录失败: {ex.Message}", "错误", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        
        /// <summary>
        /// 删除指定日期的所有历史记录命令
        /// </summary>
        [RelayCommand]
        private async Task DeleteHistoryDate(string? dateString)
        {
            if (!string.IsNullOrEmpty(dateString))
            {
                try
                {
                    // 确认删除
                    var result = System.Windows.MessageBox.Show($"确定要删除 {dateString} 的所有历史记录吗？", "确认删除", 
                        System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                    
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                        {
                            // 调用MediaRepository删除该日期的所有历史记录
                            await _mediaRepository.ClearHistoryByDateAsync(date);
                            
                            // 如果删除的是当前选中的日期，清空当前文件列表
                            if (SelectedHistoryDate == dateString)
                            {
                                CurrentHistoryFiles.Clear();
                            }
                            
                            // 重新加载历史日期列表
                            LoadHistoryDates();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"删除历史记录失败: {ex.Message}", "错误", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void ShowHistoryByDate (string dateString) {

            Console.WriteLine("");
        
        }


      
        [RelayCommand]
        private void ShowHistoryByDateAsync(string dateString)
        {
            try
            {
                List<MediaItem > lisr = new List<MediaItem>();
                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    CurrentHistoryFiles.Clear();
                    // 异步调用，但不等待结果
                    _ = Task.Run(async () =>
                    {
                        var mediaItems = await _mediaRepository.GetMediaHistoryByDateAsync(date);
                        
                        // 回到UI线程更新界面
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var mediaItem in mediaItems)
                            {
                                CurrentHistoryFiles.Add(mediaItem);
                                lisr.Add(mediaItem);
                            }
                            // 历史记录已添加到CurrentHistoryFiles中
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载历史文件失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 打开设置对话框命令
        /// </summary>
        [RelayCommand]
        private void OpenSettings()
        {
            try
            {
                // 使用消息服务发送打开设置对话框的消息
                // 这符合MVVM模式，ViewModel不应该直接创建和显示视图
                _messengerService.Send(new OpenSettingsMessage());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"打开设置失败: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 处理媒体选择消息
        /// </summary>
        private void OnMediaSelectedMessage(object recipient, MediaSelectedMessage message)
        {
            // 可以在这里添加历史记录更新逻辑
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            _messengerService.UnregisterAll(this);
        }
    }
}