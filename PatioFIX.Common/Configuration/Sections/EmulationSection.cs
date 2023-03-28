using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class EmulationSection
    {
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "Emulation";

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; } = false;
        /// <summary>
        /// 
        /// </summary>
        public bool EmulateMessages { get; } = false;
        /// <summary>
        /// 
        /// </summary>
        public Int32 EmulatePausePeriod { get; } = 500;
        /// <summary>
        /// 
        /// </summary>
        public string FixMessagesFile { get; }
        /// <summary>
        /// 
        /// </summary>
        public int MessagesLoops { get; } = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentException"></exception>
        internal EmulationSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["Enable"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Enable = Convert.ToBoolean(value);
                }


                value = section["EmulateMessages"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.EmulateMessages = Convert.ToBoolean(value);
                }


                value = section["EmulatePausePeriod"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.EmulatePausePeriod = Int32.Parse(value);
                }

                value = section["FixMessagesFile"];
                if (this.Enable && this.EmulateMessages)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentNullException($"Invalid value for FixMessagesFile");
                    }
                    this.FixMessagesFile = value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(value) == false)
                    {
                        this.FixMessagesFile = value;
                    }
                }


                value = section["MessagesLoops"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.MessagesLoops = Int32.Parse(value);
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
                theLogger.Info("Emulation::\t\tENABLED");
                theLogger.Info($"Emulation::EmulateMessages = {this.EmulateMessages}");
                theLogger.Info($"Emulation::EmulatePausePeriod = {this.EmulatePausePeriod}");
                theLogger.Info($"Emulation::FixMessagesFile = '{this.FixMessagesFile}'");
            }
            else
            {
                theLogger.Info("Emulation::\t\tDISABLED");
            }
        }
    }
}
