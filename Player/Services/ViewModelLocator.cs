using System;
using Microsoft.Extensions.DependencyInjection;
using Player.ViewModels;

namespace Player.Services
{
    /// <summary>
    /// ViewModel定位器 - 简化XAML中的数据上下文绑定
    /// </summary>
    public class ViewModelLocator
    {
        private static IServiceProvider? ServiceProvider => 
            DependencyInjectionService.ServiceProvider ?? throw new InvalidOperationException("ServiceProvider未初始化");

        /// <summary>
        /// MainViewModel实例
        /// </summary>
        public static MainViewModel MainViewModel => 
            ServiceProvider.GetRequiredService<MainViewModel>();

        /// <summary>
        /// LeftViewModel实例
        /// </summary>
        public static LeftViewModel LeftViewModel => 
            ServiceProvider.GetRequiredService<LeftViewModel>();

        /// <summary>
        /// MiddleViewModel实例
        /// </summary>
        public static MiddleViewModel MiddleViewModel => 
            ServiceProvider.GetRequiredService<MiddleViewModel>();

        /// <summary>
        /// BottomViewModel实例
        /// </summary>
        public static BottomViewModel BottomViewModel => 
            ServiceProvider.GetRequiredService<BottomViewModel>();

        /// <summary>
        /// SettingsViewModel实例
        /// </summary>
        public static SettingsViewModel SettingsViewModel => 
            ServiceProvider.GetRequiredService<SettingsViewModel>();

        /// <summary>
        /// 获取ViewModel实例（泛型版本）
        /// </summary>
        public static T GetViewModel<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 动态获取ViewModel类型
        /// </summary>
        public static object? GetViewModel(Type viewModelType)
        {
            return ServiceProvider.GetService(viewModelType);
        }
    }
}