using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Player.Core.Services
{
    /// <summary>
    /// 消息通信服务接口
    /// 提供统一的ViewModel间通信能力
    /// </summary>
    public interface IMessengerService
    {
        /// <summary>
        /// 发送强类型消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        void Send<TMessage>(TMessage message) where TMessage : class;

        /// <summary>
        /// 发送值变更消息
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="value">变更的值</param>
        void SendValueChanged<TValue>(TValue value);

        /// <summary>
        /// 注册消息接收器
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <param name="handler">消息处理程序</param>
        void Register<TMessage>(object recipient, MessageHandler<object, TMessage> handler) where TMessage : class;

        /// <summary>
        /// 取消注册消息接收器
        /// </summary>
        /// <param name="recipient">接收器实例</param>
        void UnregisterAll(object recipient);

        /// <summary>
        /// 检查指定接收器是否已注册接收指定类型的消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <returns>是否已注册</returns>
        bool IsRegistered<TMessage>(object recipient) where TMessage : class;
    }
}