using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatioFIX.WatchDog.Infrastructure
{
    internal class SecurityStatusDB
    {
        /// <summary>
        /// 
        /// </summary>
        public class SecurityStatus
        {
            public string SecurityExchange;
            public string SecurityID;

            /// <summary>
            /// A single character flag that indicates security status
            /// Possible values are:
            ///         “A” Active
            ///         “N” Not active
            ///         “S” Suspended
            ///         “H” Halted
            ///         “R” Resumed (Resumed Pre-opening of a Halt)
            /// </summary>
            public char Status;
            /// <summary>
            /// A single character alpha code that identifies the trading phase.
            /// Possible values are:
            /// “ ” Start of day (Before the Pre-opening)
            /// “P” Pre-opening Trading Phase
            /// “O” Opening Trading Phase
            /// “T” Continuous Trading Phase
            /// “A” At the Closing Price trading Phase
            /// “C” Closing Price Continuous Trading Phase
            /// “E” End Of Trading Phase
            /// “S” Stop phase (Use in auction market)
            /// </summary>
            public char PhaseID;
            /// <summary>
            /// A single character alphanumeric field indicating the trading market
            /// </summary>
            public char MarketID;
        }


        readonly Dictionary<string, SecurityStatus> m_securityStatuses = new Dictionary<string, SecurityStatus>();
        readonly System.Object m_lock = new object();

        public static readonly SecurityStatusDB Instance = new SecurityStatusDB();


        SecurityStatusDB()
        {

        }
        /// <summary>
        /// Μοναδικα Securities
        /// </summary>
        public Int32 TotalSecurities;

        /// <summary>
        /// Μοναδικα Securities
        /// </summary>
        public Int32 Total_Enabled;
        /// <summary>
        /// Μοναδικα Securities
        /// </summary>
        public Int32 Total_Disabled;

        /// <summary>
        /// Securities with status 'A'
        /// </summary>
        public Int32 Total_SActive;
        /// <summary>
        /// Securities with status 'R'
        /// </summary>
        public Int32 Total_SResume;
        /// <summary>
        /// Securities with status 'S'
        /// </summary>
        public Int32 Total_SSuspended;
        /// <summary>
        /// Securities with status 'H'
        /// </summary>
        public Int32 Total_SHalted;
        /// <summary>
        /// Securities with status 'H'
        /// </summary>
        public Int32 Total_SNotActive;


        /// <summary>
        /// Active or Resumed Securities in ' ' phase
        /// Start of day (Before the Pre-opening)
        /// </summary>
        public Int32 Total_SOD;
        /// <summary>
        /// Active or Resumed Securities in 'P' phase
        /// Pre-opening Trading Phase
        /// </summary>
        public Int32 Total_P;
        /// <summary>
        /// Active or Resumed Securities in 'O' phase
        /// Opening Trading Phase
        /// </summary>
        public Int32 Total_O;
        /// <summary>
        /// Active or Resumed Securities in 'T' phase
        /// Continuous Trading Phase
        /// </summary>
        public Int32 Total_T;
        /// <summary>
        /// Active or Resumed Securities in 'A' phase
        /// At the Closing Price trading Phase
        /// </summary>
        public Int32 Total_A;
        /// <summary>
        /// Active or Resumed Securities in 'C' phase
        /// Closing Price Continuous Trading Phase
        /// </summary>
        public Int32 Total_C;
        /// <summary>
        /// Active or Resumed Securities in 'E' phase
        /// End Of Trading Phase
        /// </summary>
        public Int32 Total_EOD;
        /// <summary>
        /// Active or Resumed Securities in 'S' phase
        /// Stop phase (Use in auction market)
        /// </summary>
        public Int32 Total_S;

        public void PushStatus(string exchange, string securityID, char status, char phase, char marketID)
        {
            if(m_securityStatuses.ContainsKey(securityID))
            {
                SecurityStatus ss = m_securityStatuses[securityID];

                UNSetCounters(ss.Status, ss.PhaseID);
                ss.Status = status;
                ss.PhaseID = phase;
                ss.MarketID = marketID;
            }
            else
            {
                SecurityStatus ss = new SecurityStatus() { MarketID = marketID, PhaseID = phase, Status = status, SecurityExchange = exchange, SecurityID = securityID };
                m_securityStatuses.Add(securityID, ss);

                Interlocked.Increment(ref TotalSecurities);    
            }

            SetCounters(status, phase);
        }


        void SetCounters(char status, char phase)
        {
            bool is_enabled = false;

            if(status == 'A')
            {
                Interlocked.Increment(ref Total_SActive);
                Interlocked.Increment(ref Total_Enabled);
                is_enabled = true;
            }
            else if (status =='R')
            {
                Interlocked.Increment(ref Total_SResume);
                Interlocked.Increment(ref Total_Enabled);
                is_enabled = true;
            }
            else if(status == 'S')
            {
                Interlocked.Increment(ref Total_SSuspended);
                Interlocked.Increment(ref Total_Disabled);
            }
            else if (status == 'H')
            {
                Interlocked.Increment(ref Total_SHalted);
                Interlocked.Increment(ref Total_Disabled);
            }
            else if (status == 'N')
            {
                Interlocked.Increment(ref Total_SNotActive);
                Interlocked.Increment(ref Total_Disabled);
            }

            if(is_enabled)
            {
                if(phase == ' ')
                {
                    Interlocked.Increment(ref Total_SOD);
                }
                else if (phase == 'P')
                {
                    Interlocked.Increment(ref Total_P);
                }
                else if (phase == 'O')
                {
                    Interlocked.Increment(ref Total_O);
                }
                else if (phase == 'T')
                {
                    Interlocked.Increment(ref Total_T);
                }
                else if (phase == 'A')
                {
                    Interlocked.Increment(ref Total_A);
                }
                else if (phase == 'C')
                {
                    Interlocked.Increment(ref Total_C);
                }
                else if (phase == 'E')
                {
                    Interlocked.Increment(ref Total_EOD);
                }
                else if (phase == 'S')
                {
                    Interlocked.Increment(ref Total_S);
                }
            }

        }


        void UNSetCounters(char status, char phase)
        {
            bool is_enabled = false;

            if (status == 'A')
            {
                Interlocked.Decrement(ref Total_SActive);
                Interlocked.Decrement(ref Total_Enabled);
                is_enabled = true;
            }
            else if (status == 'R')
            {
                Interlocked.Decrement(ref Total_SResume);
                Interlocked.Decrement(ref Total_Enabled);
                is_enabled = true;
            }
            else if (status == 'S')
            {
                Interlocked.Decrement(ref Total_SSuspended);
                Interlocked.Decrement(ref Total_Disabled);
            }
            else if (status == 'H')
            {
                Interlocked.Decrement(ref Total_SHalted);
                Interlocked.Decrement(ref Total_Disabled);
            }
            else if (status == 'N')
            {
                Interlocked.Decrement(ref Total_SNotActive);
                Interlocked.Decrement(ref Total_Disabled);
            }

            if (is_enabled)
            {
                if (phase == ' ')
                {
                    Interlocked.Decrement(ref Total_SOD);
                }
                else if (phase == 'P')
                {
                    Interlocked.Decrement(ref Total_P);
                }
                else if (phase == 'O')
                {
                    Interlocked.Decrement(ref Total_O);
                }
                else if (phase == 'T')
                {
                    Interlocked.Decrement(ref Total_T);
                }
                else if (phase == 'A')
                {
                    Interlocked.Decrement(ref Total_A);
                }
                else if (phase == 'C')
                {
                    Interlocked.Decrement(ref Total_C);
                }
                else if (phase == 'E')
                {
                    Interlocked.Decrement(ref Total_EOD);
                }
                else if (phase == 'S')
                {
                    Interlocked.Decrement(ref Total_S);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref TotalSecurities, 0);

            Interlocked.Exchange(ref Total_Enabled, 0);
            Interlocked.Exchange(ref Total_Disabled, 0);

            Interlocked.Exchange(ref Total_SActive, 0);
            Interlocked.Exchange(ref Total_SResume, 0);
            Interlocked.Exchange(ref Total_SSuspended, 0);
            Interlocked.Exchange(ref Total_SHalted, 0);
            Interlocked.Exchange(ref Total_SNotActive, 0);


            Interlocked.Exchange(ref Total_SOD, 0);
            Interlocked.Exchange(ref Total_P, 0);
            Interlocked.Exchange(ref Total_O, 0);
            Interlocked.Exchange(ref Total_T, 0);
            Interlocked.Exchange(ref Total_A, 0);
            Interlocked.Exchange(ref Total_C, 0);
            Interlocked.Exchange(ref Total_EOD, 0);
            Interlocked.Exchange(ref Total_S, 0);

            m_securityStatuses.Clear();
        }
    }
}
