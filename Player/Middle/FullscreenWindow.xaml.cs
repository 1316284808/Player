using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;

namespace Player.Middle
{
    public partial class FullscreenWindow : Window
    {
        private LibVLC? _libvlc;
        private MediaPlayer? _mediaPlayer;
        private string? _videoPath;
        private long _playbackTime;
        private bool _wasPlaying;
        private MiddleControl? _middleControl; // 保存对MiddleControl的引用

        #region 构造函数
        public FullscreenWindow()
        {
            InitializeComponent();
            // 初始化LibVLC
            LibVLCSharp.Shared.Core.Initialize();
            _libvlc = new LibVLC();
        }
        
        public FullscreenWindow(object owner, string videoPath, long playbackTime, bool wasPlaying, LibVLC libVlc)
        {
            InitializeComponent();
            
            // 初始化变量
            if (owner is Window windowOwner)
            {
                this.Owner = windowOwner;
            }
            // 如果owner是MiddleControl，直接保存引用
            if (owner is MiddleControl middleControl)
            {
                _middleControl = middleControl;
            }
            _videoPath = videoPath;
            _playbackTime = playbackTime;
            _wasPlaying = wasPlaying;
            _libvlc = libVlc ?? new LibVLC();
            
            // 设置窗口所有者
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        #endregion

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // 创建并配置MediaPlayer
            if (_mediaPlayer == null && _libvlc != null && !string.IsNullOrEmpty(_videoPath))
            {
                _mediaPlayer = new MediaPlayer(_libvlc);
                video.MediaPlayer = _mediaPlayer;
                
                // 加载视频
                try
                {
                    var media = new Media(_libvlc, new Uri(_videoPath));
                    _mediaPlayer.Play(media);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载视频失败: {ex.Message}");
                    return;
                }
                
                // 设置播放位置
                if (_playbackTime > 0)
                {
                    _mediaPlayer.Time = _playbackTime;
                }
                
                // 恢复播放状态
                if (!_wasPlaying)
                {
                    _mediaPlayer.Pause();
                }
                
                // 设置FullscreenControl的MediaPlayer和播放状态
                if (fullscreenControl != null)
                {
                    fullscreenControl.MediaPlayer = _mediaPlayer;
                    fullscreenControl.IsPlaying = _wasPlaying;
                    
                    // 绑定退出全屏事件
                    fullscreenControl.ExitFullscreen += (s, args) => this.Close();
                    
                    // 确保按钮使用图标而不是文本
                    if (fullscreenControl.IsPlaying)
                    {
                        // 直接使用fullscreenControl的UpdatePlayIcon方法更新图标，而不是修改Content
                        fullscreenControl.UpdatePlayIcon(true);
                    }
                }
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 窗口加载时的初始化
            this.Activate();
            this.Focus();
            
            // 订阅键盘事件，处理ESC键退出全屏
            this.KeyDown += FullscreenWindow_KeyDown;
            
            // ClickOverlay的点击事件已在XAML中绑定，无需在这里订阅
        }
        
        private void FullscreenWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // 当按下ESC键时，退出全屏（关闭窗口）
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
        
        private void ClickOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 确保在主线程上操作UI元素
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    // 直接设置FullscreenControl的Opacity属性来显示控制栏（基于空域处理原则）
                    if (fullscreenControl != null)
                    {
                        // 方法1：直接设置Opacity属性（更简单直接）
                        fullscreenControl.Opacity = 1.0;
                        
                        // 方法2：通过反射调用FullscreenControl的私有方法（备用方案）
                        try
                        {
                            var showControlsMethod = fullscreenControl.GetType().GetMethod("ShowControls", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var startHideTimerMethod = fullscreenControl.GetType().GetMethod("StartHideControlsTimer", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (showControlsMethod != null)
                            {
                                showControlsMethod.Invoke(fullscreenControl, null);
                            }
                            
                            if (startHideTimerMethod != null)
                            {
                                startHideTimerMethod.Invoke(fullscreenControl, null);
                            }
                        }
                        catch (Exception)
                            {
                                // 反射调用失败时，确保控制栏仍然可见
                                if (fullscreenControl.Content is Border border)
                                {
                                    border.Opacity = 1.0;
                                }
                            }
                    }
                }
                catch (Exception)
                {
                    // 静默处理异常，避免全屏模式下崩溃
                }
            });
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 获取当前播放状态
            long currentPlaybackTime = 0;
            bool isPlaying = false;
            
            if (_mediaPlayer != null)
            {
                currentPlaybackTime = _mediaPlayer.Time;
                isPlaying = _mediaPlayer.IsPlaying;
                
                // 清理资源
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
            
            // 将最新的播放状态直接传递给MiddleControl
            if (_middleControl != null)
            {
                _middleControl.UpdatePlaybackState(currentPlaybackTime, isPlaying);
            }
            
            // 调用基类方法
            base.OnClosing(e);
            
            // 清理事件订阅
            this.KeyDown -= FullscreenWindow_KeyDown;
            
            // 注意：这里不释放_libvlc，因为它可能在主窗口中被复用
        }
    }
}