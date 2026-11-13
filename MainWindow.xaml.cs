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
