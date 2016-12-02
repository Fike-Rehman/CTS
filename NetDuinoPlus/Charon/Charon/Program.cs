using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace CTS.Charon
{
    public class Program
    {
        // digital Ports to set relay outputs
        private static OutputPort ledPort;   // On Borad LED Port
        private static OutputPort digiPort0; // controls the Relay1
        private static OutputPort digiPort1; // controls the Relay2

        // Analog input Ports to read in temp values
        private static AnalogInput aPort0;
        private static AnalogInput aPort1;
        private static AnalogInput aPort2;

        public static void Main()
        {
            // initialize all ports connected to the dual 
            // channel relay and temp sensors

            ledPort = new OutputPort(Pins.ONBOARD_LED, false);
            digiPort0 = new OutputPort(Pins.GPIO_PIN_D0, true); 
            digiPort1 = new OutputPort(Pins.GPIO_PIN_D1, true);

            aPort0 = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
            aPort1 = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);
            aPort2 = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

            // wait few seconds for netduino to get the ip address and then display the IP address:
            Thread.Sleep(5000);
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface networkInterface =
                         Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
            Debug.Print("Board's IP Address:" + networkInterface.IPAddress.ToString());

            // if a valid IP address is successfully obtained then open a listener socket and 
            // start processing incoming requests:

            if (!networkInterface.IPAddress.ToString().Equals("0.0.0.0"))
            {
                // Create listener Socket and start listening on port 80

                Socket listenerSocket = new Socket(AddressFamily.InterNetwork,
                                              SocketType.Stream,
                                              ProtocolType.Tcp);

                IPEndPoint listenerEndPoint = new IPEndPoint(IPAddress.Any, 80);

                // bind to listening socket and start listening for incoming connections: 
                listenerSocket.Bind(listenerEndPoint);
                listenerSocket.Listen(1);

                // Process incoming requests on the socket created above
                ProcessRequests(listenerSocket);
            }
            else
            {
                Debug.Print("NetDuino Failed to obtain a valid IP Address!");
                Debug.Print("Exiting Program...");
                return;
            }
        }

        /// <summary>
        /// Continoulsy listens for incoming HTTP requests on the given listener socket and responds to 
        /// those requests
        /// </summary>
        /// <param name="listenerSocket"></param>
        private static void ProcessRequests(Socket listenerSocket)
        {
            while (true)
            {
                // wait for the client to connect
                Socket clientSocket = listenerSocket.Accept();

                // wait for data to arrive:
                bool dataReady = clientSocket.Poll(5000000, SelectMode.SelectRead);

                if (dataReady && clientSocket.Available > 0)
                {
                    byte[] buffer = new byte[clientSocket.Available];
                    int BytesRead = clientSocket.Receive(buffer);

                    string request =
                        new string(System.Text.Encoding.UTF8.GetChars(buffer));

                    string statusText = "Command Not Recognized!";


                    if (request.IndexOf("PingOn") > 0)
                    {
                        ledPort.Write(true);
                        statusText = "On Board LED On";

                    }
                    else if (request.IndexOf("PingOff") > 0)
                    {
                        ledPort.Write(false);
                        statusText = "On Board LED Off";
                    }
                    else if (request.IndexOf("EnergizeR1") > 0)
                    {
                        digiPort0.Write(false);
                        statusText = "R1 Energized";
                    }
                    else if (request.IndexOf("DenergizeR1") > 0)
                    {
                        digiPort0.Write(true);
                        statusText = "R1 De-energized";
                    }
                    else if (request.IndexOf("EnergizeR2") > 0)
                    {
                        digiPort1.Write(false);
                        statusText = "R2 Energized";
                    }
                    else if (request.IndexOf("DenergizeR2") > 0)
                    {
                        digiPort1.Write(true);
                        statusText = "R2 De-energized";
                    }
                    else if (request.IndexOf("GetT1FValue") > 0)
                    {
                        statusText = "Current Temp Value (F) for T1: " + GetTempF(0).ToString();
                    }
                    else if (request.IndexOf("GetT2FValue") > 0)
                    {
                        statusText = "Current Temp Value (F) for T2: " + GetTempF(1).ToString();
                    }
                    else if (request.IndexOf("GetT3FValue") > 0)
                    {
                        statusText = "Current Temp Value (F) for T3: " + GetTempF(2).ToString();
                    }

                    Debug.Print(statusText);

                    // return a message to the client telling it about the LED status
                    string response = "HTTP/1.1 200 OK\r\n" +
                                      "Content-Type: text/html; charset=utf-8\r\n\r\n" +
                                      "<html><head><title>NetDuino Plus Charon Service...</title></head>" +
                                      "<body>" + statusText + "</body></html>";

                    clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(response));
                }


                clientSocket.Close();

            } // end while
        } // end method

        /// <summary>
        /// Returns the current Celsius temp reading for the given temp Sensor 
        /// </summary>
        /// <param name="inputPort"> Temp Sensor analogPort</param>
        /// <returns></returns>
        private static double GetTempC(int inputPort)
        {
            var tempC = 0.00; 

            switch (inputPort)
            {
                case 0:
                    var reading0 = (int)aPort0.ReadRaw();
                    // convert reading to temp value
                    double voltage0 = (reading0 * 3.3) / 1024;
                    tempC = (voltage0 - 0.5) * 100;
                    break;

                case 1:
                    var reading1 = (int)aPort1.ReadRaw();
                    // convert reading to temp value
                    double voltage1 = (reading1 * 3.3) / 1024;
                    tempC = (voltage1 - 0.5) * 100;
                    break;


                case 2:
                    var reading2 = (int)aPort2.ReadRaw();
                    // convert reading to temp value
                    double voltage2 = (reading2 * 3.3) / 1024;
                    tempC = (voltage2 - 0.5) * 100;
                    break;
                
                default:
                    break;
            }

            return tempC;
        }

        /// <summary>
        /// Returns the current Fahrenheit temp reading for the given temp Sensor 
        /// </summary>
        /// <param name="inputPort">Temp Sensor analogPort</param>
        /// <returns></returns>
        private static double GetTempF(int inputPort)
        {
            var tempF = 0.00;

            tempF = (GetTempC(inputPort) * 9 / 5) + 32;

            return tempF;
        }

    }
}
