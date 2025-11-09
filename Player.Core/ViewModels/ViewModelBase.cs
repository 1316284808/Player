using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Player.Core.Messaging;

namespace Player.Core.ViewModels
{
    /// <summary>
    /// ViewModel基类，提供属性通知功能和统一的消息传递机制
    /// </summary>
    public class ViewModelBase : MessengerBase
    {
        /// <summary>
        /// 属性更改事件
        /// </summary>
        public new event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// 触发属性更改通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// 设置属性值并触发通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>值是否更改</returns>
        protected new bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}