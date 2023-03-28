using PatioFIX.Common;
using PatioFIX.WatchDog.Metrics;
using System;
using System.Diagnostics;
using System.Threading;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    internal class AdminMetrics : BaseMetrics
    {
        PerformanceCounter cpuCounter = null;
        PerformanceCounter ramCounter = null;


        public static readonly AdminMetrics Instance = new AdminMetrics();

        /// <summary>
        /// 
        /// </summary>
        AdminMetrics() : base("AdminMetrics")
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch (Exception ex)
            {
                theLogger.Error(ex.Message);
            }
            try
            {
                ramCounter = new PerformanceCounter("Memory", "Available MBytes", String.Empty);
            }
            catch (Exception ex)
            {
                theLogger.Error(ex.Message);
            }
        }



        public override MetricDataPoint ReadAccumulator()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                var c_accumulator = m_accumulator;
                m_accumulator.ResetSummaries();

                /*
				 * Here we take a snapshot of the machine's metrics
				 */
                if (cpuCounter != null) c_accumulator.CPU = cpuCounter.NextValue();
                if (ramCounter != null) c_accumulator.Memory = ramCounter.NextValue();

                //c_accumulator.Total_Users = m_totalUsers;

                return c_accumulator;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
    }


}