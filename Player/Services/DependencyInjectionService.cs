using System;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Player.Core.Repositories;
using Player.Core.Services;
using Player.ViewModels;

namespace Player.Services
{
    /// <summary>
    /// 依赖注入服务配置
    /// </summary>
    public static class DependencyInjectionService
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 初始化依赖注入容器
        /// </summary>
        public static IServiceProvider Initialize()
        {
            if (_serviceProvider != null)
                return _serviceProvider;

            var services = new ServiceCollection();

            // 注册消息服务
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            services.AddSingleton<IMessengerService>(provider => 
                new MessengerService(provider.GetRequiredService<IMessenger>()));
            
            // 注册Repository服务
            services.AddSingleton<IMediaRepository, MediaRepository>();
            
            // 注册播放器服务
            services.AddSingleton<IVlcPlayerService, VlcPlayerService>();
            services.AddSingleton<Player.Core.Models.PlaybackState>();
            
            // 注册主题服务
            services.AddSingleton<Player.Services.IThemeService, ThemeService>();
            
            // 注册通知服务
            services.AddSingleton<INotificationService, WpfNotificationService>();
            
            // 注册对话框服务
            services.AddSingleton<IDialogService, DialogService>();
            
            // 全屏功能现在使用WPF布局实现的伪全屏，不再需要自定义全屏服务
            
            // 注册新增服务
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<DisposableManager>();

            // 全屏功能现在使用VLC原生实现，不再需要自定义全屏服务
            
            // 注册ViewModel - 使用依赖注入容器自动管理生命周期
            services.AddTransient<MainViewModel>();
            services.AddTransient<BottomViewModel>();
            services.AddTransient<MiddleViewModel>();
            services.AddTransient<LeftViewModel>();
            services.AddTransient<SettingsViewModel>();

            _serviceProvider = services.BuildServiceProvider();
            return _serviceProvider;
        }

        /// <summary>
        /// 获取服务提供者实例
        /// </summary>
        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    throw new InvalidOperationException("Dependency injection container has not been initialized. Call Initialize() first.");
                
                return _serviceProvider;
            }
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 获取服务实例（可选）
        /// </summary>
        public static T? GetServiceOptional<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 获取ViewModel实例
        /// </summary>
        public static T GetViewModel<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 获取ViewModel实例（可选）
        /// </summary>
        public static T? GetViewModelOptional<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// 创建新的服务范围（用于Scoped服务）
        /// </summary>
        public static IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }
    }
}