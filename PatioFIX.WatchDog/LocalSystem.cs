using PatioFIX.WatchDog.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;

namespace PatioFIX.WatchDog
{
    internal static class LocalSystem
    {
        /// <summary>
        /// 
        /// </summary>
        public static string CommonPath
        {
            get
            {
                return Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Patio\\PatioFIXWatchDog");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        static string NLogRootPath
        {
            get
            {
                return CommonPath;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        static void EnsureLocalDirectories()
        {
            if (!Directory.Exists(CommonPath))
            {
                Directory.CreateDirectory(CommonPath);
            }

            if (!Directory.Exists(NLogRootPath))
            {
                Directory.CreateDirectory(NLogRootPath);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public static bool IsInitialized { get; private set; }
        public static bool Do_Every24hoursStuff { get; internal set; }




        /// <summary>
        /// Εκτελείται πρώτα απο όλα τα υπολοιπα και φροντίζει για μερικά βασικά πράγματα
        /// (directories, log4net, ...)
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized == false)
            {
                WatchDogConfiguration configuration;

                try
                {
                    configuration = new WatchDogConfiguration("PatioFIXWatchDog");
                }
                catch (Exception ex)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        var msg = Globals.UnWindException(ex);
                        eventLog.WriteEntry($"{Globals.ServiceName}\nException in LocalSystem.Initialize()\nPatioFIXClientConfiguration throw an exception\nMessage = {msg}", EventLogEntryType.Error);
                    }
                    throw;
                }

                try
                {
                    Globals.InitializationDT = DateTime.Now;
                    Globals.SetDayOfYear();
                    Globals.SetConfiguration(configuration);
                    EnsureLocalDirectories();

                    Logger.ConfigureLogger(Globals.ServiceName, LocalSystem.NLogRootPath);

                    Do_Every24hoursStuff = true;


                    var theLogger = new Logger("LocalSystem");

                    Globals.DumpSettings(theLogger);

                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        var msg = Globals.UnWindException(ex);
                        eventLog.WriteEntry($"{Globals.ServiceName}\nException in LocalSystem.Initialize()\nMessage = '{msg}'", EventLogEntryType.Error);
                    }
                    throw;
                }

            }
        }



        public static void PeriodicTasks(Logger theLogger)
        {
            var prvDayOfYear = Globals.DayOfYear;
            Globals.SetDayOfYear();

            /*
			 * Εαν αλλαξε η μέρα πρεπει να εκτελεσω καποια πραγματα:
			 */
            if (prvDayOfYear != Globals.DayOfYear)
            {
                theLogger.Info($"PeriodicTasks (prvDayOfYear:{prvDayOfYear} != Globals.DayOfYear:{Globals.DayOfYear})");

                /*
				 * αναγκαζουμε τον controller να τρεξει οτι χρειαζεται επειδη αλλαξε η μερα
				 */
                Do_Every24hoursStuff = true;
                /*
				 * κανω reset το Metrics.m_accumulator:
				 */
                AdminMetrics.Instance.ResetMetrics();
                BrokerMetrics.Instance.ResetMetrics();
                SecurityStatusDB.Instance.Reset();
            }
        }
    }
}
