using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Player.ViewModel;
using Player.Helpers;
using Player.Core.Models;


namespace Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlayerViewModel ViewModel { get; private set; }
        private Middle.MiddleControl middleControl;
        private Bottom.BottomControl bottomControl;
        private AppSettings Settings { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new PlayerViewModel();
            DataContext = ViewModel;
            
            // 加载所有配置项
            ConfigManager.LoadAllConfigs();
            
            // 加载设置
            Settings = ConfigManager.LoadSettings();
            
            // 添加命令绑定
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFile_Executed));
            
            // 添加键盘快捷键
            KeyDown += MainWindow_KeyDown;
            
            // 加载完成后获取中间控件引用
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 订阅历史记录保存完成事件
            ViewModel.HistorySaved += ViewModel_HistorySaved;
            
            // 获取控件引用
            middleControl = FindName("middleControl") as Middle.MiddleControl ?? 
                          VisualTreeHelperExtensions.FindVisualChild<Middle.MiddleControl>(this);
            
            bottomControl = FindName("bottomControl") as Bottom.BottomControl ?? 
                           VisualTreeHelperExtensions.FindVisualChild<Bottom.BottomControl>(this);
            
            // 设置BottomControl对MiddleControl的引用，使全屏功能正常工作
            if (bottomControl != null && middleControl != null)
            {
                bottomControl.MiddleControl = middleControl;
            }
            
            // 使用窗口级别的ToastMessageWindow，无需初始化
        }

        // 处理播放/暂停命令
        private void PlayPauseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TogglePlayPause();
        }

        // 打开文件命令
        private void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenMediaFile();
        }

        /// <summary>
        /// 刷新主题配置（公共方法，供其他类调用）
        /// </summary>
        // 处理历史记录保存完成事件
        private void ViewModel_HistorySaved(object sender, EventArgs e)
        {   
            Dispatcher.Invoke(() =>
            {   
                // 获取左侧控件引用
                var leftControlRef = FindName("leftControl") as Left.LeftControl ?? 
                                   VisualTreeHelperExtensions.FindVisualChild<Left.LeftControl>(this);
                
                // 刷新左侧控件的历史记录
                leftControlRef?.LoadHistoryDates();
            });
        }
        
        public void RefreshTheme()
        {
            try
            {
                // 重新加载主题配置
                var theme = ConfigManager.LoadTheme();
                
                // 使用ThemeManager应用主题色到整个应用程序
                if (!string.IsNullOrEmpty(theme.PrimaryColor))
                {
                    ThemeManager.ApplyTheme(theme.PrimaryColor);
                }

                SystemNotificationHelper.ShowSuccess("主题已刷新");
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"刷新主题失败: {ex.Message}");
            }
        }
        
        // 键盘快捷键处理
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                // 空格键播放/暂停
                e.Handled = true;
                TogglePlayPause();
            }
            else if (e.Key == Key.O && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+O 打开文件
                e.Handled = true;
                OpenMediaFile();
            }
            else if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+F 全屏
                e.Handled = true;
                ToggleFullscreen();
            }
        }

        // 播放/暂停切换
        private void TogglePlayPause()
        {
            if (middleControl != null)
            {
                middleControl.TogglePlayPause();
            }
        }

        // 打开媒体文件
        private void OpenMediaFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "媒体文件|*.mp4;*.avi;*.mkv;*.wmv;*.mp3;*.wav;*.flac;*.m4a|所有文件|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (middleControl != null)
                {
                    middleControl.PlayMedia(openFileDialog.FileName);
                    Title = $"媒体播放器 - {System.IO.Path.GetFileName(openFileDialog.FileName)}";
                }
            }
        }

        // 切换全屏
        private void ToggleFullscreen()
        {
            if (middleControl != null)
            {
                middleControl.ToggleFullscreen();
            }
        }

        // 文件拖放功能
        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    
                    // 检查文件类型
                    string extension = System.IO.Path.GetExtension(filePath);
                    extension = extension.ToLower();
                    
                    // 检查是否为支持的文件格式
                    bool isSupported = IsSupportedFileFormat(extension);
                    
                    if (isSupported)
                    {
                        PlayMediaIfControlExists(filePath);
                    }
                    else
                    {
                        SystemNotificationHelper.ShowWarning("不支持的文件格式");
                    }
                }
            }
            e.Handled = true;
        }

        // 使用Helpers命名空间中的VisualTreeHelperExtensions类
        // 已移除重复的FindVisualChild方法
        
        /// <summary>
        /// 检查是否为支持的文件格式
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>是否支持</returns>
        private bool IsSupportedFileFormat(string extension)
        {
            string[] supportedExtensions = { ".mp4", ".avi", ".mkv", ".wmv", ".mp3", ".wav", ".flac", ".m4a" };
            return supportedExtensions.Contains(extension);
        }
        
        /// <summary>
        /// 如果中间控件存在则播放媒体文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private void PlayMediaIfControlExists(string filePath)
        {
            if (middleControl != null)
            {
                middleControl.PlayMedia(filePath);
                Title = $"媒体播放器 - {System.IO.Path.GetFileName(filePath)}";
            }
        }

        /// <summary>
        /// 播放指定的媒体文件
        /// </summary>
        /// <param name="filePath">媒体文件路径</param>
        public void PlayMediaFile(string filePath)
        {
            if (middleControl != null && System.IO.File.Exists(filePath))
            {
                middleControl.PlayMedia(filePath);
                Title = $"媒体播放器 - {System.IO.Path.GetFileName(filePath)}";
            }
        }
        
        // 标题栏鼠标按下事件 - 用于拖动窗口
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        
        // 最小化按钮点击事件
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        // 关闭按钮点击事件
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}