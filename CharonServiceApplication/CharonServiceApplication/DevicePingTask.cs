using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CharonServiceApplication
{
    // Sends a ping to a device at a certain interval to confirm that it is online
    // A single Ping is composed of a 'On' request followed by a 'Off' request 'n' seconds
    // later. 
    internal class DevicePingTask
    {

        #region -- Singleton Pattern: --

        private static DevicePingTask _instance;

        protected DevicePingTask(string deviceIP)
        {
            DeviceIPAddress = deviceIP;

        }

        public static DevicePingTask Instance(string deviceIp)
        {
            // Uses Lazy initialization
            return _instance ?? (_instance = new DevicePingTask(deviceIp));
        }

        #endregion


        public int NumTries { get; } = 3;

        public static string DeviceIPAddress { get; private set; } = "http://192.168.0.0/";

        /// <summary>
        /// Executes a Device Ping. Tries a number of times based on the 
        /// 'NumTries setting' before giving up. 
        /// Executes synchronously... 
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public bool Execute(Action<string> logMessage)
        {
            var n = 0;

            while (n < NumTries)
            {
                n++;

                // This will be blocking! It might cause problems if we running in a ASP.NET context
                https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
                if (ExecutePingAsync(new Progress<string>(logMessage)).Result)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Executes a Device Ping Asynchronously. Tries a number of times based on the 
        /// 'NumTries setting' before giving up. 
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteAsync(Action<string> logMessage)
        {
            var bSuccess = false;

            var n = 0;

            while (n < NumTries)
            {
                n++;

               var pingresponse =  await ExecutePingAsync(new Progress<string>(logMessage));

                if (pingresponse)
                {
                    bSuccess = true;
                    break;
                }
                
            }

            return bSuccess;
        }


        #region -- Helper Methods --

        /// <summary>
        /// Executes a Single Ping on the device with a PingOn request immediately followed by a ping off request. 
        /// Makes the blue LED on the board blink. Ping delay controls how long the LED will stay On. 
        /// Can be called Asynchronously. Reports progress as it executes.
        /// </summary>
        /// <param name="progress">progress object to report on the progress</param>
        /// <param name="pingdelay">delay between ping on and ping Off (milliseconds)</param>
        /// <returns></returns>
        private static async Task<bool> ExecutePingAsync(IProgress<string> progress, int pingdelay = 2000 )
        {
            var bSuccess = false;
            // First send a Ping On:
            Task<string> pingtask =  PingAsync(true);

            progress?.Report($"Sending Ping On message to the device. Service Base address: {DeviceIPAddress}...");

            var pingResponse = await pingtask;

            if (pingResponse == "Success")
            {
                // wait for few seconds and send a ping Off
                await Task.Delay(pingdelay);
                
                pingtask = PingAsync(false);
                progress?.Report($"Sending Ping Off message to the device. Service Base Address: {DeviceIPAddress}...");
                pingResponse = await pingtask;

                progress?.Report(pingResponse == "Success" ? "Ping Complete" : $"Ping Failed! {pingResponse}");

                bSuccess = true;
            }
            else
            {
                progress?.Report($"Ping Failed! {pingResponse}" + Environment.NewLine);
            }

            return bSuccess;
        }

        
        private static async Task<string> PingAsync(bool @on)
        {
            
            var pingResponse = "";

            
           
  
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(DeviceIPAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
               // client.Timeout = TimeSpan.FromMilliseconds(10000);

                try
                {
                    var response = @on
                                ? await client.GetAsync("PingOn")
                                : await client.GetAsync("PingOff");

                    if (response.IsSuccessStatusCode)
                    {
                        pingResponse = "Success";
                    }

                }
                catch (Exception x)
                {
                    // the request takes longer than 10 secs, it is timed out
                    pingResponse = x.Message;
                }
               

                return pingResponse;
            }               
        }

        #endregion 

    }
}
