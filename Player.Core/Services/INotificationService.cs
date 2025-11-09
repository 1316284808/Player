using System;

namespace Player.Core.Services
{
    /// <summary>
    /// 通知服务接口 - 用于解耦平台特定的通知实现
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 显示信息类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        void ShowInfo(string message);

        /// <summary>
        /// 显示成功类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        void ShowSuccess(string message);

        /// <summary>
        /// 显示警告类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        void ShowWarning(string message);

        /// <summary>
        /// 显示错误类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        void ShowError(string message);
    }
}