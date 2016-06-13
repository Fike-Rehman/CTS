﻿using System;
using System.Threading;
using CharonServiceApplication;


namespace CTS.Charon.CharonApplication
{
    internal class CharonApplication
    {
        private readonly bool _consoleMode;

        private static readonly log4net.ILog _logger =
                 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Timer _pingtimer;
        private readonly DevicePingTask _pingTask;


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
            _pingTask = DevicePingTask.Instance();

            if (_pingTask.Execute(LogMessage))
            {
                // Device initialization succeeded. We can continue with more operations:
                // set up a timer that sends a ping asynchronously every minute:

                var pingInterval = new TimeSpan(0, 0, 1, 0); // 1 minute  
                _pingtimer = new Timer(OnPingTimer, null, pingInterval, Timeout.InfiniteTimeSpan);
            }
            else
            {
                // introduce a delay to give it a chance to report the progress:
                Thread.Sleep(1000);

                LogMessage($"Device Ping Failed after {_pingTask.NumTries} attempts");

                // There is not much point in continuing on at this point. Just send
                // out an Alert and stop the app:
                LogMessage("Device is either not online or has mal-functioned.");
                LogMessage("Sending Alert...");
            }


            if (!_consoleMode)
            {
                Stop();
                return;
            }
            
            Console.ReadKey();
            Stop();
        }

        private async void OnPingTimer(object state)
        {
            // send a ping asynchronously and reset the timer

            await _pingTask.ExecuteAsync(LogMessage);

            var pingInterval = new TimeSpan(0, 0, 1, 0); // 1 minute
            _pingtimer.Change(pingInterval, Timeout.InfiniteTimeSpan);
        }

        
        private void LogMessage(string msg )
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
