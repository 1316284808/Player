using System;
using System.Windows.Input;

namespace Player.Core.Commands
{
    /// <summary>
    /// 简单的命令实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">要执行的动作</param>
        /// <param name="canExecute">判断是否可以执行的函数</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 确定命令是否可以在当前状态下执行
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// 在调用命令时执行
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// 引发CanExecuteChanged事件，通知命令状态已更改
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}