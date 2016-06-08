using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CTS.Charon.CharonApplication
{
    public class CharonService : ServiceBase
    {
        CharonApplication _application;

        public const string Name = "CharonService";

        private static readonly log4net.ILog _logger =
                 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CharonService()
        {
            ServiceName = Name;
        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Starting Charon Service Application without console");
            _application = new CharonApplication(false);
        }

        protected override void OnStop()
        {
            _application.Stop();
        }
    }
}
