using System.Windows.Controls;
using System.Windows;  // 添加Slider控件所需的命名空间
using Player.Core.Enums;

namespace Player.Helpers
{
    /// <summary>
    /// UI控件管理器，集中管理播放器中的各种UI控件状态
    /// 包括播放/暂停图标、音量图标、音量条和进度条等
    /// 确保整个项目统一使用这一个类来管理所有UI控件状态
    /// </summary>
    public static class UIControlManager
    {
        #region 播放/暂停按钮管理
        /// <summary>
        /// 设置播放按钮为播放图标(▶)
        /// </summary>
        /// <param name="playPauseButton">播放/暂停按钮</param>
        public static void SetPlayButtonToPlayIcon(Button playPauseButton)
        {
            if (playPauseButton != null && playPauseButton.Content is CustomIcon icon)
            {
                icon.Kind = IconKind.Play; // 确保显示播放图标
            }
        }

        /// <summary>
        /// 设置播放按钮为暂停图标(⏸)
        /// </summary>
        /// <param name="playPauseButton">播放/暂停按钮</param>
        public static void SetPlayButtonToPauseIcon(Button playPauseButton)
        {
            if (playPauseButton != null && playPauseButton.Content is CustomIcon icon)
            {
                icon.Kind = IconKind.Pause; // 确保显示暂停图标
            }
        }

        /// <summary>
        /// 根据播放状态更新播放/暂停图标
        /// </summary>
        /// <param name="playPauseButton">播放/暂停按钮</param>
        /// <param name="isPlaying">是否正在播放</param>
        public static void UpdatePlayIcon(Button playPauseButton, bool isPlaying)
        {
            if (playPauseButton != null && playPauseButton.Content is CustomIcon icon)
            {
                icon.Kind = isPlaying ? IconKind.Pause : IconKind.Play;
            }
        }
        #endregion

        #region 音量图标管理
        /// <summary>
        /// 根据音量值更新音量图标
        /// </summary>
        /// <param name="muteButton">静音按钮</param>
        /// <param name="volume">音量值（0-100）</param>
        /// <param name="isMuted">是否静音</param>
        public static void UpdateVolumeIcon(Button muteButton, double volume, bool isMuted = false)
        {
            if (muteButton == null || !(muteButton.Content is CustomIcon icon))
                return;

            // 如果静音或音量为0，显示静音图标
            if (isMuted || volume <= 0)
            {
                icon.Kind = IconKind.VolumeMute;
            }
            else if (volume < 33)
            {
                icon.Kind = IconKind.VolumeLow;
            }
            else if (volume < 66)
            {
                icon.Kind = IconKind.VolumeMedium;
            }
            else
            {
                icon.Kind = IconKind.VolumeHigh;
            }
        }
        #endregion

        #region 音量条管理
        /// <summary>
        /// 设置音量条的值
        /// </summary>
        /// <param name="volumeSlider">音量滑块</param>
        /// <param name="volume">音量值（0-100）</param>
        public static void SetVolumeSliderValue(Slider volumeSlider, double volume)
        {
            if (volumeSlider == null) return;

            // 确保音量在0-100范围内
            volume = Math.Max(0, Math.Min(100, volume));
            volumeSlider.Value = volume;
        }

        /// <summary>
        /// 静音/取消静音音量条
        /// </summary>
        /// <param name="volumeSlider">音量滑块</param>
        /// <param name="isMuted">是否静音</param>
        /// <param name="volumeBeforeMute">静音前的音量值</param>
        public static void MuteVolumeSlider(Slider volumeSlider, bool isMuted, ref double volumeBeforeMute)
        {
            if (volumeSlider == null) return;

            if (isMuted)
            {
                // 静音时保存当前音量
                if (volumeSlider.Value > 0)
                {
                    volumeBeforeMute = volumeSlider.Value;
                }
                volumeSlider.Value = 0;
            }
            else
            {
                // 取消静音时恢复之前的音量
                volumeSlider.Value = volumeBeforeMute;
            }
        }
        #endregion

        #region 进度条管理
        /// <summary>
        /// 设置进度条的值
        /// </summary>
        /// <param name="progressBar">进度条</param>
        /// <param name="value">进度值（0-100）</param>
        public static void SetProgressBarValue(Slider progressBar, double value)
        {
            if (progressBar == null) return;

            // 确保进度值在0-100范围内
            value = Math.Max(0, Math.Min(100, value));
            progressBar.Value = value;
        }

        /// <summary>
        /// 更新进度条和时间显示
        /// </summary>
        /// <param name="progressBar">进度条</param>
        /// <param name="timeDisplay">时间显示文本块</param>
        /// <param name="currentTime">当前时间（毫秒）</param>
        /// <param name="totalTime">总时间（毫秒）</param>
        public static void UpdateProgressAndTime(Slider progressBar, TextBlock timeDisplay, long currentTime, long totalTime)
        {
            // 更新进度条
            if (totalTime > 0 && progressBar != null)
            {
                double progress = (double)currentTime / totalTime * 100;
                SetProgressBarValue(progressBar, progress);
            }

            // 更新时间显示
            if (timeDisplay != null)
            {
                timeDisplay.Text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
            }
        }

        /// <summary>
        /// 格式化时间为MM:SS格式
        /// </summary>
        /// <param name="milliseconds">毫秒数</param>
        /// <returns>格式化后的时间字符串</returns>
        private static string FormatTime(long milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        #endregion
    }
}