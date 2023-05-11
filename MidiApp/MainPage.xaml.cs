using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

using Spring.Net.Rtp;

using Spring.Net.Rtp.AppleMidi;
using Spring.WinRT.Utils;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MidiApp
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AppleMidiSessionListener listener_;
        private string address_;

        //private IRegisterService provider_;

        private List<RtpMidiSession> sessions_ = new List<RtpMidiSession>();
        private MidiSessionEventScheduler scheduler_ = new MidiSessionEventScheduler();

        private readonly SolidColorBrush brushWhitePressed_ = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xDE, 0xC6, 0xE2));
        private readonly SolidColorBrush brushWhiteDepressed_ = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF5));

        private readonly SolidColorBrush brushBlackPressed_ = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x1B, 0x2E, 0x49));
        private readonly SolidColorBrush brushBlackDepressed_ = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x07, 0x07, 0x15));

        public MainPage()
        {
            this.InitializeComponent();

            brushWhiteDepressed_ = rcKeyA4.Fill as SolidColorBrush;
            brushBlackDepressed_ = rcKeyASharp4.Fill as SolidColorBrush;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            address_ = IPAddressHelper.GetHostAddress();
            txtIPAddress.Text = address_;
            txtIPAddress.IsEnabled = false;
            btnStop.IsEnabled = false;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            int localPort = Int16.Parse(txtPortNumber.Text);

            try
            {
                listener_ = new AppleMidiSessionListener("Test", address_, localPort);
                listener_.OnSessionCreated += OnSessionCreated;


                listener_.OnSessionClosing += OnSessionClosing;

                await listener_.StartAsync();
            }
            catch (Exception /* ex */)
            {
                return;
            }

            txtPortNumber.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            listener_.Stop();
            listener_.Dispose();
            listener_ = null;

            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            txtPortNumber.IsEnabled = true;
        }

        private void txtPortNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            int port;
            if (Int32.TryParse(txtPortNumber.Text, out port))
            {
                btnStart.IsEnabled = true;
                txtPortNumber.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            }

            else
            {
                btnStart.IsEnabled = false;
                txtPortNumber.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        private void OnSessionCreated(object sender, RtpMidiSessionEventArgs e)
        {
            sessions_.Add(e.Session);
            scheduler_.Start(e.Session);
        }

        private void OnSessionClosing(object sender, RtpMidiSessionEventArgs e)
        {
            scheduler_.Stop();
            sessions_.Remove(e.Session);
        }

        private void rcKeyXX_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnPointerPressed(sender);
        }

        private void rcKeyXX_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            OnPointerReleased(sender);
        }

        private void rcKeyXX_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!e.Pointer.IsInContact) return;
            OnPointerPressed(sender);
        }

        private void rcKeyXX_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!e.Pointer.IsInContact) return;
            OnPointerReleased(sender);
        }

        private void OnPointerPressed(object sender)
        {
            var rectangle = (sender as Rectangle);
            PressKey(rectangle);
            OnKey(rectangle.Tag as String, true);
        }

        private void OnPointerReleased(object sender)
        {
            var rectangle = (sender as Rectangle);
            ReleaseKey(rectangle);
            OnKey(rectangle.Tag as String, false);
        }

        private void PressKey(Rectangle rectangle)
        {
            if (IsSharpKey(rectangle))
                rectangle.Fill = brushBlackPressed_;
            else
                rectangle.Fill = brushWhitePressed_;
        }
        private bool IsSharpKey(Rectangle rc)
        {
            return
                ReferenceEquals(rc, rcKeyCSharp3) ||
                ReferenceEquals(rc, rcKeyDSharp3) ||
                ReferenceEquals(rc, rcKeyFSharp3) ||
                ReferenceEquals(rc, rcKeyGSharp3) ||
                ReferenceEquals(rc, rcKeyASharp3) ||
                ReferenceEquals(rc, rcKeyCSharp4) ||
                ReferenceEquals(rc, rcKeyDSharp4) ||
                ReferenceEquals(rc, rcKeyFSharp4) ||
                ReferenceEquals(rc, rcKeyGSharp4) ||
                ReferenceEquals(rc, rcKeyASharp4) ||
                ReferenceEquals(rc, rcKeyCSharp5) ||
                ReferenceEquals(rc, rcKeyDSharp5) ||
                ReferenceEquals(rc, rcKeyFSharp5) ||
                ReferenceEquals(rc, rcKeyGSharp5)
                ;
        }

        private void ReleaseKey(Rectangle rectangle)
        {
            if (IsSharpKey(rectangle))
                rectangle.Fill = brushBlackDepressed_;
            else
                rectangle.Fill = brushWhiteDepressed_;
        }

        private void OnKey(string key, bool pressed)
        {
            if (sessions_.Count == 0)
                return;

            byte msg = 0x80;
            if (pressed) msg = 0x90;

            var note = byte.Parse(key);

            scheduler_.AddEvent(new byte[] { msg, note, 0x3F, });
        }
    }
}
