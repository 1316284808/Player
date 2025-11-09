using Player.Core.Services;
using Player.Helpers;

namespace Player.Services
{
    /// <summary>
    /// WPF平台的通知服务实现
    /// </summary>
    public class WpfNotificationService : INotificationService
    {
        /// <summary>
        /// 显示信息类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public void ShowInfo(string message)
        {
            SystemNotificationHelper.ShowInfo(message);
        }

        /// <summary>
        /// 显示成功类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public void ShowSuccess(string message)
        {
            SystemNotificationHelper.ShowSuccess(message);
        }

        /// <summary>
        /// 显示警告类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public void ShowWarning(string message)
        {
            SystemNotificationHelper.ShowWarning(message);
        }

        /// <summary>
        /// 显示错误类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public void ShowError(string message)
        {
            SystemNotificationHelper.ShowError(message);
        }
    }
}