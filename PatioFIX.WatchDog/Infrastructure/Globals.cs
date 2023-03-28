using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PatioFIX.WatchDog
{
    internal static class Globals
    {
        public static bool IsGuiPresent;
        public static IMonitor GuiMonitorInstance;
        public static int DayOfYear;



        #region PatioFIXAdminConfiguration
        /// <summary>
        /// 
        /// </summary>
        public static WatchDogConfiguration Configuration;


        /// <summary>
        /// 
        /// </summary>
        public static TraceLevel LogLevel => Configuration.LogLevel;
        /// <summary>
        /// 
        /// </summary>
        public static int ControllerTimer => Configuration.ControllerTimer;

        /// <summary>
        /// Η πορτα που ακουει ο AggregatorServer που ακουει
        /// </summary>
        public static int ListenPort => Configuration.ListenPort;
        /// <summary>
        /// 
        /// </summary>
        public static AdminMonitorServerSection AdminMonitorServer => Configuration.AdminMonitorServer;
        /// <summary>
        /// 
        /// </summary>
        public static BrokerMonitorServerSection BrokerMonitorServer => Configuration.BrokerMonitorServer;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(WatchDogConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion


        public static DateTime InitializationDT { get; set; }

        public static string ServiceName { get; } = "PatioFIX.WatchDog";


        public static void DumpSettings(Logger theLogger)
        {
            theLogger.Info($"----------------------------------DumpSettings START----------------------------------");
            theLogger.Info($"Globals::ServiceName = {Globals.ServiceName}");
            theLogger.Info($"Globals.DayOfYear = {Globals.DayOfYear}");

            Configuration.AdminMonitorServer.DumpSettings(theLogger);
            Configuration.BrokerMonitorServer.DumpSettings(theLogger);

            theLogger.Info("-----------------------------------DumpSettings END-----------------------------------");
        }





        public static void SetDayOfYear()
        {
            Interlocked.Exchange(ref DayOfYear, DateTime.Now.DayOfYear);
        }


        public static string UnWindException(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
            Exception _ex = ex.InnerException;
            while (_ex != null)
            {
                sb.Append(_ex.Message);
                _ex = _ex.InnerException;
            }

            return sb.ToString();
        }
    }
}
