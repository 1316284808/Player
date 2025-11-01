using System;
using System.Windows;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace Player.Helpers
{
    /// <summary>
    /// 系统通知帮助类，使用Hardcodet.NotifyIcon.Wpf库实现任务栏通知
    /// </summary>
    public static class SystemNotificationHelper
    {
        // 单例的任务栏通知图标
        private static TaskbarIcon _taskbarIcon;
        private static bool _initialized = false;

        /// <summary>
        /// 初始化通知系统
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // 确保只初始化一次
                if (!_initialized)
                {
                    // 确保在UI线程上创建
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbarIcon = new TaskbarIcon();
                            _taskbarIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
                            _taskbarIcon.ToolTipText = "媒体播放器";
                            _initialized = true;
                            
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // 避免递归调用
                SystemNotificationHelper.ShowError($"初始化系统通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示信息类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public static void ShowInfo(string message)
        {
            ShowNotification("信息", message, BalloonIcon.Info);
        }

        /// <summary>
        /// 显示成功类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public static void ShowSuccess(string message)
        {
            ShowNotification("成功", message, BalloonIcon.Info);
        }

        /// <summary>
        /// 显示警告类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public static void ShowWarning(string message)
        {
            ShowNotification("警告", message, BalloonIcon.Warning);
        }

        /// <summary>
        /// 显示错误类型通知
        /// </summary>
        /// <param name="message">通知消息内容</param>
        public static void ShowError(string message)
        {
            ShowNotification("错误", message, BalloonIcon.Error);
        }

        /// <summary>
        /// 显示自定义标题和内容的通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知消息内容</param>
        /// <param name="icon">通知图标类型</param>
        private static void ShowNotification(string title, string message, BalloonIcon icon)
        {
            try
            {
                // 确保通知系统已初始化
                if (!_initialized)
                {
                    Initialize();
                }

                // 确保在UI线程上显示通知
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_taskbarIcon != null)
                    {
                        _taskbarIcon.ShowBalloonTip(title, message, icon);
                    }
                    else
                    {
                        
                        FallbackToCustomToast(title, message);
                    }
                });
            }
            catch (Exception ex)
            {
               
                FallbackToCustomToast(title, message);
            }
        }

        /// <summary>
        /// 当原生通知失败时的回退方法
        /// </summary>
        private static void FallbackToCustomToast(string title, string message)
        {
            try
            {
                // 简单的消息框提示作为最后的回退选项
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"回退通知方法失败: {ex.Message}");
            }
        }
    }
}