using System;
using System.ComponentModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Player.Core.Events;
using Player.Core.Services;
using Player.Services;

namespace Player.ViewModels
{
    /// <summary>
    /// 底部控制栏视图模型 (BottomViewModel)
    /// 
    /// MVVM模式说明：
    /// - 负责底部播放控制栏的逻辑
    /// - 处理播放控制、进度条、音量控制等用户交互
    /// - 依赖共享的PlaybackState进行状态同步
    /// 
    /// 主要职责：
    /// 1. 管理播放控制按钮（播放/暂停/停止）
    /// 2. 处理进度条拖拽和跳转
    /// 3. 管理音量控制和静音
    /// 4. 显示播放时间和进度信息
    /// </summary>
    public partial class BottomViewModel : ObservableObject, IDisposable
    {
        // 依赖注入的服务接口
        private readonly Player.Core.Models.PlaybackState _playbackState;  // 共享的播放状态
        private readonly IMessengerService _messengerService;              // 消息通信服务
        // 全屏功能现在使用WPF布局实现的伪全屏，不再需要FullscreenService

        // 拖拽状态标记
        private bool _isDragging = false;
        private bool _wasPlayingBeforeDrag = false;  // 拖拽前的播放状态

        // 私有字段：仅用于 UI 拖拽时临时显示
        [ObservableProperty]
        private double _dragProgress;

        // 公共只读属性：用于正常播放时的进度显示
        public double DisplayProgress => 
            _playbackState.TotalDuration > 0 
                ? (_playbackState.PlaybackTime / (double)_playbackState.TotalDuration) * 100 
                : 0;

        // 兼容性属性（保持现有绑定）
        public double Progress
        {
            get => DisplayProgress;
            set
            {
                // 将百分比转换为时间并发送跳转消息
                if (_playbackState.TotalDuration > 0)
                {
                    var targetTimeMs = (long)(_playbackState.TotalDuration * value / 100);
                    var targetTime = TimeSpan.FromMilliseconds(targetTimeMs);
                    _messengerService.Send(new SeekMessage(targetTime));
                }
            }
        }

        [ObservableProperty]
        private string _timeDisplay = "00:00 / 00:00";         // 时间显示文本

        [ObservableProperty]
        private string _volumeIcon = "VolumeHigh";             // 音量图标名称
          
        // 音量属性 - 支持双向绑定
        public int Volume
        {
            get => _playbackState.Volume;
            set
            {
                if (_playbackState.Volume != value)
                {
                    _playbackState.Volume = value;
                    // 通过消息通知其他模块（如 MiddleViewModel）音量已改变
                    _messengerService.Send(new ChangeVolumeMessage(value));
                    UpdateVolumeIcon(value); // 直接更新图标
                    OnPropertyChanged(nameof(Volume)); // 通知UI更新绑定
                }
            }
        }
        
        // 播放状态属性 - 用于播放按钮图标绑定
        public bool IsPlaying
        {
            get => _playbackState.IsPlaying;
        }

        // 全屏状态属性 - 用于全屏按钮图标绑定
        public bool IsFullscreen
        {
            get => _playbackState.IsFullscreen;
            set
            {
                if (_playbackState.IsFullscreen != value)
                {
                    _playbackState.IsFullscreen = value;
                    OnPropertyChanged();
                }
            }
        }

        // 暴露PlaybackState属性以支持XAML绑定
        public Player.Core.Models.PlaybackState PlaybackState => _playbackState;
        
        // 播放速度选择索引属性 - 用于ComboBox的双向绑定
        private int _selectedSpeedIndex = 1;
        public int SelectedSpeedIndex 
        {
            get => _selectedSpeedIndex;
            set 
            {
                if (_selectedSpeedIndex != value)
                {
                    _selectedSpeedIndex = value;
                    // 当速度索引变更时自动执行命令（仅在非初始化时）
                    if (_isInitialized)
                    {
                        // 直接传递索引值给命令
                        ChangePlaybackSpeedCommand.Execute(value);
                    }
                    OnPropertyChanged(nameof(SelectedSpeedIndex));
                }
            }
        }
        
        // 根据索引获取速度值的方法
        private double GetSpeedFromIndex(int index)
        {
            return index switch
            {
                0 => 0.5,
                1 => 1.0,
                2 => 1.5,
                3 => 2.0,
                _ => 1.0 // 默认值
            };
        }
        
