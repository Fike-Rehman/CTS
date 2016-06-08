using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Reflection;
using System.IO;

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml.config", Watch = true)]

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace CTS.Charon.CharonApplication
{
    internal class Program
    {
        private static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string ApplicationName = AppDomain.CurrentDomain.FriendlyName;
        private const string ApplicationTitle = "Charon Service Application";

        private static readonly log4net.ILog _logger = 
                 log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            using (var service = new CharonService())
            {
                if (!Environment.UserInteractive)
                {
                    // running as service (service is already installed)
                    ServiceBase.Run(service);
                }
                else
                {
                    Launch(args, service.ServiceName);
                }
            }
        }

        private static void Launch(IReadOnlyList<string> args, string serviceName)
        {
            if (args.Count != 1)
            {
                Console.WriteLine("Exactly one argument must be provided");
                ShowHelp(serviceName);
                return;
            }

            switch (args[0].Trim().ToLower())
            {
                case "-console":
                case "-consol":
                case "-conso":
                case "-cons":
                case "-con":
                case "-co":
                case "-c":
                    // TODO: Introduce Dependency Injection:
                     new CharonApplication(true);
                     break;

                case "-install":
                case "-instal":
                case "-insta":
                case "-inst":
                case "-ins":
                case "-in":
                case "-i":

                    if (!ServiceManager.CheckIfServiceIsInstalled(serviceName))
                    {
                        // Service is not installed.
                        if (ServiceManager.IsUserAdmin())
                        {
                            // Install the service and start it.
                            var path = ApplicationPath + "\\" + ApplicationName;

                            _logger.Info("Attempting to install Charon Service to the Service Control Panel...");

                            if(ServiceManager.InstallService(path, serviceName, ApplicationTitle, true, true))
                            {
                                // Make sure service gets started:
                                using (var controller = new ServiceController(serviceName))
                                {
                                    if (controller.Status == ServiceControllerStatus.Stopped)
                                        controller.Start();
                                }
                            }  
                        }
                        else
                        {
                            _logger.Error("Failed to Install Service; User is not Administrator!");
                        }
                    }
                    else
                    {
                        _logger.Info("Service was already installed!");
                    }
                    break;

                case "-uninstall":
                case "-uninstal":
                case "-uninsta":
                case "-uninst":
                case "-unins":
                case "-unin":
                case "-uni":
                case "-un":
                case "-u":

                    if (ServiceManager.CheckIfServiceIsInstalled(serviceName))
                    {
                        if (ServiceManager.IsUserAdmin())
                        {
                            _logger.Info("Attempting to Un-Install Charon Service");

                            // let's first stop the service:
                            using (var controller = new ServiceController(serviceName))
                            {
                                if (controller.Status != ServiceControllerStatus.Stopped)
                                    controller.Stop();
                            }

                            if (ServiceManager.UNInstallService(serviceName))
                            {
                                _logger.Info("Service Uninstall successful!");
                            }
                        }
                        else
                        {
                            _logger.Error("Failed to Un-Install Service; User is not Administrator!");
                        }
                    }
                   
                    break;
                    

                #region commented out command line arguments
                //case "-start":
                //case "-star":
                //    using (ServiceController controller = new ServiceController(ServiceName))
                //    {
                //        controller.Start();
                //    }
                //    return;

                //case "-stop":
                //case "-sto":
                //    using (ServiceController controller = new ServiceController(ServiceName))
                //    {
                //        controller.Stop();
                //    }
                //    return;

                //case "-status":
                //case "-statu":
                //case "-stat":
                //    using (ServiceController controller = new ServiceController(ServiceName))
                //    {
                //        //try
                //        //{
                //        //   // WriteLog(oController.Status.ToString());
                //        //}
                //        //catch (Win32Exception e)
                //        //{
                //        //    WriteLog(e);
                //        //}
                //        //catch (InvalidOperationException e)
                //        //{
                //        //    WriteLog(e);
                //        //}
                //    }
                //    return;
                 #endregion

                case "-help":
                case "-hel":
                case "-he":
                case "-h":
                case "-?":
                    ShowHelp(serviceName);
                    return;

                default:
                    ShowHelp(serviceName);
                    return;
            }   
        }


        private static void ShowHelp(string serviceName)
        {
            Console.WriteLine("= usage:");
            Console.WriteLine($"=   {serviceName} -install   == install service");
            Console.WriteLine($"=   {serviceName} -uninstall == uninstall service");
            Console.WriteLine($"=   {serviceName} -start     == start service");
            Console.WriteLine($"=   {serviceName} -stop      == stop service");
            Console.WriteLine($"=   {serviceName} -status    == get the current status of the service");
            Console.WriteLine($"=   {serviceName} -console   == run in console mode");
            Console.WriteLine($"=   {serviceName} -help      == show this help message");
        }
    }
}
