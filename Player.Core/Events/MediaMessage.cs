using Player.Core.Models;
using Player.Core.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Player.Core.Events
{
    /// <summary>
    /// 媒体选择消息
    /// </summary>
    public class MediaSelectedMessage : TypedMessage<MediaItem>
    {
        public MediaSelectedMessage(MediaItem value) : base(value) { }
    }
    
    /// <summary>
    /// 播放状态变更消息
    /// </summary>
    public class PlaybackStateChangedMessage : TypedMessage<bool>
    {
        public PlaybackStateChangedMessage(bool isPlaying) : base(isPlaying) { }
    }
    
    /// <summary>
    /// 进度更新消息
    /// </summary>
    public class ProgressUpdatedMessage : TypedMessage<double>
    {
        public ProgressUpdatedMessage(double progress) : base(progress) { }
    }
    
    /// <summary>
    /// 音量变更消息
    /// </summary>
    public class VolumeChangedMessage : TypedMessage<int>
    {
        public VolumeChangedMessage(int volume) : base(volume) { }
    }
    
    /// <summary>
    /// 全屏状态变更消息
    /// </summary>
    public class FullscreenChangedMessage : TypedMessage<bool>
    {
        public FullscreenChangedMessage(bool isFullscreen) : base(isFullscreen) { }
    }
    
    /// <summary>
    /// 播放列表更新消息
    /// </summary>
    public class PlaylistUpdatedMessage : TypedMessage<List<MediaItem>>
    {
        public PlaylistUpdatedMessage(List<MediaItem> playlist) : base(playlist) { }
    }
    
    /// <summary>
    /// 进度跳转消息
    /// </summary>
    public class SeekMessage : TypedMessage<TimeSpan>
    {
        public SeekMessage(TimeSpan targetTime) : base(targetTime) { }
    }
    
    /// <summary>
    /// 音量变更命令消息
    /// </summary>
    public class ChangeVolumeMessage : TypedMessage<int>
    {
        public ChangeVolumeMessage(int volume) : base(volume) { }
    }
    
    /// <summary>
    /// 静音切换消息
    /// </summary>
    public class ToggleMuteMessage : CommandMessage
    {
        public override string CommandName => "ToggleMute";
        
        public ToggleMuteMessage() : base(true) { }
    }
    
    /// <summary>
    /// 播放状态变更命令消息
    /// </summary>
    public class PlaybackStateCommandMessage : CommandMessage
    {
        public override string CommandName => "PlaybackState";
        
        public PlaybackStateCommandMessage(bool isPlaying) : base(isPlaying) { }
    }
    
    /// <summary>
    /// 播放速度变更消息
    /// </summary>
    public class ChangePlaybackSpeedMessage : TypedMessage<int>
    {
        public ChangePlaybackSpeedMessage(int speedIndex) : base(speedIndex) { }
    }
    
    /// <summary>
    /// 退出全屏消息
    /// </summary>
    public class ExitFullscreenMessage : CommandMessage
    {
        public override string CommandName => "ExitFullscreen";
        
        public ExitFullscreenMessage() : base(true) { }
    }
}