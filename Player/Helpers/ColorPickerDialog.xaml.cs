using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Player.Helpers
{
    public partial class ColorPickerDialog : Window
    {
        private bool _isUpdatingFromSliders = false;
        private bool _isUpdatingFromTextBox = false;

        public string SelectedColor { get; private set; }

        public ColorPickerDialog()
        {
            InitializeComponent();
            SelectedColor = "#7B68EE"; // 默认紫罗兰色
        }

        /// <summary>
        /// 预设颜色按钮点击
        /// </summary>
        private void PresetColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string colorHex)
            {
                SetColor(colorHex);
            }
        }

        /// <summary>
        /// RGB 滑块值改变
        /// </summary>
        private void RgbSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdatingFromTextBox) return;
            
            // 检查控件是否已初始化
            if (RedValue == null || GreenValue == null || BlueValue == null || 
                ColorTextBox == null || ColorPreview == null)
                return;

            _isUpdatingFromSliders = true;

            byte r = (byte)RedSlider.Value;
            byte g = (byte)GreenSlider.Value;
            byte b = (byte)BlueSlider.Value;

            RedValue.Text = r.ToString();
            GreenValue.Text = g.ToString();
            BlueValue.Text = b.ToString();

            string colorHex = $"#{r:X2}{g:X2}{b:X2}";
            ColorTextBox.Text = colorHex;
            ColorPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));

            _isUpdatingFromSliders = false;
        }

        /// <summary>
        /// 颜色文本框内容改变
        /// </summary>
        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromSliders) return;
            
            // 检查控件是否已初始化
            if (ColorPreview == null || RedSlider == null || GreenSlider == null || BlueSlider == null)
                return;

            string colorText = ColorTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(colorText) || !colorText.StartsWith("#")) return;

            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorText);
                ColorPreview.Fill = new SolidColorBrush(color);

                _isUpdatingFromTextBox = true;
                RedSlider.Value = color.R;
                GreenSlider.Value = color.G;
                BlueSlider.Value = color.B;
                _isUpdatingFromTextBox = false;
            }
            catch
            {
                // 无效颜色值，忽略
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        private void SetColor(string colorHex)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorHex);
                
                _isUpdatingFromTextBox = true;
                ColorTextBox.Text = colorHex;
                ColorPreview.Fill = new SolidColorBrush(color);
                RedSlider.Value = color.R;
                GreenSlider.Value = color.G;
                BlueSlider.Value = color.B;
                _isUpdatingFromTextBox = false;
            }
            catch (Exception ex)
            {
                SystemNotificationHelper.ShowError($"无效的颜色值: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用按钮点击
        /// </summary>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string colorText = ColorTextBox.Text.Trim();
            if (!colorText.StartsWith("#") || colorText.Length != 7)
            {
                SystemNotificationHelper.ShowWarning("请输入有效的颜色值（格式：#RRGGBB）");
                return;
            }

            try
            {
                // 验证颜色值
                Color color = (Color)ColorConverter.ConvertFromString(colorText);
                SelectedColor = colorText.ToUpper();
                DialogResult = true;
                Close();
            }
            catch
            {
                SystemNotificationHelper.ShowError("无效的颜色值");
            }
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        //private void CancelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //    Close();
        //}
    }
}
