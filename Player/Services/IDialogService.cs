using System;
using System.Threading.Tasks;

namespace Player.Services
{
    /// <summary>
    /// 对话框服务接口
    /// 抽象对话框显示逻辑，符合MVVM原则
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 显示设置对话框
        /// </summary>
        /// <returns>任务</returns>
        Task ShowSettingsDialogAsync();
        
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="confirmText">确认按钮文本</param>
        /// <param name="cancelText">取消按钮文本</param>
        /// <returns>是否确认</returns>
        Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "确定", string cancelText = "取消");
        
        /// <summary>
        /// 显示信息对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>任务</returns>
        Task ShowInformationDialogAsync(string title, string message, string buttonText = "确定");
        
        /// <summary>
        /// 显示错误对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="buttonText">按钮文本</param>
        /// <returns>任务</returns>
        Task ShowErrorDialogAsync(string title, string message, string buttonText = "确定");
    }
}