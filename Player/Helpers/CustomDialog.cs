using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Player.Helpers
{
    /// <summary>
    /// 自定义对话框 - 替换MaterialDesign的DialogHost
    /// </summary>
    public class CustomDialog : ContentControl
    {
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(CustomDialog),
                new PropertyMetadata(false, OnIsOpenChanged));

        public static readonly DependencyProperty CloseOnClickAwayProperty =
            DependencyProperty.Register(nameof(CloseOnClickAway), typeof(bool), typeof(CustomDialog),
                new PropertyMetadata(true));

        public static readonly DependencyProperty DialogContentProperty =
            DependencyProperty.Register(nameof(DialogContent), typeof(object), typeof(CustomDialog));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public bool CloseOnClickAway
        {
            get => (bool)GetValue(CloseOnClickAwayProperty);
            set => SetValue(CloseOnClickAwayProperty, value);
        }

        public object DialogContent
        {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dialog = (CustomDialog)d;
            dialog.UpdateVisualState();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (CloseOnClickAway && IsOpen)
            {
                IsOpen = false;
                e.Handled = true;
            }
        }

        private void UpdateVisualState()
        {
            if (IsOpen)
            {
                Visibility = Visibility.Visible;
                Focus();
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// 关闭对话框命令
    /// </summary>
    public static class CustomDialogCommands
    {
        public static readonly RoutedUICommand CloseDialogCommand = 
            new RoutedUICommand("Close Dialog", "CloseDialog", typeof(CustomDialogCommands));

        static CustomDialogCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(Button), 
                new CommandBinding(CloseDialogCommand, OnCloseDialog));
        }

        private static void OnCloseDialog(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var dialog = FindParent<CustomDialog>(button);
                if (dialog != null)
                {
                    dialog.IsOpen = false;
                }
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T found)
                    return found;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}