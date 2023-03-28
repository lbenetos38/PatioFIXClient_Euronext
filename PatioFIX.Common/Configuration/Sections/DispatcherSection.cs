using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class DispatcherSection
    {
        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "Dispatcher";

        /// <summary>
        /// 
        /// </summary>
        public bool Disable { get; } = false;
        /// <summary>
        /// OrdersDispatcher: Καθε ποτε (expressed in milliseconds) ρωταει την βαση για νεες pending εντολες (Orders, Cancels, Updates)
        /// Valid values είναι απο 100 εως 10000 (Default 300)
        /// </summary>
        public Int32 WaitInterval { get; } = 300;
        /// <summary>
        /// 
        /// </summary>
        public Int32 TopRows { get; } = 60;
        /// <summary>
        /// Εαν τα αποτελεσματα του διαβασματος των pending εντολες (Orders, Cancels, Updates) ταξινομηθουν με βαση το WorkingDate
        /// </summary>
        public bool Use_GetPendingRecords_Merged { get; } = false;
        /// <summary>
        /// UnConfirmedPool - MaxUnconfirmedMessage
        /// </summary>
        public Int32 MaxUnconfirmedMessage { get; } = 6;
        /// <summary>
        /// UnConfirmedPool - AbandonedInterval
        /// </summary>
        public Int32 AbandonedInterval { get; } = 8;

        /// <summary>
        /// SendMessagesGuard
        /// </summary>
        public bool LogMessageGuard { get; } = false;
        /// <summary>
        /// UnConfirmedPool
        /// </summary>
        public bool LogUnConfirmedPool { get; } = false;
        /// <summary>
        /// UnConfirmedPool
        /// </summary>
        public bool LogUnConfirmedPoolMessages { get; } = false;



        internal DispatcherSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["Disable"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Disable = Convert.ToBoolean(value);
                }


                value = section["WaitInterval"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.WaitInterval = Int32.Parse(value);
                }

                value = section["TopRows"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.TopRows = Int32.Parse(value);
                }


                value = section["Use_GetPendingRecords_Merged"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.Use_GetPendingRecords_Merged = Convert.ToBoolean(value);
                }


                value = section["MaxUnconfirmedMessage"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.MaxUnconfirmedMessage = Int32.Parse(value);
                }

                value = section["AbandonedInterval"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.AbandonedInterval = Int32.Parse(value);
                }


                value = section["LogMessageGuard"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.LogMessageGuard = Convert.ToBoolean(value);
                }
                value = section["LogUnConfirmedPool"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.LogUnConfirmedPool = Convert.ToBoolean(value);
                }
                value = section["LogUnConfirmedPoolMessages"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.LogUnConfirmedPoolMessages = Convert.ToBoolean(value);
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
            theLogger.Info($"Dispatcher::Disable = {this.Disable}");
            theLogger.Info($"Dispatcher::WaitInterval = {this.WaitInterval} milliseconds");
            theLogger.Info($"Dispatcher::TopRows = {this.TopRows}");
            theLogger.Info($"Dispatcher::Use_GetPendingRecords_Merged = '{this.Use_GetPendingRecords_Merged}'");
            theLogger.Info($"Dispatcher::(UnConfirmedPool)MaxUnconfirmedMessage = {this.MaxUnconfirmedMessage}");
            theLogger.Info($"Dispatcher::(UnConfirmedPool)AbandonedInterval = {this.AbandonedInterval} seconds");
            theLogger.Info($"Dispatcher::LogMessageGuard = '{this.LogMessageGuard}'");
            theLogger.Info($"Dispatcher::LogUnConfirmedPool = '{this.LogUnConfirmedPool}'");
            theLogger.Info($"Dispatcher::LogUnConfirmedPoolMessages = '{this.LogUnConfirmedPoolMessages}'");
        }
    }
}
