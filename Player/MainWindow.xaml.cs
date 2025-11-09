using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Globalization;
using System.Windows.Data;
using Player.ViewModels;
using Player.Helpers;
using Player.Core.Models;
using Player.Core.Events;
using Player.Core.Repositories;
using Player.Core.Services; // 重新添加以使用IMessengerService
using Player.Services; // 优先使用Player.Services命名空间

namespace Player
{
    /// <summary>
    /// 反向布尔值到可见性转换器
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFullscreen)
            {
                return isFullscreen ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return true;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; private set; } = null!;
        public BottomViewModel BottomViewModel { get; private set; } = null!;
        private Middle.MiddleControl? middleControlInstance;
        private Bottom.BottomControl? bottomControlInstance;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            
            // 主题已在App.xaml.cs中初始化，这里不再重复初始化
            
            // 添加键盘快捷键
            KeyDown += MainWindow_KeyDown;
            
            // 加载完成后获取中间控件引用
            Loaded += MainWindow_Loaded;
            
            // 窗口关闭时保存状态
            Closing += MainWindow_Closing;
        }

        private void InitializeViewModel()
        {
            try
            {
                // 使用依赖注入容器获取ViewModel实例
                ViewModel = Services.DependencyInjectionService.GetViewModel<MainViewModel>();
                DataContext = ViewModel;
                
                // 使用依赖注入容器获取BottomViewModel实例
                BottomViewModel = Services.DependencyInjectionService.GetViewModel<BottomViewModel>();
                
                // 现在ViewModel之间通过消息服务进行通信，不再需要手动创建共享状态
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ViewModel初始化失败", ex);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 直接获取控件引用
            middleControlInstance = FindName("middleControl") as Middle.MiddleControl ??
                           FindVisualChild<Middle.MiddleControl>(this);
            
            bottomControlInstance = FindName("BottomControl") as Bottom.BottomControl ??
                           FindVisualChild<Bottom.BottomControl>(this);
            
            // 确保MiddleControl有正确的DataContext
            if (middleControlInstance != null && ViewModel != null)
            {
                // 使用依赖注入容器获取MiddleViewModel实例
                var middleViewModel = Services.DependencyInjectionService.GetViewModel<MiddleViewModel>();
                
                middleControlInstance.DataContext = middleViewModel;
                
                // 保存到ViewModel中供后续使用
                ViewModel.MiddleViewModel = middleViewModel;
                
                // 初始化VLC播放器
                middleViewModel.InitializeVlc();
                
                // 添加一个简单的测试，当窗口加载时尝试播放一个视频文件
                // 这里可以注释掉，或者保留作为测试
                // TestPlayMedia(middleViewModel);
            }
            
            // 设置BottomControl的DataContext
            if (bottomControlInstance != null)
            {
                bottomControlInstance.DataContext = BottomViewModel;
            }
            
            // 监听全屏状态变化消息
            StartMonitoringFullscreenState();
            
            // 订阅全屏状态变化，用于控制底部控制栏的显示/隐藏
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ViewModel.PlaybackState))
                    {
                        UpdateBottomControlVisibility();
                    }
                };
            }
        }
        
        private void TogglePseudoFullscreen(bool isFullscreen)
        {
            try
            {
                if (isFullscreen)
                {
                    // 保存当前状态
                    _previousWindowStyle = WindowStyle;
                    _previousWindowState = WindowState;
                    _previousLeftColumnWidth = LeftColumn.Width;
                    _previousResizeMode = ResizeMode;
                    _previousShowInTaskbar = ShowInTaskbar;
                    _previousTop = Top;
                    _previousLeft = Left;
                    _previousWidth = Width;
                    _previousHeight = Height;
                    
                    // 第一步：先让窗体真正全屏，覆盖系统任务栏
                    WindowState = WindowState.Normal; // 先恢复为正常状态
                    
                    // 获取屏幕的工作区域（不包括任务栏）和整个屏幕区域
                    var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                    var workingArea = screen.WorkingArea; // 工作区域（不包括任务栏）
                    var bounds = screen.Bounds;           // 整个屏幕区域
                    
                    // 设置窗口位置和大小，覆盖整个屏幕，包括任务栏区域
                    Left = bounds.Left;
                    Top = bounds.Top;
                    Width = bounds.Width;
                    Height = bounds.Height;
                    
                    // 第二步：隐藏左侧面板，让RightColumn占据整个窗体空间
                    LeftColumn.Width = new GridLength(0);
                    
                    // 第三步：最后隐藏标题栏，此时RightColumn已经铺满整个窗体
                    WindowStyle = WindowStyle.None;
                    
                    // 隐藏系统边框和任务栏图标
                    ResizeMode = ResizeMode.NoResize;
                    ShowInTaskbar = false;
                    
                    // 确保窗口在最上层
                    Topmost = true;
                }
                else
                {
                    // 第一步：先恢复标题栏
                    WindowStyle = _previousWindowStyle;
                    
                    // 第二步：恢复左侧面板
                    LeftColumn.Width = _previousLeftColumnWidth;
                    
                    // 第三步：最后恢复窗口状态和位置
                    WindowState = WindowState.Normal; // 先恢复为Normal
                    
                    // 恢复窗口位置和大小
                    Left = _previousLeft;
                    Top = _previousTop;
                    Width = _previousWidth;
                    Height = _previousHeight;
                    
                    WindowState = _previousWindowState; // 再恢复为之前的状态
                    
                    // 恢复系统边框和任务栏图标
                    ResizeMode = _previousResizeMode;
                    ShowInTaskbar = _previousShowInTaskbar;
                    
                    // 取消窗口最上层状态
                    Topmost = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"切换伪全屏状态异常: {ex.Message}");
                // 尝试恢复基本状态
                try
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                    LeftColumn.Width = new GridLength(250);
                    ResizeMode = ResizeMode.CanResize;
                    ShowInTaskbar = true;
                    Topmost = false;
                }
                catch {}
            }
        }
        
        // 保存原始窗口状态的私有字段
        private WindowStyle _previousWindowStyle = WindowStyle.SingleBorderWindow;
        private WindowState _previousWindowState = WindowState.Normal;
        private GridLength _previousLeftColumnWidth = new GridLength(250);
        private ResizeMode _previousResizeMode = ResizeMode.CanResize;
        private bool _previousShowInTaskbar = true;
        private double _previousTop = 100;
        private double _previousLeft = 100;
        private double _previousWidth = 1200;
        private double _previousHeight = 800;
        
        private void StartMonitoringFullscreenState()
        {
            // 使用消息系统监听全屏状态变化，而不是轮询
            var messengerService = Services.DependencyInjectionService.GetService<IMessengerService>();
            if (messengerService != null)
            {
                // 注册全屏状态变化消息监听器
                messengerService.Register<FullscreenChangedMessage>(this, OnFullscreenChangedMessage);
                
                // 在窗口关闭时取消注册
                Closed += (s, e) => messengerService.UnregisterAll(this);
            }
        }
        
        private void OnFullscreenChangedMessage(object recipient, FullscreenChangedMessage message)
        {
            // 直接在UI线程中处理全屏状态变化
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"MainWindow收到全屏状态变化消息: {message.Value}");
                    
                    // 首先更新底部控制栏的可见性，确保UI状态更新
                    UpdateBottomControlVisibility();
                    
                    // 然后使用伪全屏功能
                    TogglePseudoFullscreen(message.Value);
                    
                    // 强制更新布局
                    UpdateLayout();
                    InvalidateVisual();
                    
                    System.Diagnostics.Debug.WriteLine($"全屏状态变化处理完成: {message.Value}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"全屏状态消息处理异常: {ex.Message}");
                }
            });
        }
        
      
        
        #region 全屏控制
        
        /// <summary>
        /// 更新底部控制栏的可见性
        /// </summary>
        private void UpdateBottomControlVisibility()
        {
            try
            {
                if (ViewModel?.PlaybackState?.IsFullscreen == true)
                {
                    // 全屏时：先创建副本，再隐藏主窗口的底部控制栏
                    // 这样可以确保原始控制栏在副本创建过程中保持可见和响应能力
                    
                    if (ViewModel?.MiddleViewModel != null && bottomControlInstance != null)
                    {
                        System.Diagnostics.Debug.WriteLine("开始创建BottomControl副本...");
                        
                        // 创建BottomControl的副本以避免可视化元素归属冲突
                        var bottomControlClone = new Bottom.BottomControl();
                        
                        // 为全屏副本设置专门的DataContext，使用现有的BottomViewModel
                        // 这样播放/暂停命令就能正确工作了
                        bottomControlClone.DataContext = BottomViewModel;
                        
                        // 强制初始化BottomControlClone
                        bottomControlClone.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        bottomControlClone.Arrange(new Rect(0, 0, bottomControlClone.DesiredSize.Width, bottomControlClone.DesiredSize.Height));
                        bottomControlClone.UpdateLayout();
                        
                        // 绑定BottomControlClone的Opacity属性到MiddleViewModel的ControlBarOpacity属性
                        var playerControlsContainer = bottomControlClone.FindName("PlayerControlsContainer") as System.Windows.Controls.Border;
                        if (playerControlsContainer != null && ViewModel?.MiddleViewModel != null)
                        {
                            var binding = new System.Windows.Data.Binding("ControlBarOpacity")
                            {
                                Source = ViewModel.MiddleViewModel
                            };
                            playerControlsContainer.SetBinding(System.Windows.UIElement.OpacityProperty, binding);
                        }
                        
                        // 先设置副本到全屏区域
                        ViewModel.MiddleViewModel.BottomControlContent = bottomControlClone;
                        
                        System.Diagnostics.Debug.WriteLine($"全屏模式：已创建并设置BottomControl副本，BottomControlContent是否为null: {ViewModel.MiddleViewModel.BottomControlContent == null}");
                        
                        // 只有在副本成功创建并设置后，才隐藏原始控制栏
                        bottomControlInstance.Visibility = Visibility.Collapsed;
                        
                        // 重置控制栏透明度并启动计时器
                        if (ViewModel?.MiddleViewModel != null)
                        {
                            ViewModel.MiddleViewModel.ControlBarOpacity = 1.0;
                            ViewModel.MiddleViewModel.ResetHideTimer();
                            System.Diagnostics.Debug.WriteLine("全屏模式：已重置控制栏透明度并启动计时器");
                        }
                        
                        System.Diagnostics.Debug.WriteLine("全屏模式：已隐藏原始底部控制栏");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"无法创建BottomControl副本：MiddleViewModel是否为null: {ViewModel?.MiddleViewModel == null}, bottomControlInstance是否为null: {bottomControlInstance == null}");
                    }
                }
                else
                {
                    // 非全屏时：先清空VideoPlayer中的内容，再显示主窗口的底部控制栏
                    if (ViewModel?.MiddleViewModel != null)
                    {
                        ViewModel.MiddleViewModel.BottomControlContent = null;
                        
                        // 停止计时器并重置控制栏透明度
                        ViewModel.MiddleViewModel.ControlBarOpacity = 1.0;
                        // 在MiddleViewModel的UpdateFullscreenStyles方法中已经处理了计时器的停止
                        
                        System.Diagnostics.Debug.WriteLine("非全屏模式：已清空BottomControlContent并重置控制栏透明度");
                    }
                    
                    // 然后显示原始控制栏
                    if (bottomControlInstance != null)
                    {
                        bottomControlInstance.Visibility = Visibility.Visible;
                        System.Diagnostics.Debug.WriteLine("非全屏模式：已显示原始底部控制栏");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新底部控制栏可见性异常: {ex.Message}");
            }
        }
        
        // 查找视觉树中的子控件
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;
                
                var found = FindVisualChild<T>(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        // 注意：移除了直接的事件处理器，现在通过EventBus通信
        // 全屏状态变化处理已移至ViewModel中
        
        public void RefreshTheme()
        {
            try
            {
                // 获取ThemeService实例（明确指定使用Player.Services命名空间下的接口）
                var themeService = Services.DependencyInjectionService.GetService<Player.Services.IThemeService>();
                
                // 重新加载主题配置（明确指定使用Player.Helpers命名空间下的类）
                var theme = Player.Helpers.LoadConfigManager.LoadTheme();
                
                // 使用ThemeService应用主题色到整个应用程序
                if (!string.IsNullOrEmpty(theme.PrimaryColor) && themeService.IsValidColor(theme.PrimaryColor))
                {
                    themeService.ApplyTheme(theme.PrimaryColor);
                }

                SystemNotificationHelper.ShowSuccess("主题已刷新");
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"刷新主题失败: {ex.Message}");
            }
        }
        
        // 键盘快捷键处理
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Escape键退出全屏
                if (ViewModel?.PlaybackState?.IsFullscreen == true)
                {
                    e.Handled = true;
                    ViewModel.ToggleFullscreenCommand.Execute(null);
                }
            }
            else if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                // 空格键播放/暂停
                e.Handled = true;
                ViewModel.PlayPauseCommand.Execute(null);
            }
            else if (e.Key == Key.O && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+O 打开文件
                e.Handled = true;
                ViewModel.OpenFileCommand.Execute(null);
            }
            else if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+F 全屏
                e.Handled = true;
                ViewModel.ToggleFullscreenCommand.Execute(null);
            }
            else if (e.Key == Key.Left && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+Left 快退
                e.Handled = true;
                ViewModel.SeekBackwardCommand.Execute(null);
            }
            else if (e.Key == Key.Right && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+Right 快进
                e.Handled = true;
                ViewModel.SeekForwardCommand.Execute(null);
            }
            else if (e.Key == Key.Up && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+Up 增加音量
                e.Handled = true;
                ViewModel.IncreaseVolumeCommand.Execute(null);
            }
            else if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+Down 减少音量
                e.Handled = true;
                ViewModel.DecreaseVolumeCommand.Execute(null);
            }
            else if (e.Key == Key.M && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+M 静音
                e.Handled = true;
                ViewModel.ToggleMuteCommand.Execute(null);
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // 保存窗口状态到播放状态
            ViewModel.WindowState = WindowState.ToString();
            ViewModel.WindowWidth = Width;
            ViewModel.WindowHeight = Height;
            ViewModel.WindowPositionX = Left;
            ViewModel.WindowPositionY = Top;
        }

        // 文件拖放功能 - 已移至ViewModel中通过命令处理
        
        // 窗口控制按钮已使用MVVM命令绑定或不再需要，已移除原生事件处理方法
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // 取消所有事件订阅避免内存泄漏
            KeyDown -= MainWindow_KeyDown;
            Loaded -= MainWindow_Loaded;
            Closing -= MainWindow_Closing;
            
            // 清理ViewModel资源
            if (ViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        #endregion
    }
}