using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CharonServiceApplication
{
    // sends a ping to a device every minute to confirm that it is online
    // Ping is composed by a On request followed by a off request 'n' seconds
    // later where class needs to be singleton
    internal class DevicePingTask
    {
        //private readonly int _numTries = 3;

        public void SetUp()
        {
                
        }

        public int NumTries { get; } = 3;

        public bool Execute(Action<string> logMessage)
        {
            var n = 0;

            while (n < NumTries)
            {
                n++;

                // This will be blocking! It might cause problems if we running in a ASP.NET context
                https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
                if (ExecuteAsync(new Progress<string>(logMessage)).Result)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> ExecuteAsync(IProgress<string> progress )
        {
            var bSuccess = false;
            // First send a Ping On:
            Task<string> pingtask =  PingAsync(true);

            progress?.Report("Sending Ping On message to the device...");
            var pingResponse = await pingtask;

            if (pingResponse == "Success")
            {
                // wait for few seconds and send a ping Off
                await Task.Delay(2000);
                
                pingtask = PingAsync(false);
                progress?.Report("Sending Ping OFF message to device...");
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

        public void TearDown()
        {

        }

        private static async Task<string> PingAsync(bool @on)
        {
            var pingResponse = "";

            // TODO: read from config
            const string baseAddress = "http://192.168.0.200/";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
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

    }
}
