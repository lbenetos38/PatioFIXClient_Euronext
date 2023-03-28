using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    internal class WatchDogConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public string SettingsFilePath { get; }
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; }



        /// <summary>
        /// 
        /// </summary>
        public TraceLevel LogLevel { get; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 ControllerTimer { get; } = 3000;
        /// <summary>
        /// 
        /// </summary>
        public Int32 ListenPort { get; } = 41002;


        /// <summary>
        /// Οι ρυθμισεις μας για τον webserver του admin
        /// </summary>
        public AdminMonitorServerSection AdminMonitorServer { get; }

        /// <summary>
        /// Οι ρυθμισεις μας για τον webserver του broker
        /// </summary>
        public BrokerMonitorServerSection BrokerMonitorServer { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="settingsFilePath"></param>
        public WatchDogConfiguration(string sectionName, string settingsFilePath = "appsettings.json")
        {
            this.SettingsFilePath = settingsFilePath;
            this.SectionName = sectionName;

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


            value = root["ListenPort"];
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.ListenPort = Int32.Parse(value);
            }


            try
            {
                this.AdminMonitorServer = new AdminMonitorServerSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a AdminMonitorServerSection.", ex);
            }

            try
            {
                this.BrokerMonitorServer = new BrokerMonitorServerSection(root, required: false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating a BrokerMonitorServerSection.", ex);
            }
        }
    }


}
