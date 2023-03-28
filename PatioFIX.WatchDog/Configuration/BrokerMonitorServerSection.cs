using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    internal class BrokerMonitorServerSection
    {
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "BrokerMonitorServer";

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; } = true;
        /// <summary>
        /// 
        /// </summary>
        public string URI { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool LogRequests { get; } = false;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal BrokerMonitorServerSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["Enable"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Enable = Convert.ToBoolean(value);
                }



                value = section["URI"];
                if (this.Enable)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException($"Invalid value for {section.Path}:URI");
                    }
                    this.URI = value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(value) == false)
                    {
                        this.URI = value;
                    }
                }


                value = section["LogRequests"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.LogRequests = Convert.ToBoolean(value);
                }

            }
            else
            {
                if (required)
                {
                    throw new ArgumentException($"There is no {section.Path} section but is a required one");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="theLogger"></param>
        internal void DumpSettings(Logger theLogger)
        {
            if (this.Enable)
            {
                theLogger.Info("BrokerMonitorServer::\tENABLED");
                theLogger.Info($"BrokerMonitorServer::URI = {this.URI}");
            }
            else
            {
                theLogger.Info("BrokerMonitorServer::\tDISABLED");
            }
        }
    }
}
