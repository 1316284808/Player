using System.Windows;
using Microsoft.Xaml.Behaviors;
using Player.ViewModels;

namespace Player.Behaviors
{
    /// <summary>
    /// VLC初始化行为 - 将VLC播放器初始化逻辑从代码隐藏移至XAML
    /// </summary>
    public class VlcInitializationBehavior : Behavior<FrameworkElement>
    {
        private MiddleViewModel? _viewModel;

        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
            
            base.OnDetaching();
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = AssociatedObject.DataContext as MiddleViewModel;
            if (_viewModel != null)
            {
                _viewModel.InitializeVlc();
            }
        }

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            // 清理逻辑由ViewModel的Dispose模式处理
            _viewModel = null;
        }
    }
}