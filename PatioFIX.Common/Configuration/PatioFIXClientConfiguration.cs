using Microsoft.Extensions.Configuration;
using PatioFIX.Common.FixSupport;
using System;
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class PatioFIXClientConfiguration
    {
        const int ControllerTimer_MINVALUE = 3000;
        const int EventsListenerTimer_MINVALUE = 3000;
        const int FIXReconnectInterval_MINVALUE = 6000;

        /// <summary>
        /// 
        /// </summary>
        public string SettingsFilePath { get; }
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; }

        bool RequiredDispatcher { get; }


        /// <summary>
        /// 
        /// </summary>
        public TraceLevel LogLevel { get; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 ControllerTimer { get; } = ControllerTimer_MINVALUE;
        /// <summary>
        /// 
        /// </summary>
        public Int32 EventsListenerTimer { get; } = EventsListenerTimer_MINVALUE;
        /// <summary>
        /// The time interval between invocations of the FixClient.Start() method (milliseconds)
        /// </summary>
        public Int32 FIXReconnectInterval { get; } = FIXReconnectInterval_MINVALUE;
        /// <summary>
        /// 
        /// </summary>
        public bool RetryRetryableException { get; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool EnableMonitoring { get; }
        /// <summary>
        /// H IP του PatioFIX.WatchDog
        /// </summary>
        public string AggregatorServerIP { get; } = "127.0.0.1";
        /// <summary>
        /// 
        /// </summary>
        public int AggregatorServerPort { get; } = 41002;
        /// <summary>
        /// 
        /// </summary>
        public bool MonitorSecurityStatus { get; } = false;
        /// <summary>
        /// 
        /// </summary>
        public bool MonitorMarketStatus { get; } = false;



        /// <summary>
        /// 
        /// </summary>
        public FixServerSection FixServer { get; }
        /// <summary>
        /// 
        /// </summary>
        public FixSessionSection FixSession { get; }
        /// <summary>
        /// 
        /// </summary>
        public FixClientSection FixClient { get; }
        /// <summary>
        /// 
        /// </summary>
        public TCPConnectionSection TCPConnection { get; }
        /// <summary>
        /// 
        /// </summary>
        public PatioOMSSection PatioOMS { get; }
        /// <summary>
        /// 
        /// </summary>
        public EmulationSection Emulation { get; }
        /// <summary>
        /// 
        /// </summary>
        public SimpleSchedulerSection SimpleScheduler { get; }
        /// <summary>
        /// 
        /// </summary>
        public DispatcherSection Dispatcher { get; }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="requiredDispatcher"></param>
        /// <param name="settingsFilePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public PatioFIXClientConfiguration(string sectionName, bool requiredDispatcher = false, string settingsFilePath = "appsettings.json")
        {
            this.SettingsFilePath = settingsFilePath;
            this.SectionName = sectionName;
            this.RequiredDispatcher = requiredDispatcher;

            IConfigurationRoot config = new ConfigurationBuilder()
                 .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                 //.SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile(settingsFilePath)
                 .Build();


            var root = config.GetSection(sectionName);
            if (root.Exists() == false)
            {
                throw new ArgumentNullException($"There is no {sectionName} section in {settingsFilePath}");
            }



            //TraceLevel
            var value = root["TraceLevel"];
            if (string.IsNullOrWhiteSpace(value))
            {
                this.LogLevel = TraceLevel.Info;
            }
            else
            {
                if (value == "0")
                    this.LogLevel = TraceLevel.Off;
                else if (value == "1")
                    this.LogLevel = TraceLevel.Error;
                else if (value == "2")
                    this.LogLevel = TraceLevel.Warning;
                else if (value == "3")
                    this.LogLevel = TraceLevel.Info;
                else if (value == "4")
                    this.LogLevel = TraceLevel.Verbose;
                else
                    throw new ArgumentException($"TraceLevel '{value}' is invalid");
            }

            value = root["ControllerTimer"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.ControllerTimer = Int32.Parse(value);
            }
            if (this.ControllerTimer < ControllerTimer_MINVALUE)
            {
                throw new ArgumentOutOfRangeException($"Invalid value for {root.Path}:ControllerTimer. Cannot be smaller than {ControllerTimer_MINVALUE} ms");
            }

            value = root["EventsListenerTimer"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.EventsListenerTimer = Int32.Parse(value);
            }
            if (this.EventsListenerTimer < EventsListenerTimer_MINVALUE)
            {
                throw new ArgumentOutOfRangeException($"Invalid value for {root.Path}:EventsListenerTimer. Cannot be smaller than {EventsListenerTimer_MINVALUE} ms");
            }

            value = root["FIXReconnectInterval"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.FIXReconnectInterval = Int32.Parse(value);
            }
            if (this.FIXReconnectInterval < FIXReconnectInterval_MINVALUE)
            {
                throw new ArgumentOutOfRangeException($"Invalid value for {root.Path}:FIXReconnectInterval. Cannot be smaller than {FIXReconnectInterval_MINVALUE} ms");
            }

            value = root["RetryRetryableException"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.RetryRetryableException = Convert.ToBoolean(value);
            }


            value = root["EnableMonitoring"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.EnableMonitoring = Convert.ToBoolean(value);
            }
            value = root["AggregatorServerIP"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.AggregatorServerIP = value;
            }
            value = root["AggregatorServerPort"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.AggregatorServerPort = Convert.ToInt32(value);
            }

            value = root["MonitorSecurityStatus"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.MonitorSecurityStatus = Convert.ToBoolean(value);
            }
            value = root["MonitorMarketStatus"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.MonitorMarketStatus = Convert.ToBoolean(value);
            }



            try
            {
                this.FixServer = new FixServerSection(root, required: true);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a FixServerSection.", ex);
            }

            try
            {
                this.FixSession = new FixSessionSection(root, required: true);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a FixSessionSection.", ex);
            }

            try
            {
                this.FixClient = new FixClientSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a FixClientSection.", ex);
            }


            try
            {
                this.TCPConnection = new TCPConnectionSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a TCPConnectionSection.", ex);
            }

            try
            {
                this.PatioOMS = new PatioOMSSection(root, required: true);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a PatioOMSSection.", ex);
            }

            try
            {
                this.Emulation = new EmulationSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a EmulationSection.", ex);
            }



            try
            {
                this.Dispatcher = new DispatcherSection(root, required: requiredDispatcher);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a DispatcherSection.", ex);
            }

            try
            {
                this.SimpleScheduler = new SimpleSchedulerSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a SimpleSchedulerSection.", ex);
            }

        }


        /// <summary>
        /// Δημιουργει ενα FixConfiguration απο τις τρεχουσες ρυθμισεις
        /// </summary>
        /// <returns></returns>
        public FixConfiguration GetFixConfiguration()
        {
            var conf = new FixConfiguration();

            conf.ClientRole = Globals.ClientRole;
            conf.ServerIP = this.FixServer.ServerIP;
            conf.Port1 = this.FixServer.Port1;
            conf.Port2 = this.FixServer.Port2;
            conf.SSLEnable = this.FixServer.SSLEnable;
            conf.VerifyCertificate = this.FixServer.VerifyCertificate;
            conf.SSLServerName = this.FixServer.SSLServerName;

            conf.Version = this.FixSession.Version;
            conf.HeartbeatInterval = this.FixSession.HeartbeatInterval;
            conf.SenderCompID = this.FixSession.SenderCompID;
            conf.SenderSubID = this.FixSession.SenderSubID;
            conf.TargetCompID = this.FixSession.TargetCompID;
            conf.TargetSubID = this.FixSession.TargetSubID;
            conf.Username = this.FixSession.Username;
            conf.Password = this.FixSession.Password;

            conf.TcpNoDelay = this.TCPConnection.TcpNoDelay;
            conf.ReceiveBufferSize = this.TCPConnection.ReceiveBufferSize;
            conf.ReceiveTimeout = this.TCPConnection.ReceiveTimeout;
            conf.SendBufferSize = this.TCPConnection.SendBufferSize;
            conf.SendTimeout = this.TCPConnection.SendTimeout;
            conf.MaxRcvBuffer = this.TCPConnection.MaxRcvBuffer;


            conf.SessionTimerInterval = this.FixClient.SessionTimerInterval;
            conf.TCPReconnectInterval = this.FixClient.TCPReconnectInterval;
            conf.LogInboundMessages = this.FixClient.LogInboundMessages;
            conf.LogOutboundMessages = this.FixClient.LogOutboundMessages;
            conf.UseAlwaysResetGapFilling = this.FixClient.UseAlwaysResetGapFilling;


            #region Fix Message Validation
            conf.ValidateCheckSum = this.FixClient.ValidateCheckSum;
            conf.ValidateBodyLength = this.FixClient.ValidateBodyLength;
            conf.ValidateDuplicatedFields = this.FixClient.ValidateDuplicatedFields;
            conf.ValidateEmptyFieldValues = this.FixClient.ValidateEmptyFieldValues;
            conf.ValidateFieldValues = this.FixClient.ValidateFieldValues;
            conf.ValidateRepeatingGroupEntryCount = this.FixClient.ValidateRepeatingGroupEntryCount;
            conf.ValidateRepeatingGroupLeadingField = this.FixClient.ValidateRepeatingGroupLeadingField;
            conf.ValidateDuplicatePartyRole = this.FixClient.ValidateDuplicatePartyRole;
            conf.ValidateRequiredFields = this.FixClient.ValidateRequiredFields;
            conf.ValidateUnknownFields = this.FixClient.ValidateUnknownFields;
            conf.ValidateUnknownMessages = this.FixClient.ValidateUnknownMessages;
            #endregion

            conf.MaxMessageLength = this.FixClient.MaxMessageLength;
            conf.MaxMessageFields = this.FixClient.MaxMessageFields;

            return conf;
        }

        public void DumpSettings(Logger theLogger)
        {
            theLogger.Info($"LogLevel = {this.LogLevel}");
            theLogger.Info($"ControllerTimer = {this.ControllerTimer} milliseconds");
            theLogger.Info($"EventsListenerTimer = {this.EventsListenerTimer} milliseconds");
            theLogger.Info($"FIXReconnectInterval = {this.FIXReconnectInterval} milliseconds");
            theLogger.Info($"RetryRetryableException = {this.RetryRetryableException}");


            this.FixServer.DumpSettings(theLogger);
            this.FixSession.DumpSettings(theLogger);
            this.FixClient.DumpSettings(theLogger);
            this.TCPConnection.DumpSettings(theLogger);
            this.PatioOMS.DumpSettings(theLogger);
            this.Emulation.DumpSettings(theLogger);
            if (this.RequiredDispatcher)
            {
                this.Dispatcher.DumpSettings(theLogger);
            }
            //this.SimpleScheduler.DumpSettings(theLogger);

        }
    }
}
