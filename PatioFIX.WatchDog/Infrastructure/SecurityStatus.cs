using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatioFIX.WatchDog.Infrastructure
{
    internal class SecurityStatus
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
    }

}
