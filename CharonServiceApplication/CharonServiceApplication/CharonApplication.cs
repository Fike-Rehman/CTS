using System;
using System.Threading;
using CharonServiceApplication;


namespace CTS.Charon.CharonApplication
{
    internal class CharonApplication
    {
        private readonly bool _consoleMode;

        private static readonly log4net.ILog _logger =
                 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

            ExecuteDevicePing();

            if (!_consoleMode)
            {
                Stop();
                return;
            }
            
            Console.ReadKey();
            Stop();
        }

        private void ExecuteDevicePing()
        {
            //var numRetries = 3;

            //TODO: elevate this to class variable and call execute repeatedly
            var pingTask = new DevicePingTask();

            //var n = 0;
             
            //while(n < numRetries)
            //{
            //    n++;

            //    // This will be blocking! It might cause problems if we running in a ASP.NET context
            //    https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
            //    if (pingTask.ExecuteAsync(new Progress<string>(LogMessage)).Result)
            //    {
            //        return;
            //    }
            //}

            pingTask.Execute(LogMessage);

            // introduce a delay to give it a chance to report the progress:
            Thread.Sleep(1000);

            LogMessage($"Device Ping Failed after {pingTask.NumTries} attempts");
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
