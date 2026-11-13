using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UNIDEN_UNIACE_100
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //InitializeAsync();
            SetNeedlePosition(-60);

            StartSlidingAnimation();
        }

        // Move the needle horizontally (negative values move left, positive move right)
        public void SetNeedlePosition(double xOffset)
        {
            NeedleTranslation.X = xOffset; // e.g., between -30 and 30
        }
        public void StartSlidingAnimation()
        {
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = -170.0,                         // Pixels to the left
                To = 54.0,                            // Pixels to the right
                Duration = TimeSpan.FromSeconds(1.0), // Time for one-way slide
                AutoReverse = true,                   // Automatically slide back
                RepeatBehavior = RepeatBehavior.Forever // Loop continuously
            };

            // Smooth acceleration/deceleration at the edges
            slideAnimation.EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut };

            NeedleTranslation.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
        }

        private bool _isDragging = false;
        private Point _lastMousePosition;
        private int _currentPosition = 1; // Tracks position from 1 to 40
        private double _currentAngle = 0;   // Tracks cumulative angle for continuous smooth spinning

        private void ChannelChangeKnob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(ChannelChangeKnob);
            Mouse.Capture(ChannelChangeKnob);
            e.Handled = true;
        }

        private void ChannelChangeKnob_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentMousePos = e.GetPosition(ChannelChangeKnob);
            double deltaY = _lastMousePosition.Y - currentMousePos.Y; // Dragging UP increases position

            // Adjust sensitivity: every 10 pixels moved changes position by 1 step
            if (Math.Abs(deltaY) >= 10)
            {
                int stepChange = (int)(deltaY / 10);
                RotateBySteps(stepChange);
                LargeChannelDisplay.Channel = _currentPosition;
                _lastMousePosition = currentMousePos; // Reset baseline for next threshold
            }
        }

        private void ChannelChangeKnob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        private void ChannelChangeKnob_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Scroll UP increments (+1), Scroll DOWN decrements (-1)
            int stepChange = e.Delta > 0 ? 1 : -1;
            RotateBySteps(stepChange);
            LargeChannelDisplay.Channel = _currentPosition;
            e.Handled = true;
        }

        private void RotateBySteps(int stepChange)
        {
            if (stepChange == 0) return;

            // 1. Calculate new position with infinite 1-40 wrap-around
            int newPosition = _currentPosition + stepChange;
            _currentPosition = ((newPosition - 1) % 40 + 40) % 40 + 1;

            // 2. Accumulate the angle continuously (9 degrees per step)
            double degreesPerStep = 360.0 / 40.0;
            _currentAngle += stepChange * degreesPerStep;

            // 3. Animate the Image's rotation transform
            DoubleAnimation animation = new DoubleAnimation
            {
                From = KnobRotateTransform.Angle,
                To = _currentAngle,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            KnobRotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
        //private async void InitializeAsync()
        //{
        //    // Wait for the Chromium engine to spin up inside the control
        //    await SdrWebView.EnsureCoreWebView2Async();

        //    // Point the control directly to QRZ
        //    SdrWebView.Source = new Uri("https://www.qrz.com/");
        //    //SdrWebView.Source = new Uri("http://websdr.ewi.utwente.nl:8901/m.html");

        //    // Once the page finishes loading, inject JavaScript to hide unwanted elements
        //    SdrWebView.CoreWebView2.NavigationCompleted += (sender, args) =>
        //    {
        //        if (args.IsSuccess)
        //        {
        //            InjectOpenWebRxCleanupScript();
        //        }
        //    };
        //}

        //private async void InjectOpenWebRxCleanupScript()
        //{
        //    string cleanOpenWebRxScript = @"
        //        (function() {
        //            // Hide OpenWebRX header banners or sidebar information blocks if desired
        //            var sidebar = document.querySelector('.sidebar, #sidebar, header');
        //            if (sidebar) {
        //                sidebar.style.display = 'none';
        //            }

        //            // Ensure the main spectrum/waterfall container expands to fill available space
        //            var panel = document.querySelector('#waterfall-container, .waterfall-panel');
        //            if (panel) {
        //                panel.style.width = '100%';
        //            }
        //        })();
        //    ";

        //    await SdrWebView.CoreWebView2.ExecuteScriptAsync(cleanOpenWebRxScript);
        //}

        private void RigCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}