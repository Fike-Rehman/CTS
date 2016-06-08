using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Security.Principal;

namespace CTS.Charon.CharonApplication
{
    public sealed class NativeMethods
    {
        NativeMethods() { }
        #region DLLImport

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
                                                   int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
                                                   string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [DllImport("advapi32.dll")]
        internal static extern void CloseServiceHandle(IntPtr SCHANDLE);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);

        [DllImport("advapi32.dll")]
        internal static extern int DeleteService(IntPtr SVHANDLE);

        //[DllImport("kernel32.dll")]
        //public static extern int GetLastWin32Error();
        //[System.Runtime.InteropServices.DllImport("Kernel32")]
        //public extern static Boolean CloseHandle(IntPtr handle);
        #endregion DLLImport
    }
    /// <summary>
    /// Provide methods that allow for Installing/Uninstalling a service
    /// in the Windows Service Control Panel.
    /// </summary>
    public sealed class ServiceManager
    {
        ServiceManager()
        {

        }

        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="servicePath">The complete path of the service.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="serviceDisplayName">Display name of the service.</param>
        /// <param name="startNow"> </param>
        /// <param name="username">Username to run service as.</param>
        /// <param name="password">Password for username to run service.</param>
        /// <param name="autoStart"> </param>
        /// <returns>True if the process went thro successfully. False if there was any problem</returns>
        public static bool InstallService(string servicePath, string serviceName, string serviceDisplayName, bool autoStart, bool startNow, string username = null, string password = null)
        {
            #region Constants declaration.
            var SC_MANAGER_CREATE_SERVICE = 0x0002;
            var SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            var SERVICE_DEMAND_START = 0x00000003;
            var SERVICE_ERROR_NORMAL = 0x00000001;
            var STANDARD_RIGHTS_REQUIRED = 0xF0000;
            var SERVICE_QUERY_CONFIG = 0x0001;
            var SERVICE_CHANGE_CONFIG = 0x0002;
            var SERVICE_QUERY_STATUS = 0x0004;
            var SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            var SERVICE_START = 0x0010;
            var SERVICE_STOP = 0x0020;
            var SERVICE_PAUSE_CONTINUE = 0x0040;
            var SERVICE_INTERROGATE = 0x0080;
            var SERVICE_USER_DEFINED_CONTROL = 0x0100;
            var SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
            SERVICE_QUERY_CONFIG |
            SERVICE_CHANGE_CONFIG |
            SERVICE_QUERY_STATUS |
            SERVICE_ENUMERATE_DEPENDENTS |
            SERVICE_START |
            SERVICE_STOP |
            SERVICE_PAUSE_CONTINUE |
            SERVICE_INTERROGATE |
            SERVICE_USER_DEFINED_CONTROL);
            var SERVICE_AUTO_START = 0x00000002;
            #endregion Constants declaration.

            try
            {
                var dwStartType = SERVICE_AUTO_START;
                if (autoStart == false) dwStartType = SERVICE_DEMAND_START;

                var scHandle = NativeMethods.OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                if (scHandle.ToInt32() != 0)
                {
                    var svHandle = NativeMethods.CreateService(scHandle, serviceName, serviceDisplayName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, dwStartType, SERVICE_ERROR_NORMAL, servicePath, null, 0, null, username, password);
                    if (svHandle.ToInt32() == 0)
                    {
                        NativeMethods.CloseServiceHandle(scHandle);
                        return false;
                    }
                    if (startNow)
                    {
                        //now trying to start the service
                        var i = NativeMethods.StartService(svHandle, 0, null);
                        // If the value i is zero, then there was an error starting the service.
                        // note: error may arise if the service is already running or some other problem.
                        if (i == 0)
                        {
                            //Console.WriteLine("Couldnt start service");
                            return false;
                        }
                        NativeMethods.CloseServiceHandle(scHandle);
                        NativeMethods.CloseServiceHandle(svHandle);
                        return true;
                    }
                    NativeMethods.CloseServiceHandle(scHandle);
                    NativeMethods.CloseServiceHandle(svHandle);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
               // Logger.WriteException(ex, LoggerType.Error);
                return false;
            }
        }

        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public static bool UNInstallService(string svcName)
        {
            var GENERIC_WRITE = 0x40000000;
            var DELETE = 0x10000;
            var scHndl = NativeMethods.OpenSCManager(null, null, GENERIC_WRITE);
            if (scHndl.ToInt32() != 0)
            {

                var svcHndl = NativeMethods.OpenService(scHndl, svcName, DELETE);
                //Console.WriteLine(svc_hndl.ToInt32());
                if (svcHndl.ToInt32() != 0)
                {
                    var i = NativeMethods.DeleteService(svcHndl);
                    if (i != 0)
                    {
                        NativeMethods.CloseServiceHandle(scHndl);
                        NativeMethods.CloseServiceHandle(svcHndl);
                        return true;
                    }
                    NativeMethods.CloseServiceHandle(scHndl);
                    NativeMethods.CloseServiceHandle(svcHndl);
                    return false;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Determines if a specific Windows service is installed
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public static bool CheckIfServiceIsInstalled(string applicationName)
        {
            var controllers = ServiceController.GetServices();

            return controllers.Any(con => con.ServiceName == applicationName);
        }


        /// <summary>
        /// Determines if User has admin right to install/UnInstall service
        /// </summary>
        /// <returns></returns>
        public static bool IsUserAdmin()
        {
            //bool value to hold our return value 
            bool isAdmin;
            try
            {
                //get the currently logged in user 
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
