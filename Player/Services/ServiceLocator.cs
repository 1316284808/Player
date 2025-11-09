using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Player.Core.Repositories;
using Player.Core.Services;

namespace Player.Services
{
    /// <summary>
    /// 服务定位器，提供依赖注入配置
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化服务提供者
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;
            
            // 注册消息服务
            Register<IMessenger>(WeakReferenceMessenger.Default);
            Register<IMessengerService>(new MessengerService(WeakReferenceMessenger.Default));
            
            // 注册Repository服务
            Register<IMediaRepository>(new MediaRepository());
            
            // 注册其他服务
            Register<IVlcPlayerService>(new VlcPlayerService());
            Register<Player.Core.Models.PlaybackState>(new Player.Core.Models.PlaybackState());
            Register<Player.Services.IThemeService>(new ThemeService());
            
            // 注册通知服务
            Register<INotificationService>(new WpfNotificationService());
            
            // 注册对话框服务
            Register<IDialogService>(new DialogService());
            
            _isInitialized = true;
        }

        /// <summary>
        /// 注册服务实例
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="service">服务实例</param>
        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例</returns>
        public static T GetService<T>() where T : class
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize() first.");
            }
            
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        /// <summary>
        /// 获取服务实例（可选）
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例或null</returns>
        public static T? GetServiceOptional<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            return null;
        }
    }
}