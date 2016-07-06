using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTS.Charon.Devices
{
    /// <summary>
    /// Interface provides methods for Pinging a Device in the System
    /// </summary>
    internal interface IPingTask
    {
        /// <summary>
        /// Asynchronous version of Ping Task
        /// </summary>
        /// <param name="progress"> Delegate to report method progress </param>
        /// <returns></returns>
        Task<bool> ExecutePingAsync(Action<string> progress);

        /// <summary>
        /// Synchronous verion of Ping task
        /// </summary>
        /// <param name="progress"> delegate to report method progress </param>
        /// <returns></returns>
        bool ExecutePing(Action<string> progress);
    }
}