        // 根据速度值获取索引的方法
        private int GetIndexFromSpeed(double speed)
        {
            return speed switch
            {
                0.5 => 0,
                1.0 => 1,
                1.5 => 2,
                2.0 => 3,
                _ => 1 // 默认索引
            };
        }
        
        // 标记是否已初始化，避免初始化时触发命令
        private bool _isInitialized = false;

        public TimeSpan CurrentTime => TimeSpan.FromMilliseconds(_playbackState.PlaybackTime);
        public TimeSpan TotalTime => TimeSpan.FromMilliseconds(_playbackState.TotalDuration);

        /// <summary>
        /// 构造函数 - 依赖注入模式
        /// </summary>
        /// <param name="playbackState">共享的播放状态对象</param>
        /// <param name="messengerService">消息通信服务</param>
        /// <param name="fullscreenService">全屏服务</param>
        public BottomViewModel(Player.Core.Models.PlaybackState playbackState, IMessengerService messengerService)
        {
            _playbackState = playbackState;
            _messengerService = messengerService ?? throw new ArgumentNullException(nameof(messengerService));

            // 订阅消息 - 监听进度更新消息
            _messengerService.Register<ProgressUpdatedMessage>(this, OnProgressUpdatedMessage);

            // 订阅播放状态变更消息
            _messengerService.Register<PlaybackStateChangedMessage>(this, OnPlaybackStateChangedMessage);
            
            // 初始化选中速度为默认显示1.0（对应索引1）
            SelectedSpeedIndex = 1; // 通过属性setter设置，会触发属性变更通知

            // 初始化音量图标
            UpdateVolumeIcon(_playbackState.Volume);

            // 初始化时间显示
            UpdateTimeDisplay();
            
            // 标记初始化完成
            _isInitialized = true;
        }

        #region RelayCommands

        [RelayCommand]
        private void TogglePlayPause()
        {
            _messengerService.Send(new PlaybackStateCommandMessage(!_playbackState.IsPlaying));
        }

        [RelayCommand]
        private void ToggleFullscreen()
        {
            // 通过消息系统通知全屏状态变化
            bool newFullscreenState = !_playbackState.IsFullscreen;
            System.Diagnostics.Debug.WriteLine($"BottomViewModel发送全屏状态变化消息: {newFullscreenState}");
            _messengerService.Send(new FullscreenChangedMessage(newFullscreenState));
            
            // 同时更新本地状态
            IsFullscreen = newFullscreenState;
        }

        [RelayCommand]
        private void ToggleMute()
        {
            _messengerService.Send(new ToggleMuteMessage());
            //更新静音图标
            UpdateVolumeIcon(Volume);
            OnPropertyChanged(nameof(Volume));
        }

        [RelayCommand]
        private void Seek(double position)
        {
            if (_playbackState.TotalDuration > 0)
            {
                var targetTimeMs = (long)(_playbackState.TotalDuration * position / 100);
                var targetTime = TimeSpan.FromMilliseconds(targetTimeMs);
                _messengerService.Send(new SeekMessage(targetTime));
            }
        }

        [RelayCommand]
        private void ChangeVolume(int newVolume)
        {
            _messengerService.Send(new ChangeVolumeMessage(newVolume));
        }

        [RelayCommand]
        private void ChangePlaybackSpeed(int speedIndex)
        {
            _messengerService.Send(new ChangePlaybackSpeedMessage(speedIndex));
        }

        #region Drag Commands

        [RelayCommand]
        private void BeginDragProgress()
        {
            // 记录拖拽前的播放状态
            _wasPlayingBeforeDrag = _playbackState.IsPlaying;

            // 用户开始拖拽时，设置拖拽标记
            _isDragging = true;

            // 初始化拖拽进度为当前显示进度
            DragProgress = DisplayProgress;

            // 如果正在播放，暂停播放以保持拖拽位置
            if (_playbackState.IsPlaying)
            {
                _messengerService.Send(new PlaybackStateCommandMessage(false));
            }
        }

