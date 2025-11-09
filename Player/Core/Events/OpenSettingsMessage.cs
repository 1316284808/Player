using Player.Core.Events;

namespace Player.Core.Events
{
    /// <summary>
    /// 打开设置对话框的消息
    /// 
    /// MVVM架构说明：
    /// - 用于ViewModel之间通信的消息类
    /// - 当LeftViewModel中的OpenSettings命令被执行时发送此消息
    /// - MainViewModel负责监听并显示SettingsDialog
    /// </summary>
    public class OpenSettingsMessage
    {
        // 消息不需要额外的参数
    }
}
