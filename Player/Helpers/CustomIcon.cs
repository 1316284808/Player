using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Player.Core.Enums;

namespace Player.Helpers
{
    /// <summary>
    /// 自定义图标组件 - 替换MaterialDesign的PackIcon
    /// </summary>
    public class CustomIcon : Control
    {
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(IconKind), typeof(CustomIcon), 
                new PropertyMetadata(IconKind.None, OnKindChanged));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(CustomIcon), 
                new PropertyMetadata(Brushes.Black));

        public IconKind Kind
        {
            get => (IconKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = (CustomIcon)d;
            icon.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            var geometry = GetIconGeometry(Kind);
            if (geometry != null)
            {
                drawingContext.DrawGeometry(Foreground, null, geometry);
            }
        }

        private Geometry GetIconGeometry(IconKind kind)
        {
            switch (kind)
            {
                case IconKind.Settings:
                    return Geometry.Parse("M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z");
                
                case IconKind.Close:
                    return Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z");
                
                case IconKind.FolderPlus:
                    return Geometry.Parse("M10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6H12L10,4M15,9V12H12V14H15V17H17V14H20V12H17V9H15Z");
                
                case IconKind.Play:
                    return Geometry.Parse("M14,19H18V5H14M6,19H10V5H6V19Z");

                case IconKind.Pause:
                    return Geometry.Parse("M9,6V18L17,12L9,6Z");
                 

                case IconKind.VolumeHigh:
                    return Geometry.Parse("M14,3.23V5.29C16.89,6.15 19,8.83 19,12C19,15.17 16.89,17.84 14,18.7V20.77C18,19.86 21,16.28 21,12C21,7.72 18,4.14 14,3.23M16.5,12C16.5,10.23 15.5,8.71 14,7.97V16C15.5,15.29 16.5,13.76 16.5,12M3,9V15H7L12,20V4L7,9H3Z");
                
                case IconKind.VolumeMedium:
                    return Geometry.Parse("M5,9V15H9L14,20V4L9,9H5M18.5,12C18.5,10.23 17.5,8.71 16,7.97V16C17.5,15.29 18.5,13.76 18.5,12Z");
                
                case IconKind.VolumeLow:
                    return Geometry.Parse("M7,9V15H11L16,20V4L11,9H7Z");
                
                case IconKind.VolumeMute:
                    return Geometry.Parse("M12,4L9.91,6.09L12,8.18M4.27,3L3,4.27L7.73,9H3V15H7L12,20V13.27L16.25,17.53C15.58,18.04 14.83,18.46 14,18.7V20.77C15.38,20.45 16.63,19.82 17.68,18.96L19.73,21L21,19.73L12,10.73M19,12C19,12.94 18.8,13.82 18.46,14.64L19.97,16.15C20.62,14.91 21,13.5 21,12C21,7.72 18,4.14 14,3.23V5.29C16.89,6.15 19,8.83 19,12M16.5,12C16.5,10.23 15.5,8.71 14,7.97V10.18L16.45,12.63C16.5,12.43 16.5,12.21 16.5,12Z");
                
                case IconKind.Fullscreen:
                    return Geometry.Parse("M5,5H10V7H7V10H5V5M14,5H19V10H17V7H14V5M17,14H19V19H14V17H17V14M10,17V19H5V14H7V17H10Z");
                
                case IconKind.ChevronLeft:
                    return Geometry.Parse("M15.41,16.58L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.58Z");
                
                case IconKind.ChevronRight:
                    return Geometry.Parse("M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z");
                
                case IconKind.MotionPlay:
                  
                    return Geometry.Parse("M14,19H18V5H14M6,19H10V5H6V19Z");
                case IconKind.MotionPause:
                    return Geometry.Parse("M8,5.14V19.14L19,12.14L8,5.14Z");

                default:
                    return null;
            }
        }
    }

}