        [RelayCommand]
        private void EndDragProgress(double finalPosition)
        {
            // 用户结束拖拽时，执行跳转
            if (_playbackState.TotalDuration > 0)
            {
                var targetTimeMs = (long)(_playbackState.TotalDuration * finalPosition / 100);
                var targetTime = TimeSpan.FromMilliseconds(targetTimeMs);

                // 发送跳转消息
                _messengerService.Send(new SeekMessage(targetTime));

                // 立即更新UI显示
                OnPropertyChanged(nameof(DisplayProgress));
                UpdateTimeDisplay();
            }

            // 重置拖拽状态，允许后续进度更新
            _isDragging = false;

            // 如果拖拽前正在播放，拖拽结束后恢复播放
            if (_wasPlayingBeforeDrag)
            {
                _messengerService.Send(new PlaybackStateCommandMessage(true));
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        private void OnProgressUpdatedMessage(object recipient, ProgressUpdatedMessage message)
        {
            // 在拖拽期间忽略进度更新，避免冲突
            if (_isDragging)
                return;

            // 更新播放时间（将0-1范围的进度值转换为毫秒）
            if (_playbackState.TotalDuration > 0)
            {
                _playbackState.PlaybackTime = (long)(message.Value * _playbackState.TotalDuration);
            }

            // 通知UI更新进度显示和滑块位置
            OnPropertyChanged(nameof(DisplayProgress)); // 通知 UI 更新显示进度
            
            // 同时更新拖拽进度，确保滑块跟随播放进度
            DragProgress = DisplayProgress;
            
            UpdateTimeDisplay();
        }

        private void OnPlaybackStateChangedMessage(object recipient, PlaybackStateChangedMessage message)
        {
            OnPropertyChanged(nameof(_playbackState.IsPlaying));
            OnPropertyChanged(nameof(_playbackState.IsMuted));
            
            // 确保SelectedSpeedIndex与PlaybackRate同步（仅在播放状态变化时）
            // 注意：这里只同步，不覆盖用户选择的默认值
            if (_playbackState.PlaybackRate > 0)
            {
                int newIndex = GetIndexFromSpeed(_playbackState.PlaybackRate);
                if (_selectedSpeedIndex != newIndex)
                {
                    _selectedSpeedIndex = newIndex;
                    OnPropertyChanged(nameof(SelectedSpeedIndex));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 更新音量图标
        /// </summary>
        /// <param name="volume">音量值</param>
        private void UpdateVolumeIcon(int volume)
        {
            if (_playbackState.IsMuted || volume == 0)
            {
                VolumeIcon = "VolumeOff";
            }
            else if (volume < 33)
            {
                VolumeIcon = "VolumeLow";
            }
            else if (volume < 66)
            {
                VolumeIcon = "VolumeMedium";
            }
            else
            {
                VolumeIcon = "VolumeHigh";
            }
        }

        /// <summary>
        /// 更新时间显示
        /// </summary>
        private void UpdateTimeDisplay()
        {
            TimeSpan currentTimeSpan = CurrentTime;
            TimeSpan totalTimeSpan = TotalTime;
            
            // 当播放接近结束时（差值小于1秒），显示总时长作为当前时间
            if (totalTimeSpan.TotalSeconds > 0 && 
                (totalTimeSpan - currentTimeSpan).TotalMilliseconds < 1000)
            {
                currentTimeSpan = totalTimeSpan;
            }
            
            TimeDisplay = $"{FormatTime(currentTimeSpan)} / {FormatTime(totalTimeSpan)}";
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="timeSpan">时间跨度</param>
        /// <returns>格式化的时间字符串</returns>
        private string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
                return timeSpan.ToString(@"h\:mm\:ss");
            else
                return timeSpan.ToString(@"mm\:ss");
        }

        /// <summary>
        /// 更新播放进度
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="totalTime">总时长</param>
        public void UpdateProgress(TimeSpan currentTime, TimeSpan totalTime)
        {
            if (totalTime.TotalSeconds <= 0)
                return;

            _playbackState.PlaybackTime = Convert.ToInt64(currentTime.TotalMilliseconds);
            _playbackState.TotalDuration = Convert.ToInt64(totalTime.TotalMilliseconds);

            UpdateTimeDisplay();
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messengerService.UnregisterAll(this);
                }
                _disposed = true;
            }
        }

        ~BottomViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}