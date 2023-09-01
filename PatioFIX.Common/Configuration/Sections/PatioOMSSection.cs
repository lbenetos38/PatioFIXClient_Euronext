using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class PatioOMSSection
    {
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "PatioOMS";

        /// <summary>
        /// 
        /// </summary>
        public bool DisableDataLayer { get; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string ODLConnStr { get; }
        /// <summary>
        /// 
        /// </summary>
        public string TargetConnection { get; }
		/// <summary>
		/// Μετατρεπει τα πεδια τύπου UTCTimestamp/UTCTimeOnly σε Greek Standard (Local) Time
		/// </summary>
		public bool ConvertUTCTimeToLocal { get; } = true;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="required"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		internal PatioOMSSection(IConfigurationSection root, bool required = true)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["DisableDataLayer"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.DisableDataLayer = false;
                }
                else
                {
                    this.DisableDataLayer = Convert.ToBoolean(value);
                }


                value = section["ODLConnStr"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:ODLConnStr");
                }
                this.ODLConnStr = value;


                value = section["TargetConnection"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.TargetConnection = "ETS";
                }
                else
                {
                    //check if value is one of: ETS, ORA, *
                    value = value.Trim().ToUpperInvariant();
                    if (string.Compare(value, "ETS", false) != 0 && string.Compare(value, "ORA", false) != 0 && string.Compare(value, "*", false) != 0)
                    {
                        throw new ArgumentException($"Invalid value '{value}' for {section.Path}:TargetConnection");
                    }
                    this.TargetConnection = value.ToUpperInvariant();
                }


				value = section["ConvertUTCTimeToLocal"];
				if (string.IsNullOrWhiteSpace(value))
				{
					this.ConvertUTCTimeToLocal = true;
				}
				else
				{
					this.ConvertUTCTimeToLocal = Convert.ToBoolean(value);
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
            if (this.DisableDataLayer)
            {
                theLogger.Info("PatioOMS::\t\t\tDISABLED");
            }
            else
            {
                theLogger.Info("PatioOMS::\t\t\tENABLED");
                theLogger.Info($"PatioOMS::ODLConnStr = {this.ODLConnStr}");
                theLogger.Info($"PatioOMS::TargetConnection = {this.TargetConnection}");
				theLogger.Info($"PatioOMS::ConvertUTCTimeToLocal = {this.ConvertUTCTimeToLocal}");
			}
        }
    }
}
