using PatioFIX.Common;
using PatioFIX.Common.Configuration;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace PatioFIX.Admin
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
                return Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Patio\\PatioFIXAdmin");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string FixSessionRootPath
        {
            get
            {
                return Path.Combine(CommonPath, "FixSessions");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static string NLogRootPath
        {
            get
            {
                return Path.Combine(CommonPath, "Logs");
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

            if (!Directory.Exists(FixSessionRootPath))
            {
                Directory.CreateDirectory(FixSessionRootPath);
            }

            if (!Directory.Exists(NLogRootPath))
            {
                Directory.CreateDirectory(NLogRootPath);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="theLogger"></param>
        static void EnsureAllLocalParameters(Logger theLogger)
        {
            try
            {
                string filePath = Path.Combine(LocalSystem.CommonPath, "instance.txt");
                if (File.Exists(filePath) == true)
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            string line = sr.ReadToEnd();
                            Globals.AppID = Guid.Parse(line.Substring(6));
                        }
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            Globals.AppID = Guid.NewGuid();

                            sw.WriteLine("AppID={0}", Globals.AppID.ToString("D"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                Globals.AppID = Guid.NewGuid();
            }
        }


        public static ClientStatus ReadStatusFromDB(Logger theLogger, int maxRetries = 5)
        {
            var retryCount = 0;

            theLogger.Info("ReadStatusFromDB()");
            if (Globals.PatioOMS.DisableDataLayer == true)
            {
                /*
                 * Επιστρεφουμε πισω ενα "ψευτικο" ClientStatus:
                 */
                var _now = DateTime.Now;
                return new ClientStatus() { CreateDT = _now, AppID = Globals.AppID, DayOfYear = _now.DayOfYear, DisableDataLayer = true };
            }

            var m_odlDataLayer = new OdlDataLayer();
            while (retryCount < maxRetries)
            {
                try
                {
                    var status = m_odlDataLayer.Clients_GetStatus(Globals.AppID, Globals.ClientRole, Globals.DayOfYear);

                    if (retryCount > 0)
                    {
                        theLogger.Info($"ReadStatusFromDB SUCCEEDED (retryCount = {retryCount})");
                    }

                    return status;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == -2/* Timeout expired*/)
                    {
                        retryCount++;
                        theLogger.Warning($"ReadStatusFromDB TIMEOUT HANDLED (retryCount = {retryCount}) {ex.Message}");
                    }
                    else if (ex.Number == 1205/*deadlock*/)
                    {
                        retryCount++;
                        theLogger.Warning($"ReadStatusFromDB DEADLOCK HANDLED (retryCount = {retryCount}) {ex.Message}");
                        Thread.SpinWait(retryCount * 200);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return new ClientStatus();
        }

        public static void DBHouseKeeping(Logger theLogger, int maxRetries = 5)
        {
            var retryCount = 0;

            theLogger.Info("DBHouseKeeping()");
            if (Globals.PatioOMS.DisableDataLayer == true)
            {
                Do_DBHouseKeeping = false;            //πετυχε, δεν θελουμε να την ξανακαλεσουμε
                return;
            }

            var m_odlDataLayer = new OdlDataLayer();
            while (retryCount < maxRetries)
            {
                try
                {
                    m_odlDataLayer.Clients_Housekeeping(Globals.AppID, Globals.ClientRole, Globals.DayOfYear);

                    if (retryCount > 0)
                    {
                        theLogger.Info($"DBHouseKeeping SUCCEEDED (retryCount = {retryCount})");
                    }

                    Do_DBHouseKeeping = false;        //πετυχε, δεν θελουμε να την ξανακαλεσουμε
                    return;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == -2/* Timeout expired*/)
                    {
                        retryCount++;
                        theLogger.Warning($"DBHouseKeeping TIMEOUT HANDLED (retryCount = {retryCount}) {ex.Message}");
                    }
                    else if (ex.Number == 1205/*deadlock*/)
                    {
                        retryCount++;
                        theLogger.Warning($"DBHouseKeeping DEADLOCK HANDLED (retryCount = {retryCount}) {ex.Message}");
                        Thread.SpinWait(retryCount * 200);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Do_DBHouseKeeping = true;                 //ΑΠΟΤΥΧΕ, πρεπει να κληθει ξανα
            return;
        }


        /// <summary>
        /// 
        /// </summary>
        public static bool IsInitialized { get; private set; }
        public static bool Do_DBHouseKeeping { get; internal set; }
        public static bool Do_StartOfDayTasks { get; internal set; }


        /// <summary>
        /// Εκτελείται πρώτα απο όλα τα υπολοιπα και φροντίζει για μερικά βασικά πράγματα
        /// (directories, log4net, ...)
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized == false)
            {
                /*
                 * To .NET (core) out of the box υποστηριζει μονο λιγα codepages:
                 *      Info.CodePage      Info.Name                    Info.DisplayName
                 *      1200               utf-16                       Unicode                      
                 *      1201               utf-16BE                     Unicode (Big-Endian)         
                 *      12000              utf-32                       Unicode (UTF-32)             
                 *      12001              utf-32BE                     Unicode (UTF-32 Big-Endian)    
                 *      20127              us-ascii                     US-ASCII                     
                 *      28591              iso-8859-1                   Western European (ISO)       
                 *      65000              utf-7                        Unicode (UTF-7)              
                 *      65001              utf-8                        Unicode (UTF-8)   
                 * 
                 * Εμεις ομεως χρειαζομαστε να διαβαζουμε και ASCII Ελληνικους χαρακτηρες (iso-8859-7) απο
                 * το ATHEX και δεν μας φτανουν οι παραπανω codepages. Θελουμε το framework να φορτωσει ολες
                 * τις διαθεσιμες codepages
                 * 
                 * The following statement, Provides access to an encoding provider for code 
                 * pages that otherwise are available only in the desktop .NET Framework.
                 * 
                 */
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


                /*
                 * Load Configuration
                 */
                PatioFIXClientConfiguration configuration;

                try
                {
                    configuration = new PatioFIXClientConfiguration("PatioFIXAdmin");
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
                    Globals.ServiceName = "PatioFIXAdmin";
                    Globals.ClientRole = ODLMesssageSource.Administrator;
                    Globals.InitializationDT = DateTime.Now;
                    Globals.SetDayOfYear();
                    Globals.SetConfiguration(configuration);
                    EnsureLocalDirectories();
                    Globals.Schedule.LoadSchedule(configuration.SimpleScheduler);

                    Logger.ConfigureLogger(Globals.ServiceName, LocalSystem.NLogRootPath);

                    Do_DBHouseKeeping = true; //αναγκαζουμε τον controller να καλεσει την DataBaseHouseKeeping
                    Do_StartOfDayTasks = true;


                    var theLogger = new Logger("LocalSystem");

                    EnsureAllLocalParameters(theLogger);
                    Globals.DumpSettings(theLogger);


                    #region load iso-8859-7
                    try
                    {
                        CharEncoding.DefaultEncoding = Encoding.GetEncoding("iso-8859-7");
                    }
                    catch (Exception ex)
                    {
                        theLogger.Warning("Error while GetEncoding(\"iso-8859-7\")");
                        theLogger.Error(ex.Message);
                    }
                    if (CharEncoding.DefaultEncoding == null)
                    {
                        try
                        {
                            CharEncoding.DefaultEncoding = Encoding.GetEncoding("iso-8859-1");
                        }
                        catch (Exception ex)
                        {
                            theLogger.Error("Error while GetEncoding(\"iso-8859-1\")");
                            theLogger.Error(ex.Message);
                            CharEncoding.DefaultEncoding = Encoding.ASCII;
                        }
                    }
                    #endregion


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
				 * αναγκαζουμε τον controller να καλεσει την DBHouseKeeping
				 */
                Do_DBHouseKeeping = true;
                /*
				 * αναγκαζουμε τον controller να τρεξει οτι χρειαζεται επειδη αλλαξε η μερα
				 */
                Do_StartOfDayTasks = true;
                /*
				 * κανω reset το Metrics.m_accumulator:
				 */
                MetricsProxy.Instance.ResetMetrics();
            }
        }
    }
}
