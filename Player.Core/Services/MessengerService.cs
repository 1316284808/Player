using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Player.Core.Services
{
    /// <summary>
    /// 消息通信服务实现
    /// 提供统一的ViewModel间通信能力
    /// </summary>
    public class MessengerService : IMessengerService
    {
        private readonly IMessenger _messenger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="messenger">底层消息传递器实例</param>
        public MessengerService(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        }

        /// <summary>
        /// 发送强类型消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        public void Send<TMessage>(TMessage message) where TMessage : class
        {
            _messenger.Send(message);
        }

        /// <summary>
        /// 发送值变更消息
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="value">变更的值</param>
        public void SendValueChanged<TValue>(TValue value)
        {
            var message = new ValueChangedMessage<TValue>(value);
            _messenger.Send(message);
        }

        /// <summary>
        /// 注册消息接收器
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <param name="handler">消息处理程序</param>
        public void Register<TMessage>(object recipient, MessageHandler<object, TMessage> handler) where TMessage : class
        {
            _messenger.Register(recipient, handler);
        }

        /// <summary>
        /// 取消注册消息接收器
        /// </summary>
        /// <param name="recipient">接收器实例</param>
        public void UnregisterAll(object recipient)
        {
            _messenger.UnregisterAll(recipient);
        }

        /// <summary>
        /// 检查指定接收器是否已注册接收指定类型的消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <returns>是否已注册</returns>
        public bool IsRegistered<TMessage>(object recipient) where TMessage : class
        {
            return _messenger.IsRegistered<TMessage>(recipient);
        }
    }
}