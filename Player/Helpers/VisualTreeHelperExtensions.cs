using System.Windows;
using System.Windows.Media;

namespace Player.Helpers
{
    /// <summary>
    /// 视觉树辅助方法扩展
    /// </summary>
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// 在视觉树中查找指定类型的子元素
        /// </summary>
        /// <typeparam name="T">要查找的元素类型</typeparam>
        /// <param name="parent">父元素</param>
        /// <returns>找到的元素，如果未找到则返回null</returns>
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                
                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>
        /// 在视觉树中查找指定名称的子元素
        /// </summary>
        /// <typeparam name="T">要查找的元素类型</typeparam>
        /// <param name="parent">父元素</param>
        /// <param name="childName">子元素名称</param>
        /// <returns>找到的元素，如果未找到则返回null</returns>
        public static T? FindVisualChildByName<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t && t.Name == childName)
                    return t;
                
                T? childOfChild = FindVisualChildByName<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>
        /// 查找指定元素的父元素
        /// </summary>
        /// <typeparam name="T">要查找的父元素类型</typeparam>
        /// <param name="child">子元素</param>
        /// <returns>找到的父元素，如果未找到则返回null</returns>
        public static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T t)
                    return t;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}