using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Player.Core.Messaging
{
    /// <summary>
    /// 消息传递基类，提供统一的IMessenger接口、类型约束和属性通知功能
    /// </summary>
    public abstract class MessengerBase : ObservableObject
    {
        /// <summary>
        /// 默认的消息传递器实例
        /// </summary>
        protected static readonly IMessenger DefaultMessenger = WeakReferenceMessenger.Default;

        /// <summary>
        /// 当前实例使用的消息传递器
        /// </summary>
        protected readonly IMessenger Messenger;

        /// <summary>
        /// 使用默认消息传递器初始化
        /// </summary>
        protected MessengerBase() : this(DefaultMessenger)
        {
        }

        /// <summary>
        /// 使用指定消息传递器初始化
        /// </summary>
        /// <param name="messenger">消息传递器实例</param>
        protected MessengerBase(IMessenger messenger)
        {
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        }

        /// <summary>
        /// 发送强类型消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="message">消息实例</param>
        protected void SendMessage<TMessage>(TMessage message) where TMessage : class
        {
            Messenger.Send(message);
        }

        /// <summary>
        /// 发送值变更消息
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="value">变更的值</param>
        protected void SendValueChangedMessage<TValue>(TValue value)
        {
            var message = new ValueChangedMessage<TValue>(value);
            Messenger.Send(message);
        }

        /// <summary>
        /// 注册消息接收器
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <param name="handler">消息处理程序</param>
        protected void RegisterMessageHandler<TMessage>(
            object recipient, 
            MessageHandler<object, TMessage> handler) 
            where TMessage : class
        {
            Messenger.Register(recipient, handler);
        }

        /// <summary>
        /// 取消注册消息接收器
        /// </summary>
        /// <param name="recipient">接收器实例</param>
        protected void UnregisterMessageHandler(object recipient)
        {
            Messenger.UnregisterAll(recipient);
        }

        /// <summary>
        /// 检查指定接收器是否已注册接收指定类型的消息
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="recipient">接收器实例</param>
        /// <returns>是否已注册</returns>
        protected bool HasActiveRecipients<TMessage>(object recipient) where TMessage : class
        {
            return Messenger.IsRegistered<TMessage>(recipient);
        }
    }

    /// <summary>
    /// 强类型消息基类
    /// </summary>
    /// <typeparam name="T">消息数据类型</typeparam>
    public abstract class TypedMessage<T> : ValueChangedMessage<T>
    {
        /// <summary>
        /// 消息类型标识
        /// </summary>
        public virtual string MessageType => GetType().Name;

        /// <summary>
        /// 消息时间戳
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        protected TypedMessage(T value) : base(value)
        {
        }
    }

    /// <summary>
    /// 命令消息基类
    /// </summary>
    public abstract class CommandMessage : ValueChangedMessage<bool>
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public abstract string CommandName { get; }

        /// <summary>
        /// 命令执行是否成功
        /// </summary>
        public bool IsSuccessful => Value;

        protected CommandMessage(bool isSuccessful) : base(isSuccessful)
        {
        }
    }
}