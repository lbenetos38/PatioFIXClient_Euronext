using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Diagnostics;
using System.IO;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Logger
    {
        readonly ILogger _nlogLogger = null;
        readonly IMonitor m_monitor = null;
        readonly string m_ownerName = string.Empty;
        TraceLevel m_traceLevel = TraceLevel.Error;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputPath"></param>
        public static void ConfigureLogger(string serviceName, string outputPath)
        {
            var config = new LoggingConfiguration();


            var fileTarget = new FileTarget
            {
                Name = "logfile",
                FileNameKind = FilePathKind.Absolute,
                //FileName = Path.Combine(outputPath, $"{serviceName}.${{cached:cached=true:Inner=${{date:format=yyyy-MM-dd:CacheKey=${{shortdate}}.log"),
                FileName = Path.Combine(outputPath, $"{serviceName}.log"),
                //Layout = "${longdate} ${uppercase:${level:padding=-6:fixedLength=true}} ${logger:padding=-14} ${message} ${exception:format=tostring}",
                Layout = "${time} ${uppercase:${level:padding=-6:fixedLength=true}} ${logger:padding=-15} ${message} ${exception:format=tostring}",
                CreateDirs = true,
                KeepFileOpen = true,
                ConcurrentWrites = false,
                AutoFlush = true,

                //ArchiveFileName = Path.Combine(outputPath, "${date}.old"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                MaxArchiveFiles = 60
            };

            var asyncFileTarget = new AsyncTargetWrapper(fileTarget)
            {
                Name = fileTarget.Name,
                QueueLimit = 10000,
                OverflowAction = NLog.Targets.Wrappers.AsyncTargetWrapperOverflowAction.Grow
            };

            //Notice that AddRule internally automatically calls AddTarget
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, asyncFileTarget, "*");

            LogManager.Configuration = config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerName"></param>
        public Logger(string ownerName)
        {
            m_ownerName = ownerName;

            if (Globals.IsGuiPresent)
            {
                m_monitor = Globals.GuiMonitorInstance;
            }
            m_traceLevel = Globals.LogLevel;

            _nlogLogger = LogManager.GetLogger(ownerName);
        }



        public void Error(string msg)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowMessage(TraceLevel.Error, msg, m_ownerName);

            if (m_traceLevel >= TraceLevel.Error)
            {
                _nlogLogger.Error(msg);
            }
        }

        public void Error(Exception ex)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowException(TraceLevel.Error, ex, m_ownerName);

            if (m_traceLevel >= TraceLevel.Error)
            {
                string fullmsg = Globals.UnWindException(ex);

                _nlogLogger.Error(ex, fullmsg);
            }
        }


        public void Warning(string msg)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowMessage(TraceLevel.Warning, msg, m_ownerName);

            if (m_traceLevel >= TraceLevel.Warning)
            {
                _nlogLogger.Warn(msg);
            }
        }
        public void Warning(Exception ex)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowException(TraceLevel.Warning, ex, m_ownerName);


            if (m_traceLevel >= TraceLevel.Warning)
            {
                string fullmsg = Globals.UnWindException(ex);

                _nlogLogger.Warn(ex, fullmsg);
            }
        }


        public void Info(string msg)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowMessage(TraceLevel.Info, msg, m_ownerName);


            if (m_traceLevel >= TraceLevel.Info)
            {
                _nlogLogger.Info(msg);
            }
        }


        public void Verbose(string msg)
        {
            //Στο monitor στέλνουμε τα πάντα χωρίς έλεγχο για το TraceLevel
            if (m_monitor != null)
                m_monitor.ShowMessage(TraceLevel.Verbose, msg, m_ownerName);


            if (m_traceLevel >= TraceLevel.Verbose)
            {
                _nlogLogger.Debug(msg);
            }
        }



    }
}
