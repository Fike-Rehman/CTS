using System;
using System.Configuration;
using System.Threading.Tasks;
using CTS.Charon.Devices;
using CTS.Common.Utilities;
using System.Timers;


namespace CTS.Charon.CharonApplication
{
    internal class CharonApplication
    {
        private static bool consoleMode;

        private static readonly log4net.ILog logger =
                 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Timer pingtimer;
        private static Timer changeStateR1Timer;
        private static Timer changeStateR2Timer;

        // TODO: inject this as dependency
        private readonly NetDuinoPlus netDuino;

       
        private static int dcBusOnTimeOffset;
        private static DateTime dcBusOffTime;
        private static int acBusOnTimeOffset;
        private static DateTime acBusOffTime;


        public CharonApplication(bool consoleMode)
        {
            CharonApplication.consoleMode = consoleMode;

            
            if (CharonApplication.consoleMode)
            {
                Console.WriteLine($"Started Charon Service in console mode {DateTime.Now}");
                Console.WriteLine("Press any key to exit...");
                Console.WriteLine();
            }   
            else
                logger.Info($"Started Charon Service in console mode {DateTime.Now}");


            // Read in the configuration:
            var deviceIP = string.Empty;

            try
            {
               deviceIP = ConfigurationManager.AppSettings["deviceIPAddress"];

               dcBusOnTimeOffset = Convert.ToInt16(ConfigurationManager.AppSettings["DCRelayOnTimeOffest"]);
               dcBusOffTime = Convert.ToDateTime(ConfigurationManager.AppSettings["DCRelayOffTime"]);

               acBusOnTimeOffset = Convert.ToInt16(ConfigurationManager.AppSettings["DCRelayOnTimeOffest"]);
               acBusOffTime = Convert.ToDateTime(ConfigurationManager.AppSettings["ACRelayOffTime"]);
            }
            catch (ConfigurationErrorsException)
            {
                LogMessage("Error Reading Configuration File...");
            }

            // Instantiate the device:
            netDuino = NetDuinoPlus.Instance(deviceIP);

            // and Run with it:
            Run();
            
            if (!CharonApplication.consoleMode)
            {
                Stop();
                return;
            }
            
            Console.ReadKey();
            Stop();
        }


        /// <summary>
        /// Executes a Device Ping. If the ping is successful, it starts a ping timer and 
        /// then sets up the netduino relays to the correct state based on the time of the day.
        /// Also sets up the timers for subsequent relay state changes.
        /// </summary>
        private void Run()
        {
            if (netDuino.ExecutePing(LogMessage))
            {
                LogMessage("Device Initialization Success...");

                // Device initialization succeeded. We can continue with more operations:
                // set up a timer that sends a ping asynchronously every minute:
                var pingInterval = new TimeSpan(0, 0, 1, 0); // 1 minute  

                pingtimer = new Timer(pingInterval.TotalMilliseconds);
                pingtimer.Elapsed += OnPingTimer;
                pingtimer.AutoReset = true;
                pingtimer.Enabled = true;

                // Start with setting up the netDuino relays:
                SetNetDuinoRelaysAsync();
            }
            else
            {
                // introduce a delay to give it a chance to report the progress:
                Task.Delay(1000);
                

                LogMessage($"Device Ping Failed after {this.netDuino.NumTries} attempts");

                // There is not much point in continuing on at this point. Just send
                // out an Alert and stop the app:
                LogMessage("Device is either not online or has mal-functioned.");
                LogMessage("Sending Alert...");

                var alert = new AlertSender();

                var @address = NetDuinoPlus.DeviceIPAddress.Substring(7, 13);

                var msg =
                    "Your device has failed to respond to Ping request(s) dispatched to address: " + @address + " after repeated attempts.\r\n" +
                    $"{Environment.NewLine}Event Date & Time: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {Environment.NewLine}" +
                    $"{Environment.NewLine}Please check device and make sure that it is still online!";

                LogMessage(alert.SendEmailAlert("Atert: Device Ping Failed", bodyText: msg)
                    ? "Alert dispatch via Email completed successfully"
                    : "Attempt to send an email alert failed!");

                LogMessage(alert.SendSMSAlert("Atert: Device Ping Failed", msg)
                    ? "Alert dispatch via SMS completed successfully"
                    : "Attempt to send an SMS alert failed!");
            }

        }

        private async void SetNetDuinoRelaysAsync()
        {
            await SetNetDuinoDCRelay();
            await SetNetDuinoACRelay();
        }

