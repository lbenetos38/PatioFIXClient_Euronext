using PatioFIX.Common.Configuration;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PatioFIX.Common
{
    /// <summary>
    /// Εδω έχουμε το configuration για τα applications μας
    /// </summary>
    public static class Globals
    {
        public static bool IsGuiPresent;
        public static IMonitor GuiMonitorInstance;
        public static int DayOfYear;
        public static SimpleSchedule Schedule = new();




        #region PatioFIXAdminConfiguration
        public static PatioFIXClientConfiguration Configuration;
        public static TraceLevel LogLevel
        {
            get
            {
                if (Configuration != null)
                {
                    return Configuration.LogLevel;
                }

                return TraceLevel.Verbose;
            }
        }
        public static bool RetryRetryableException => Configuration.RetryRetryableException;

        public static FixServerSection FixServer => Configuration.FixServer;
        public static FixSessionSection FixSession => Configuration.FixSession;
        public static FixClientSection FixClient => Configuration.FixClient;
        public static PatioOMSSection PatioOMS => Configuration.PatioOMS;
        public static EmulationSection Emulation => Configuration.Emulation;

        public static DispatcherSection Dispatcher => Configuration.Dispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(PatioFIXClientConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        public static ODLMesssageSource ClientRole { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public static Guid AppID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static DateTime InitializationDT { get; set; }

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public static string ServiceName { get; set; }

        public static string ETS_SiteMemberId = "EB";
        public static string ETS_SiteTraderId = "EB00C";
        public static string ETS_SiteClearingMemberId = "EB11";
        public static string ORA_SiteMemberId = "AT";
        public static string ORA_SiteTraderId = "EB00C";
        public static string ORA_SiteClearingMemberId = "EB00";


        public static void DumpSettings(Logger theLogger)
        {
            theLogger.Info($"----------------------------------DumpSettings START----------------------------------");
            theLogger.Info($"Globals::ServiceName = {Globals.ServiceName}");
            theLogger.Info($"Globals::ClientRole = {Globals.ClientRole}");
            theLogger.Info($"Globals::AppID = {Globals.AppID.ToString("D")}");
            theLogger.Info($"Globals.DayOfYear = {Globals.DayOfYear}");

            Configuration.DumpSettings(theLogger);
            Schedule.DumpSchedule(theLogger);


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
