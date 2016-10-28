using System;
using System.Windows;
using System.Windows.Input;
using CTS.Charon.Devices;
using System.Configuration;
using System.Windows.Threading;
using CTS.Common.Utilities;
using System.Speech.Synthesis;


namespace CharonUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NetDuinoPlus netDuino;

        public MainWindow()
        {
            InitializeComponent();


            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();


            // Display today's sunset/sunrise times:

            DateTime sunriseToday, sunsetToday;
            SunTimes.GetSunTimes(out sunriseToday, out sunsetToday);

            lblSunriseVal.Content = sunriseToday.ToShortTimeString();
            lblSunsetVal.Content = sunsetToday.ToShortTimeString();


            // We dont really have a way to get the current state of the AC & DC bus
            // So we just initialize the switches to 'off' state which may not be the true 
            // state of the Netduino.

            BtnDCBus.IsChecked = false;
            BtnACBus.IsChecked = false;

            BtnDCBus.Checked += BtnDCBus_Checked;
            BtnDCBus.Unchecked += BtnDCBus_Unchecked;

            BtnACBus.Checked += BtnACBus_Checked;
            BtnACBus.Unchecked += BtnACBus_Unchecked;


            // Read in the configurtion:
            var deviceIP = string.Empty;

            try
            {
                deviceIP = ConfigurationManager.AppSettings["deviceIPAddress"];

            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error Reading Configuration File");
            }

            
            netDuino = NetDuinoPlus.Instance(deviceIP);        
        }

        private void BtnACBus_Unchecked(object sender, RoutedEventArgs e)
        {
            // turn the lights off

            var synth = new SpeechSynthesizer();

            synth.SetOutputToDefaultAudioDevice();
            
            synth.SpeakAsync("Turned the AC Bus Off at" + DateTime.Now.ToShortTimeString());
                  
        }

        private void BtnACBus_Checked(object sender, RoutedEventArgs e)
        {
            // turn the lights On
            MessageBox.Show("turned the AC Bus oN");

            // SetACRelayAsync(true);
        }

        private void BtnDCBus_Unchecked(object sender, RoutedEventArgs e)
        {
            // turn the lights off
            MessageBox.Show("turned the DC Bus off");

           // SetDCRelayAsync(false);

        }

        private void BtnDCBus_Checked(object sender, RoutedEventArgs e)
        {
            // trun the lights oN
            MessageBox.Show("turned the DC Bus oN");

            // SetDCRelayAsync(true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lblDate.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void SetDCRelayAsync(bool on)
        {
            string result;

            // Denergize the relay to turn the light on, otherwise enrgize them

            result = @on ? await netDuino.DenergizeRelay1() : await netDuino.EnergizeRelay1();

            if (result == "Success")
            {
                // announce success
            }
        }

        private async void SetACRelayAsync(bool on)
        {
            string result;

            // Denergize the relay to turn the light on, otherwise enrgize them

            result = @on ? await netDuino.DenergizeRelay2() : await netDuino.EnergizeRelay2();

            if (result == "Success")
            {
                // announce success
            }
        }
    }
}
