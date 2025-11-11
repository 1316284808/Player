using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Player.Core.Enums;
using FontAwesome.Sharp;

namespace Player.Core.Helpers
{
    /// <summary>
    /// 自定义图标组件 - 使用FontAwesome图标替换几何路径
    /// </summary>
    public class CustomIcon : ContentControl
    {
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(IconKind), typeof(CustomIcon), 
                new PropertyMetadata(IconKind.None, OnKindChanged));

        public new static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(CustomIcon), 
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(CustomIcon), 
                new PropertyMetadata(16.0, OnIconSizeChanged));

        public IconKind Kind
        {
            get => (IconKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public new Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public CustomIcon()
        {
           
            this.Content = new IconBlock();
            // 设置默认图标大小
            IconSize = 16.0;
            // 临时修改前景色为红色，以便验证图标是否使用 
            UpdateIcon();
        }

        private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
            var icon = (CustomIcon)d;
            icon.UpdateIcon();
        }

        private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = (CustomIcon)d;
            icon.UpdateIcon();
        }

        private void UpdateIcon()
        {
           
            if (this.Content is IconBlock iconBlock)
            {
                var newIcon = GetFontAwesomeIcon(Kind);
                iconBlock.Icon = newIcon;
                iconBlock.FontSize = IconSize;
              
                this.InvalidateVisual();
                iconBlock.InvalidateVisual();
            }
        }
  
        private IconChar GetFontAwesomeIcon(IconKind kind)
        {
            return kind switch
            {
                IconKind.Settings => IconChar.GithubAlt,
                IconKind.Close => IconChar.Times,
                IconKind.FolderPlus => IconChar.FolderOpen,
                IconKind.Play => IconChar.Play,
                IconKind.Pause => IconChar.Pause,
                IconKind.MotionPlay => IconChar.Play,
                IconKind.MotionPause => IconChar.Pause,
                IconKind.VolumeHigh => IconChar.VolumeUp,
                IconKind.VolumeMedium => IconChar.VolumeDown,
                IconKind.VolumeLow => IconChar.VolumeOff,
                IconKind.VolumeMute => IconChar.VolumeMute,
                IconKind.Fullscreen => IconChar.Expand,
                IconKind.FullscreenExit => IconChar.Compress,
                IconKind.ChevronLeft => IconChar.List,
                IconKind.ChevronRight => IconChar.ListUl,
                IconKind.Delete => IconChar.DeleteLeft,
                _ => IconChar.None
            };
        }
    }
}