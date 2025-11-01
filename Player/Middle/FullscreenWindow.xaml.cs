using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Player.Core.Models;

namespace Player.Middle
{
    public partial class FullscreenWindow : Window
    {
        private LibVLC? _libvlc;
        private MediaPlayer? _mediaPlayer;
        private PlaybackState _playbackState = new PlaybackState();
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
            
            // 使用PlaybackState实体类初始化播放状态
            _playbackState = new PlaybackState(videoPath, playbackTime, wasPlaying);
            _libvlc = libVlc ?? new LibVLC();
            
            // 设置窗口所有者
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        #endregion

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // 创建并配置MediaPlayer
            if (_mediaPlayer == null && _libvlc != null && !string.IsNullOrEmpty(_playbackState.MediaPath))
            {
                _mediaPlayer = new MediaPlayer(_libvlc);
                video.MediaPlayer = _mediaPlayer;
                
                // 加载视频
                try
                {
                    var media = new Media(_libvlc, new Uri(_playbackState.MediaPath));
                    _mediaPlayer.Play(media);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载视频失败: {ex.Message}");
                    return;
                }
                
                // 设置播放位置
                if (_playbackState.PlaybackTime > 0)
                {
                    _mediaPlayer.Time = _playbackState.PlaybackTime;
                }
                
                // 恢复播放状态
                if (!_playbackState.IsPlaying)
                {
                    _mediaPlayer.Pause();
                }
                
                // 设置FullscreenControl的MediaPlayer和播放状态
                if (fullscreenControl != null)
                {
                    // 设置MediaPlayer引用，这会自动根据MediaPlayer的实际状态更新UI
                    fullscreenControl.MediaPlayer = _mediaPlayer;
                    
                    // 绑定退出全屏事件
                    fullscreenControl.ExitFullscreen += (s, args) => this.Close();
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
            // 首先确保清理FullscreenControl中的MediaPlayer引用和事件订阅
            if (fullscreenControl != null)
            {
                // 先移除MediaPlayer的引用，这会触发属性setter中的事件清理
                fullscreenControl.MediaPlayer = null;
            }
            
            // 获取当前播放状态并更新PlaybackState实体类
            if (_mediaPlayer != null)
            {
                try
                {
                    // 尝试获取播放状态，但添加try-catch以防对象已部分无效
                    _playbackState.UpdatePlaybackTime(_mediaPlayer.Time);
                    _playbackState.UpdatePlaybackStatus(_mediaPlayer.IsPlaying);
                    _playbackState.UpdateMediaInfo(_mediaPlayer.Length);
                }
                catch (AccessViolationException)
                {
                    // 忽略访问冲突异常，使用默认值
                    _playbackState.Reset();
                }
                catch (Exception)
                {
                    // 忽略其他异常
                }
                
                try
                {
                    // 安全地停止和释放MediaPlayer
                    _mediaPlayer.Stop();
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null;
                }
                catch (Exception)
                {
                    // 忽略释放过程中的异常
                }
            }
            
            // 将最新的播放状态直接传递给MiddleControl
            if (_middleControl != null)
            {
                try
                {
                    // 使用PlaybackState实体类传递完整的播放状态
                    _middleControl.SetPlaybackState(_playbackState);
                }
                catch (Exception)
                {
                    // 忽略传递状态时的异常
                }
            }
            
            // 清理事件订阅
            this.KeyDown -= FullscreenWindow_KeyDown;
            
            // 调用基类方法
            base.OnClosing(e);
            
            // 注意：这里不释放_libvlc，因为它可能在主窗口中被复用
        }
    }
}