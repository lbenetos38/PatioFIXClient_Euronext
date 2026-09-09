using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class FixSessionSection
    {
        public string SectionName { get; } = "FixSession";

        /// <summary>
        /// 
        /// </summary>
        public string Version { get; }
        /// <summary>
        /// HeartBtInt (Tag = 108, Type: int) (seconds)
        /// The HeartBtInt (108) field is used to declare the timeout interval for generating heartbeats 
        /// (same value used by both sides).
        /// </summary>
        public int HeartbeatInterval { get; } = 30;
        /// <summary>
        /// SenderCompID (Tag = 49, Type: String)
        /// Assigned value used to identify firm sending message.
        /// </summary>
        public string SenderCompID { get; }
        /// <summary>
        /// SenderSubID (Tag = 50, Type: String)
        /// Assigned value used to identify specific message originator (desk, trader, etc.)
        /// </summary>
        public string SenderSubID { get; }
        /// <summary>
        /// TargetCompID (Tag = 56, Type: String)
        /// Assigned value used to identify receiving firm.
        /// </summary>
        public string TargetCompID { get; }
        /// <summary>
        /// TargetSubID (Tag = 57, Type: String)
        /// Assigned value used to identify specific individual or unit intended to receive message
        /// </summary>
        public string TargetSubID { get; }
        /// <summary>
        /// Username (Tag = 553, Type: String)
        /// Userid or username.
        /// </summary>
        public string Username { get; }
        /// <summary>
        /// Password (Tag = 554, Type: String)
        /// Password or passphrase.
        /// </summary>
        public string Password { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal FixSessionSection(IConfigurationSection root, bool required = true)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {
                var value = section["HeartbeatInterval"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.HeartbeatInterval = Int32.Parse(value);
                }

                value = section["Version"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for Version");
                }
                if (value != "FIX.4.2" && value != "FIX.4.3" && value != "FIX.4.4" && value != "FIX.5.0")
                {
                    throw new ArgumentException($"Unsupported version {value}");
                }
                this.Version = value;


                value = section["SenderCompID"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for SenderCompID");
                }
                this.SenderCompID = value;

                value = section["SenderSubID"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.SenderSubID = value;
                }


                value = section["TargetCompID"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for TargetCompID");
                }
                this.TargetCompID = value;


                value = section["TargetSubID"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.TargetSubID = value;
                }


                value = section["Username"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Username = value;
                }

                value = section["Password"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Password = value;
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
            theLogger.Info($"FixSession::Version = {this.Version}");
            theLogger.Info($"FixSession::SenderCompID = {this.SenderCompID}");
            theLogger.Info($"FixSession::TargetCompID = {this.TargetCompID}");
            theLogger.Info($"FixSession::Username = {this.Username}");
            theLogger.Info($"FixSession::Password = {this.Password}");
            theLogger.Info($"FixSession::HeartbeatInterval = {this.HeartbeatInterval} seconds");
        }
    }
}
