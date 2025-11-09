using System;

namespace Player.Core.Events
{
    /// <summary>
    /// Mock MediaPlayer时间变更事件参数
    /// </summary>
    public class MockMediaPlayerTimeChangedEventArgs : EventArgs
    {
        public long Time { get; }

        public MockMediaPlayerTimeChangedEventArgs(long time)
        {
            Time = time;
        }
    }

    /// <summary>
    /// Mock MediaPlayer长度变更事件参数
    /// </summary>
    public class MockMediaPlayerLengthChangedEventArgs : EventArgs
    {
        public long Length { get; }

        public MockMediaPlayerLengthChangedEventArgs(long length)
        {
            Length = length;
        }
    }

    /// <summary>
    /// Mock MediaPlayer音量变更事件参数
    /// </summary>
    public class MockMediaPlayerVolumeChangedEventArgs : EventArgs
    {
        public float Volume { get; }

        public MockMediaPlayerVolumeChangedEventArgs(float volume)
        {
            Volume = volume;
        }
    }
}