using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CTS.Charon.Devices;
using System.Configuration;

namespace CharonUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       // private static readonly NetDuinoPlus _netDuino;

        public MainWindow()
        {
            InitializeComponent();

          //  tbTime.IsReadOnly = true;
          //  tbTime.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

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

            // talk to this device and get the current status of the relays:
          //  _netDuino = NetDuinoPlus.Instance(deviceIP);

           // InitButtonStates();
        }

       

        private void OnTurnOn(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void OnTurnOff(object sender, RoutedEventArgs e)
        {

        }


        //private void InitButtonStates()
        //{
        //   if(NetDuinoPlus.IsRelay1Energized)
        //   {
        //        // means the DC Bus is  off. Slide the button to off position:
        //        BtnDCBus.IsChecked = false;
        //   }
        //   else
        //   {
        //        BtnDCBus.IsChecked = true;
        //   }

        //}
    }
}
