using Microsoft.Extensions.Configuration;
using System;
using System.Net;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class FixServerSection
    {
        public string SectionName { get; } = "FixServer";

        /// <summary>
        /// ATHEX FIX Server IP
        /// </summary>
        public IPAddress ServerIP { get; }
        /// <summary>
        /// Main FIX Server Port
        /// </summary>
        public int Port1 { get; }
        /// <summary>
        /// Backup FIX Server Port
        /// </summary>
        public int Port2 { get; }


        /// <summary>
        /// 
        /// </summary>
        public bool SSLEnable { get; }

        #region Για το certificate του remote peer
        /// <summary>
        /// θα κανουμε validation του remote certificate 
        /// </summary>
        public bool VerifyCertificate { get; } = false;
        /// <summary>
        /// The name of the server the client is trying to connect to. 
        /// That name is used for server certificate validation. 
        /// </summary>
        public string SSLServerName { get; }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal FixServerSection(IConfigurationSection root, bool required = true)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["ServerIP"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:ServerIP");
                }
                if (!IPAddress.TryParse(value, out IPAddress ip))
                {
                    throw new ArgumentException($"Invalid ServerIP {value}");
                }
                this.ServerIP = ip;


                value = section["Port1"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:Port1");
                }
                this.Port1 = Int32.Parse(value);
                if (this.Port1 <= 1023 || this.Port1 > 65535)
                {
                    throw new ArgumentOutOfRangeException($"Invalid Port1 {value}");
                }


                value = section["Port2"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:Port2");
                }
                this.Port2 = Int32.Parse(value);
                if (this.Port2 <= 1023 || this.Port2 > 65535)
                {
                    throw new ArgumentOutOfRangeException($"Invalid Port2 {value}");
                }


                value = section["SSLEnable"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.SSLEnable = bool.Parse(value);
                }

                //value = section["VerifyCertificate"];
                //if (!string.IsNullOrWhiteSpace(value))
                //{
                //    this.VerifyCertificate = bool.Parse(value);
                //}


                value = section["SSLServerName"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.SSLServerName = value.Trim();
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
            theLogger.Info($"FixServer::ServerIP = {this.ServerIP}");
            theLogger.Info($"FixServer::Port1 = {this.Port1}");
            theLogger.Info($"FixServer::Port2 = {this.Port2}");
            theLogger.Info($"FixServer::SSLEnable = {this.SSLEnable}");
            theLogger.Info($"FixServer::SSLServerName = {this.SSLServerName}");
            theLogger.Info($"FixServer::VerifyCertificate = {this.VerifyCertificate}");
        }
    }
}
