using System;
using System.Threading.Tasks;
using System.Windows;

namespace Player.Services
{
    /// <summary>
    /// 对话框服务实现
    /// 封装对话框显示逻辑，符合MVVM原则
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// 显示设置窗口（具有对话框风格的Window）
        /// </summary>
        /// <returns>任务</returns>
        public async Task ShowSettingsDialogAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var settingsWindow = new Player.Left.SettingsDialog();
                    
                    // 设置窗口的所有者为当前窗口
                    if (Application.Current.MainWindow != null)
                    {
                        settingsWindow.Owner = Application.Current.MainWindow;
                        settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }
                    
                    // 使用 ShowDialog() 保持模态窗口行为
                    settingsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    // 使用对话框服务自身显示错误
                    ShowErrorDialogAsync("错误", $"打开设置失败: {ex.Message}").Wait();
                }
            });
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="confirmText">确认按钮文本</param>
        /// <param name="cancelText">取消按钮文本</param>
        /// <returns>是否确认</returns>
        public async Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "确定", string cancelText = "取消")
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.Cancel);
                
                return result == MessageBoxResult.OK;
            });
        }

        /// <summary>
        /// 显示信息对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>任务</returns>
        public async Task ShowInformationDialogAsync(string title, string message, string buttonText = "确定")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>任务</returns>
        public async Task ShowErrorDialogAsync(string title, string message, string buttonText = "确定")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }
}