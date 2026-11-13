using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace UNIDEN_UNIACE_100
{
    /// <summary>
    /// Interaction logic for ChannelDisplay.xaml
    /// </summary>
    public partial class ChannelDisplay : UserControl
    {
        // 1. Register the Dependency Property as 'long' to match calculatedFrequency
        public static readonly DependencyProperty ChannelProperty =
            DependencyProperty.Register(
                nameof(Channel),
                typeof(long),
                typeof(ChannelDisplay),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChannelChanged));

        // 1b. Define the Dependency Property for the editing state
        public static readonly DependencyProperty IsDigitEditingProperty =
            DependencyProperty.Register(
                nameof(IsDigitEditing),
                typeof(bool),
                typeof(ChannelDisplay),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsDigitEditing
        {
            get => (bool)GetValue(IsDigitEditingProperty);
            set => SetValue(IsDigitEditingProperty, value);
        }

        public long Channel
        {
            get => (long)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        // ... (keep the rest of your methods below unchanged)

        private bool _isUpdating = false;

        public ChannelDisplay()
        {
            InitializeComponent();

            // Wait until components are initialized to wire up the events
            this.Loaded += ChannelDisplay_Loaded;
        }

        private void ChannelDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            // 2. Hook into property change notifications for each individual digit
            var dpd = DependencyPropertyDescriptor.FromProperty(SevenSegmentDisplay.HexDigitProperty, typeof(SevenSegmentDisplay));

            // Also grab a descriptor for the new IsEditing property
            var editDpd = DependencyPropertyDescriptor.FromProperty(SevenSegmentDisplay.IsEditingProperty, typeof(SevenSegmentDisplay));

            SevenSegmentDisplay[] digits = { Digit_1, Digit_2 };

            foreach (var digit in digits)
            {
                if (digit != null)
                {
                    dpd.AddValueChanged(digit, OnChildDigitChanged);

                    // Added: Track when a digit enters or leaves its scroll-wheel edit state
                    editDpd.AddValueChanged(digit, OnChildEditStateChanged);
                }
            }

            // Push the initial channel value down to the display
            UpdateDisplayFromChannel();
        }

        // Automatically runs whenever any child digit turns its scroll wheel on or off
        private void OnChildEditStateChanged(object sender, EventArgs e)
        {
            IsDigitEditing = (Digit_1 != null && Digit_1.IsEditing) ||
                             (Digit_2 != null && Digit_2.IsEditing);
        }

        // Triggered when someone changes the overarching "Channel" property externally
        private static void OnChannelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ChannelDisplay)d;
            control.UpdateDisplayFromChannel();
        }
        // Pushes the 'Channel' long value out into the individual UI segments
        private void UpdateDisplayFromChannel()
        {
            // Changed: If _isUpdating OR a digit is being actively edited, return immediately!
            if (_isUpdating || IsDigitEditing) return;
            _isUpdating = true;

            try
            {
                // Pad left to 2 digits (e.g., 01 -> "40")
                string freqStr = Channel.ToString().PadLeft(2, '0');

                if (freqStr.Length > 2)
                    freqStr = freqStr.Substring(freqStr.Length - 2); // clamp to max 2 digits

                Digit_1.HexDigit = (SevenSegmentDisplay.HexDigits)char.GetNumericValue(freqStr[0]);
                Digit_2.HexDigit = (SevenSegmentDisplay.HexDigits)char.GetNumericValue(freqStr[1]);
            }
            catch
            {
                // Fallback catch if parsing fails
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // 3. Triggered when the user clicks/scrolls a child digit UI component manually
        private void OnChildDigitChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                // Reconstruct the channel string based on the current states of the UI elements
                string freqStr =
                    ((int)Digit_1.HexDigit).ToString() +
                    ((int)Digit_2.HexDigit).ToString();

                if (long.TryParse(freqStr, out long calculatedFrequency))
                {
                    // Update the master dependency property, which bubbles out to your ViewModel/Main Application
                    Channel = calculatedFrequency;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}