        /// <summary>
        /// Sends commands to set the current state of the NetDuino Relay R1 (DC Relay) based on the 
        /// configured OnTime and OffTime values
        /// </summary>
        /// <returns> returns a TimeSpan that tells us when we need to call this method again</returns>
        private async Task<TimeSpan> SetNetDuinoDCRelay()
        {
            string result;
            var alert = new AlertSender();

            // First calculate the DC Bus On Time value using Today's Sunset time &
            // given on Time offset value:
            DateTime sunriseToday, sunsetToday;
            SunTimes.GetSunTimes(out sunriseToday, out sunsetToday);

            var onTime = sunsetToday - new TimeSpan(0, 0, dcBusOnTimeOffset, 0);
            var offTime = dcBusOffTime;

            if (onTime > offTime)
            {
                LogMessage("Invalid Configuration!. Please check the On/Off Time values");
                return TimeSpan.MinValue;
            }

            // time to wait for next state change trigger
            TimeSpan stateChangeInterval;
            
            if(DateTime.Now.TimeOfDay < onTime.TimeOfDay)
            {
                // we are in daytime
                // energize the relay 1 to turn lights off and set the interval for next state change
                result = await netDuino.EnergizeRelay1();
                stateChangeInterval = onTime.TimeOfDay - DateTime.Now.TimeOfDay;
            }
            else if (DateTime.Now.TimeOfDay >= onTime.TimeOfDay && DateTime.Now.TimeOfDay <= offTime.TimeOfDay)
            {
                // we are in the onTime..
                // de-energize relay1 to turn the lights on and set them to turn off at offTime
                result = await netDuino.DenergizeRelay1();
                stateChangeInterval = offTime.TimeOfDay - DateTime.Now.TimeOfDay;
                
                alert.SendSMSAlert("Alert",
                    $"DC Bus powered on at {DateTime.Now.ToLongTimeString()}. Today's Sunset Time: {sunsetToday.ToLongTimeString()}");
            }
            else
            {
                // Current time is between OffTime and midnight
                // energize the relays to turn the light off and set the interval to onTime + 1 Day
                result = await netDuino.EnergizeRelay1();
                stateChangeInterval = (new TimeSpan(1,0,0,0) + onTime.TimeOfDay) - DateTime.Now.TimeOfDay;
            }

            if (result == "Success")
            {
                if (changeStateR1Timer == null)
                {
                    //This is the first time this method is executed
                    // set up the timer to trigger next time the Relay state change is needed: 
                    changeStateR1Timer = new Timer(stateChangeInterval.TotalMilliseconds);
                    changeStateR1Timer.Elapsed += OnChangeStateR1Timer;
                    changeStateR1Timer.AutoReset = false;
                    changeStateR1Timer.Enabled = true;
                }
            }
            else
            {
                // here we deal with the failure...
                // set the TimeSpan to min value to return an indication of failure and log appropriate messages and alerts
                stateChangeInterval = TimeSpan.MinValue;
                
                var @address = NetDuinoPlus.DeviceIPAddress.Substring(7, 13);

                var msg =
                    "Netduino has failed to respond to Energize/Denergize DC relay request(s) dispatched to address: " + @address + "in a timely fashion" +
                    $"{Environment.NewLine}Event Date & Time: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {Environment.NewLine}" +
                    $"{Environment.NewLine}Please check Netduino and make sure that it is still online!";

                LogMessage(alert.SendEmailAlert("Atert: Energize/Denergize Relay request to NetDuino Failed", bodyText: msg)
                    ? "Alert dispatch via Email completed successfully"
                    : "Attempt to send an email alert failed!");

                LogMessage(alert.SendSMSAlert("Atert: Energize/Denergize Relay request to NetDuino Failed", msg)
                    ? "Alert dispatch via SMS completed successfully"
                    : "Attempt to send an SMS alert failed!");
            }
            
            return stateChangeInterval;
        }

        

