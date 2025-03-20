using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MyQuanLyTrangSuc.View
{
    public partial class NotificationWindow : Window
    {
        private readonly Rect _screenArea = SystemParameters.WorkArea;

        public string Header { get; set; }
        public string Message { get; set; }
        public string ImagePath { get; set; }
        public LinearGradientBrush Gradient { get; set; }
        public SolidColorBrush RecFill { get; set; }
        public string Position { get; set; }

        public NotificationWindow()
        {
            InitializeComponent();
            DataContext = this;
            _Border.MouseEnter += OnBorderMouseEnter;
            _Border.MouseLeave += OnBorderMouseLeave;
        }

        public NotificationWindow(string header, string message, string imagePath, LinearGradientBrush gradient, SolidColorBrush recFill, string position)
            : this()
        {
            Header = header;
            Message = message;
            ImagePath = imagePath;
            Gradient = gradient;
            RecFill = recFill;
            Position = position;
        }

        private void OnBorderMouseLeave(object sender, MouseEventArgs e)
        {
            var fadeOut = (Storyboard)Resources["CloseButtonFadeOutAnimation"];
            fadeOut.Begin();
        }

        private void OnBorderMouseEnter(object sender, MouseEventArgs e)
        {
            var fadeIn = (Storyboard)Resources["CloseButtonFadeInAnimation"];
            fadeIn.Begin();
        }

        private void OnCloseMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowPosition();
            StartSlideInAnimation();
        }

        private void SetWindowPosition()
        {
            switch (Position)
            {
                case "Center":
                    Left = (_screenArea.Width - Width) / 2;
                    Top = (_screenArea.Height - Height) / 2;
                    break;
                case "BottomRight":
                    Left = _screenArea.Right - Width - 10;
                    Top = _screenArea.Bottom - Height - 10;
                    break;
            }
        }

        private void StartSlideInAnimation()
        {
            var slideIn = (Storyboard)Resources["WindowSlideInAnimation"];
            slideIn.Begin();
        }

        private void OnSlideInAnimationCompleted(object sender, EventArgs e)
        {
            StartRectangleWidthDecreaseAnimation();
        }

        private void StartRectangleWidthDecreaseAnimation()
        {
            var decreaseWidth = (Storyboard)Resources["RectangleWidthDecreaseAnimation"];
            decreaseWidth.Begin();
        }

        private void OnRectangleWidthDecreaseAnimationCompleted(object sender, EventArgs e)
        {
            StartSlideOutAnimation();
        }

        private void StartSlideOutAnimation()
        {
            var slideOut = (Storyboard)Resources["WindowSlideOutAnimation"];
            slideOut.Begin();
        }

        private void OnSlideOutAnimationCompleted(object sender, EventArgs e)
        {
            Close();
        }
    }
}

