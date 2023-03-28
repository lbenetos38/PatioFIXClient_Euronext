using PatioFIX.Common;
using System;
using System.ServiceProcess;

namespace PatioFix.Broker
{
    partial class Actuator : ServiceBase
    {
        readonly Logger theLogger;

        public Actuator()
        {
            InitializeComponent();

            LocalSystem.Initialize();
            theLogger = new Logger("Actuator");
            this.ServiceName = Globals.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.AutoLog = false;

                theLogger.Info($"*****************************************************************************************");
                theLogger.Info($"***********************************{Globals.ServiceName} New Run*********************************");
                theLogger.Info($"{Globals.ServiceName} is starting by SCM at {DateTime.Now.ToString()}");
                theLogger.Info(string.Empty);

                TheController.Instance.Start();

                theLogger.Info(string.Format("{0} service STARTED!", Globals.ServiceName));
            }
            catch (Exception ex)
            {
                theLogger.Error(string.Format("{0} service DID NOT start! See the logs for more info!", Globals.ServiceName));
                theLogger.Error(ex);
                throw new Exception($"{Globals.ServiceName} failed to start. See the logs for more info!", ex);
            }

        }

        protected override void OnStop()
        {
            try
            {
                theLogger.Info($"*************************************************************");
                theLogger.Info($"{Globals.ServiceName} is stopping by SCM at {DateTime.Now.ToString()}");


                TheController.Instance.Quit();

                //if (!TheController.Instance.Quit())
                //{
                //    theLogger.Warning(string.Format("{0} service DID NOT stop gracefully", Globals.ServiceName));
                //    theLogger.Warning(string.Format("{0} service it will be killed by the SCM!", Globals.ServiceName));
                //}
                //else
                //{
                //    theLogger.Info(string.Format("{0} service stopped gracefully!", Globals.ServiceName));
                //}
            }
            catch (Exception ex)
            {
                theLogger.Warning(string.Format("{0} service DID NOT stop gracefully, error = {1}", Globals.ServiceName, ex.Message));
                theLogger.Warning(string.Format("{0} service it will be killed by the SCM!", Globals.ServiceName));
            }
        }
    }
}
