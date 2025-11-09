using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Services
{
    /// <summary>
    /// 导航服务实现 - 基于URI的导航管理
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Uri> _viewMapping;
        private readonly Stack<string> _navigationStack;
        private Frame? _navigationFrame;

        public event EventHandler<NavigationEventArgs>? Navigated;

        public NavigationService()
        {
            _viewMapping = new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);
            _navigationStack = new Stack<string>();
            InitializeViewMapping();
        }

        /// <summary>
        /// 设置导航框架
        /// </summary>
        /// <param name="frame">导航框架</param>
        public void SetNavigationFrame(Frame frame)
        {
            _navigationFrame = frame;
        }

        /// <summary>
        /// 初始化视图映射
        /// </summary>
        private void InitializeViewMapping()
        {
            // 注册视图URI映射
            _viewMapping["Main"] = new Uri("/Views/MainView.xaml", UriKind.Relative);
            _viewMapping["Settings"] = new Uri("/Views/SettingsView.xaml", UriKind.Relative);
            // 可以根据需要添加更多视图映射
        }

        /// <summary>
        /// 注册视图
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="viewUri">视图URI</param>
        public void RegisterView(string viewName, Uri viewUri)
        {
            _viewMapping[viewName] = viewUri;
        }

        public void NavigateTo(string viewName, object? parameter = null)
        {
            if (_navigationFrame == null || !_viewMapping.TryGetValue(viewName, out var uri))
                return;

            _navigationStack.Push(viewName);
            _navigationFrame.Navigate(uri, parameter);
            
            Navigated?.Invoke(this, new NavigationEventArgs(viewName, parameter));
        }

        public async Task NavigateToAsync(string viewName, object? parameter = null)
        {
            await Task.Run(() => NavigateTo(viewName, parameter));
        }

        public void GoBack()
        {
            if (_navigationFrame?.CanGoBack == true && _navigationStack.Count > 1)
            {
                _navigationStack.Pop(); // 移除当前视图
                var previousView = _navigationStack.Peek();
                
                if (_viewMapping.TryGetValue(previousView, out var uri))
                {
                    _navigationFrame.GoBack();
                    Navigated?.Invoke(this, new NavigationEventArgs(previousView));
                }
            }
        }

        /// <summary>
        /// 获取当前视图名称
        /// </summary>
        public string CurrentView => _navigationStack.Count > 0 ? _navigationStack.Peek() : string.Empty;

        /// <summary>
        /// 判断是否可以返回
        /// </summary>
        public bool CanGoBack => _navigationStack.Count > 1;
    }
}