using PatioFIX.Common;
using PatioFIX.WatchDog.Metrics;
using System.Threading;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    internal class BrokerMetrics : BaseMetrics
    {


        public static readonly BrokerMetrics Instance = new BrokerMetrics();


        /// <summary>
        /// 
        /// </summary>
        BrokerMetrics() : base("BrokerMetrics")
        {

        }


        public override MetricDataPoint ReadAccumulator()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                var c_accumulator = m_accumulator;
                m_accumulator.ResetSummaries();

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