using System;

namespace Player.Core.Models
{
    /// <summary>
    /// 应用程序设置
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 最大历史记录项数
        /// </summary>
        public int MaxHistoryItems { get; set; } = 50;

        /// <summary>
        /// 是否自动播放下一个
        /// </summary>
        public bool AutoPlayNext { get; set; } = true;

        /// <summary>
        /// 是否记住播放位置
        /// </summary>
        public bool RememberPosition { get; set; } = true;

        /// <summary>
        /// 是否显示字幕
        /// </summary>
        public bool ShowSubtitles { get; set; } = true;

        /// <summary>
        /// 默认音量
        /// </summary>
        public int DefaultVolume { get; set; } = 80;

        /// <summary>
        /// 是否启用硬件解码
        /// </summary>
        public bool EnableHardwareDecoding { get; set; } = true;

        /// <summary>
        /// 是否启用代理
        /// </summary>
        public bool ProxyEnabled { get; set; } = false;

        /// <summary>
        /// 代理服务器
        /// </summary>
        public string ProxyServer { get; set; } = "";

        /// <summary>
        /// 代理端口
        /// </summary>
        public int ProxyPort { get; set; } = 0;

        /// <summary>
        /// 代理用户名
        /// </summary>
        public string ProxyUsername { get; set; } = "";

        /// <summary>
        /// 代理密码
        /// </summary>
        public string ProxyPassword { get; set; } = "";

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMuted { get; set; } = false;
    }

    /// <summary>
    /// 主题设置
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>
        /// 主色
        /// </summary>
        public string PrimaryColor { get; set; } = "#3F51B5";

        /// <summary>
        /// 次要色
        /// </summary>
        public string SecondaryColor { get; set; } = "#757575";

        /// <summary>
        /// 强调色
        /// </summary>
        public string AccentColor { get; set; } = "#FF4081";

        /// <summary>
        /// 背景刷
        /// </summary>
        public string BackgroundBrush { get; set; } = "#212121";

        /// <summary>
        /// 前景刷
        /// </summary>
        public string ForegroundBrush { get; set; } = "#FFFFFF";

        /// <summary>
        /// 次要刷
        /// </summary>
        public string SecondaryBrush { get; set; } = "#303030";

        /// <summary>
        /// 次要亮色刷
        /// </summary>
        public string SecondaryLightBrush { get; set; } = "#424242";

        /// <summary>
        /// 当前主题
        /// </summary>
        public string CurrentTheme { get; set; } = "BlueTheme";
    }
    
    /// <summary>
    /// 硬件设置
    /// </summary>
    public class HardwareSettings
    {
        /// <summary>
        /// GPU 加速是否启用（默认关闭）
        /// </summary>
        public bool EnableGPUAcceleration { get; set; } = false;

        /// <summary>
        /// 硬件解码器选择
        /// </summary>
        public string HardwareDecoder { get; set; } = "d3d11va";

        /// <summary>
        /// 超分辨率是否启用（默认关闭）
        /// </summary>
        public bool EnableSuperResolution { get; set; } = false;

        /// <summary>
        /// 超分辨率级别
        /// </summary>
        public int SuperResolutionLevel { get; set; } = 2;

        /// <summary>
        /// 视频渲染器
        /// </summary>
        public string VideoRenderer { get; set; } = "direct3d11";

        /// <summary>
        /// 使用系统内存避免显存问题
        /// </summary>
        public bool UseSystemMemory { get; set; } = true;

        /// <summary>
        /// 启用硬件YUV转换
        /// </summary>
        public bool EnableHardwareYUV { get; set; } = true;

        /// <summary>
        /// 跳过环路滤波提高性能
        /// </summary>
        public bool SkipLoopFilter { get; set; } = true;

        /// <summary>
        /// 启用快速解码模式
        /// </summary>
        public bool EnableFastDecoding { get; set; } = true;

        /// <summary>
        /// 不丢弃延迟的帧
        /// </summary>
        public bool NoDropLateFrames { get; set; } = true;

        /// <summary>
        /// 不跳过帧
        /// </summary>
        public bool NoSkipFrames { get; set; } = true;

        /// <summary>
        /// 文件缓存时间（毫秒）
        /// </summary>
        public int FileCaching { get; set; } = 1000;

        /// <summary>
        /// 网络缓存时间（毫秒）
        /// </summary>
        public int NetworkCaching { get; set; } = 2000;

        /// <summary>
        /// 使用自动线程数
        /// </summary>
        public bool AutoThreads { get; set; } = true;

        /// <summary>
        /// 启用快速解码
        /// </summary>
        public bool EnableHurryUpDecoding { get; set; } = true;

        /// <summary>
        /// 启用去隔行扫描
        /// </summary>
        public bool EnableDeinterlace { get; set; } = true;

        /// <summary>
        /// 去隔行扫描模式
        /// </summary>
        public string DeinterlaceMode { get; set; } = "blend";
    }
}