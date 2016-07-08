using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CharonServiceApplication;
using CTS.Charon.Devices;


namespace CTS.Charon.CharonApplication
{
    internal class CharonApplication
    {
        private static bool _consoleMode;

        private static readonly log4net.ILog _logger =
                 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Timer _pingtimer;
        private static Timer _changeStateR1Timer;

        // TODO: inject this as dependency
        private readonly NetDuinoPlus _netDuino;

        //TODO abstract these out to a class or interface
        private static DateTime _DCBusOnTime;
        private static DateTime _DCBusOffTime;


        // TODO: re-factor this to smaller method
        public CharonApplication(bool consoleMode)
        {
            _consoleMode = consoleMode;

            if (_consoleMode)
            {
                Console.WriteLine($"Started Charon Service in console mode {DateTime.Now}");
                Console.WriteLine("Press any key to exit...");
                Console.WriteLine();
            }   
            else
                _logger.Info($"Started Charon Service in console mode {DateTime.Now}");


            // Initialize and execute a device Ping to see if our board is online:
            var deviceIP = string.Empty;

            try
            {
               deviceIP = ConfigurationManager.AppSettings["deviceIPAddress"];

               _DCBusOnTime = Convert.ToDateTime(ConfigurationManager.AppSettings["12vRelayOnTime"]);
               _DCBusOffTime = Convert.ToDateTime(ConfigurationManager.AppSettings["12vRelayOffTime"]);             
            }
            catch (ConfigurationErrorsException)
            {
                LogMessage("Error Reading Configuration File...");
            }

            _netDuino = NetDuinoPlus.Instance(deviceIP);


            if (_netDuino.ExecutePing(LogMessage))
            {
                LogMessage("Device Initialization Success...");

                // Device initialization succeeded. We can continue with more operations:
                // set up a timer that sends a ping asynchronously every minute:
                var pingInterval = new TimeSpan(0, 0, 1, 0); // 1 minute  
                _pingtimer = new Timer(OnPingTimer, null, pingInterval, Timeout.InfiniteTimeSpan);

                // we set the R1 state synchronously at first
                SetNetDuinoRelay1();      
            }
            else
            {
                // introduce a delay to give it a chance to report the progress:
                Thread.Sleep(1000);

                LogMessage($"Device Ping Failed after {_netDuino.NumTries} attempts");

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


            if (!_consoleMode)
            {
                Stop();
                return;
            }
            
            Console.ReadKey();
            Stop();
        }

        /// <summary>
        /// Sends commands to set the current state of the NetDuino Relay R1 based on the given onTime and offTime
        /// values.
        /// </summary>
        /// <returns> returns a TimeSpan that tells us when we need to call this method again</returns>
        private static async Task<TimeSpan> SetNetDuinoRelay1()
        {
            string result;
            var onTime = _DCBusOnTime;
            var offTime = _DCBusOffTime;

            const bool bTesting = true;

            var alert = new AlertSender();

            Debug.Assert(onTime < offTime);

            // time to wait for next state change trigger
            TimeSpan stateChangeInterval;
            
            if(DateTime.Now.TimeOfDay < onTime.TimeOfDay)
            {
                // we are in daytime
                // energize the relay 1 to turn lights off and set the interval for next state change
                result = await NetDuinoPlus.EnergizeRelay1();
                stateChangeInterval = onTime.TimeOfDay - DateTime.Now.TimeOfDay;
                if (bTesting)
                {
                    alert.SendSMSAlert("Charon Alert", $"DC Bus was turned off at {DateTime.Now.ToLongTimeString()}");
                }
            }
            else if (DateTime.Now.TimeOfDay >= onTime.TimeOfDay && DateTime.Now.TimeOfDay <= offTime.TimeOfDay)
            {
                // we are in the onTime..
                // de-energize relay1 to turn the lights on and set then to turn off at offTime
                result = await NetDuinoPlus.DenergizeRelay1();
                stateChangeInterval = offTime.TimeOfDay - DateTime.Now.TimeOfDay;
                if (bTesting)
                {
                    alert.SendSMSAlert("Charon Alert", $"DC Bus was turned on at {DateTime.Now.ToLongTimeString()}");
                }
            }
            else
            {
                // Current time is between OffTime and midnight
                // energize the relays to turn the light off and set the interval to onTime + 1 Day
                result = await NetDuinoPlus.EnergizeRelay1();
                stateChangeInterval = (new TimeSpan(1,0,0,0) + onTime.TimeOfDay) - DateTime.Now.TimeOfDay;
                if (bTesting)
                {
                    alert.SendSMSAlert("Charon Alert", $"DC Bus was turned off at {DateTime.Now.ToLongTimeString()}");
                }
            }

            if (result == "Success")
            {
                if (_changeStateR1Timer == null)
                {
                    //This is the first time this method is executed
                    // set up the timer to trigger next time the Relay state change is needed: 
                    _changeStateR1Timer = new Timer(OnChangeStateR1Timer, null, stateChangeInterval, Timeout.InfiniteTimeSpan);
                }
            }
            else
            {
                // here we deal with the failure...
                // set the TimeSpan to min value to return an indication of failure and log appropriate messages and alerts
                stateChangeInterval = TimeSpan.MinValue;
                
                var @address = NetDuinoPlus.DeviceIPAddress.Substring(7, 13);

                var msg =
                    "Netduino has failed to respond to Energize/Denergize relay R1 request(s) dispatched to address: " + @address + "in a timely fashion" +
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

        private async void OnPingTimer(object state)
        {
            // send a ping asynchronously and reset the timer

            await _netDuino.ExecutePingAsync(LogMessage);

            var pingInterval = new TimeSpan(0, 0, 1, 0); // 1 minute
            _pingtimer.Change(pingInterval, Timeout.InfiniteTimeSpan);
        }

        private static async void OnChangeStateR1Timer(object state)
        {
           var stateChangeInterval =  await SetNetDuinoRelay1();

            if (stateChangeInterval > TimeSpan.MinValue)
            {
                _changeStateR1Timer.Change(stateChangeInterval, Timeout.InfiniteTimeSpan);
            }
        }

        
        private static void LogMessage(string msg )
        {
            if (_consoleMode)
            {
                Console.WriteLine(msg);
            }
            else
            {
                _logger.Info(msg);      
            }
        }


        public void Stop()
        {
            if(_consoleMode)
            {
                Console.WriteLine($"Charon Service Stop requested at {DateTime.Now}");
                _logger.Info("Exiting Charon Service Application...");

                var n = 3;
                while (n > 0)
                {
                    Console.Write($"\rStopping application in {n} seconds");
                    Thread.Sleep(1000);
                    n--;
                }
            }
            else
            {
                _logger.Info("Stopping Charon Service Application");
            } 
        }
    }
}
