using System;
using System.Threading.Tasks;

namespace Player.Services
{
    /// <summary>
    /// 导航服务接口 - 统一管理视图切换
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 导航到指定视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="parameter">导航参数</param>
        void NavigateTo(string viewName, object? parameter = null);

        /// <summary>
        /// 异步导航到指定视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="parameter">导航参数</param>
        Task NavigateToAsync(string viewName, object? parameter = null);

        /// <summary>
        /// 返回上一视图
        /// </summary>
        void GoBack();

        /// <summary>
        /// 导航事件
        /// </summary>
        event EventHandler<NavigationEventArgs>? Navigated;
    }

    /// <summary>
    /// 导航事件参数
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; }
        public object? Parameter { get; }

        public NavigationEventArgs(string viewName, object? parameter = null)
        {
            ViewName = viewName;
            Parameter = parameter;
        }
    }
}