        /// <summary>
        /// Sends commands to set the current state of the NetDuino Relay R2 (AC Relay) based on the 
        /// configured OnTime and OffTime values
        /// </summary>
        /// <returns> returns a TimeSpan that tells us when we need to call this method again</returns>
        private async Task<TimeSpan> SetNetDuinoACRelay()
        {
            string result;
            var alert = new AlertSender();

            // First calculate the DC Bus On Time value using Today's Sunset time &
            // given on Time offset value:
            DateTime sunriseToday, sunsetToday;
            SunTimes.GetSunTimes(out sunriseToday, out sunsetToday);

            var onTime = sunsetToday - new TimeSpan(0, 0, acBusOnTimeOffset, 0);
            var offTime = acBusOffTime;

            if (onTime > offTime)
            {
                LogMessage("Invalid Configuration!. Please check the On/Off Time values");
                return TimeSpan.MinValue;
            }

            // time to wait for next state change trigger
            TimeSpan stateChangeInterval;

            if (DateTime.Now.TimeOfDay < onTime.TimeOfDay)
            {
                // we are in daytime
                // energize the relay 1 to turn lights off and set the interval for next state change
                result = await netDuino.EnergizeRelay2();
                stateChangeInterval = onTime.TimeOfDay - DateTime.Now.TimeOfDay;
            }
            else if (DateTime.Now.TimeOfDay >= onTime.TimeOfDay && DateTime.Now.TimeOfDay <= offTime.TimeOfDay)
            {
                // we are in the onTime..
                // de-energize relay1 to turn the lights on and set then to turn off at offTime
                result = await netDuino.DenergizeRelay2();
                stateChangeInterval = offTime.TimeOfDay - DateTime.Now.TimeOfDay;
            }
            else
            {
                // Current time is between OffTime and midnight
                // energize the relays to turn the light off and set the interval to onTime + 1 Day
                result = await netDuino.EnergizeRelay2();
                stateChangeInterval = (new TimeSpan(1, 0, 0, 0) + onTime.TimeOfDay) - DateTime.Now.TimeOfDay;
            }

            if (result == "Success")
            {
                if (changeStateR2Timer == null)
                {
                    //This is the first time this method is executed
                    // set up the timer to trigger next time the Relay state change is needed: 
                    changeStateR2Timer = new Timer(stateChangeInterval.TotalMilliseconds);
                    changeStateR2Timer.Elapsed += OnChangeStateR2Timer;
                    changeStateR2Timer.AutoReset = false;
                    changeStateR2Timer.Enabled = true;
                }
            }
            else
            {
                // here we deal with the failure...
                // set the TimeSpan to min value to return an indication of failure and log appropriate messages and alerts
                stateChangeInterval = TimeSpan.MinValue;

                var @address = NetDuinoPlus.DeviceIPAddress.Substring(7, 13);

                var msg =
                    "Netduino has failed to respond to Energize/Denergize AC relay request(s) dispatched to address: " + @address + "in a timely fashion" +
                    $"{Environment.NewLine}Event Date & Time: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {Environment.NewLine}" +
                    $"{Environment.NewLine}Please check Netduino and make sure that it is still online!";

                LogMessage(alert.SendEmailAlert("Atert: Energize/Denergize AC Relay request to NetDuino Failed", bodyText: msg)
                    ? "Alert dispatch via Email completed successfully"
                    : "Attempt to send an email alert failed!");

                LogMessage(alert.SendSMSAlert("Atert: Energize/Denergize Relay request to NetDuino Failed", msg)
                    ? "Alert dispatch via SMS completed successfully"
                    : "Attempt to send an SMS alert failed!");
            }

            return stateChangeInterval;
        }

        #region Timer event Handler methods

        private async void OnPingTimer(object sender, ElapsedEventArgs e)
        {
            // send a ping asynchronously
            
            await netDuino.ExecutePingAsync(LogMessage);     
        }

        private async void OnChangeStateR1Timer(object sender, ElapsedEventArgs e)
        {
            changeStateR1Timer.Stop();

            var stateChangeInterval =  await SetNetDuinoDCRelay();

            LogMessage(
                $"-- Setting R1 timer to go off in {stateChangeInterval}. Current Time: {DateTime.Now.ToLongTimeString()}");

            if (stateChangeInterval > TimeSpan.MinValue)
            {
                changeStateR1Timer.Interval = stateChangeInterval.TotalMilliseconds;
                changeStateR1Timer.Start();
            }

            
        }

        private async void OnChangeStateR2Timer(object sender, ElapsedEventArgs e)
        {
            changeStateR2Timer.Stop();

            var stateChangeInterval = await SetNetDuinoACRelay();

            if (stateChangeInterval > TimeSpan.MinValue)
            {
                changeStateR2Timer.Interval = stateChangeInterval.TotalMilliseconds;
                changeStateR2Timer.Start();
            }
        }

        #endregion


        private static void LogMessage(string msg )
        {
            if (consoleMode)
            {
                Console.WriteLine(msg);
            }
            else
            {
                logger.Info(msg);      
            }
        }


        public void Stop()
        {
            if(consoleMode)
            {
                Console.WriteLine($"Charon Service Stop requested at {DateTime.Now}");
                logger.Info("Exiting Charon Service Application...");

                var n = 3;
                while (n > 0)
                {
                    Console.Write($"\rStopping application in {n} seconds");
                    Task.Delay(1000);
                    n--;
                }
            }
            else
            {
                logger.Info("Stopping Charon Service Application");
            } 

            // Dispose the Timers:
            pingtimer.Dispose();
            changeStateR1Timer.Dispose();
            changeStateR2Timer.Dispose();
        }
    }
}
