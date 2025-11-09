using System;
using System.ComponentModel;

namespace Player.ViewModels
{
    /// <summary>
    /// ViewModel基类接口 - 增强可测试性
    /// </summary>
    public interface IViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 是否正在加载
        /// </summary>
        bool IsLoading { get; set; }

        /// <summary>
        /// 是否有错误
        /// </summary>
        bool HasError { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        string? ErrorMessage { get; set; }

        /// <summary>
        /// 初始化ViewModel
        /// </summary>
        void Initialize();

        /// <summary>
        /// 异步初始化ViewModel
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 重置ViewModel状态
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 支持导航的ViewModel接口
    /// </summary>
    public interface INavigableViewModel : IViewModelBase
    {
        /// <summary>
        /// 导航到当前ViewModel
        /// </summary>
        /// <param name="parameter">导航参数</param>
        void OnNavigatedTo(object? parameter);

        /// <summary>
        /// 从当前ViewModel导航离开
        /// </summary>
        void OnNavigatedFrom();
    }
}