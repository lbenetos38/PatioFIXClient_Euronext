using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Market Status ("CC")
    /// </summary>
    class MarketStatusMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου MarketStatusMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            TradingSessionID = message[Tags.TradingSessionID].AsChar;       //REQUIRED
            TradSesStatus = message[Tags.TradSesStatus].AsChar;             //REQUIRED
            SecurityExchange = message[Tags.SecurityExchange].AsString;     //REQUIRED
            MarketID = message[CustomTags.MarketID].AsChar;                 //REQUIRED
            BoardID = message[CustomTags.BoardID].AsChar;                   //REQUIRED

            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Market_Status;
        /// <summary>
        /// A 4-character alphanumeric field indicating host trading venue (market place). 
        /// The valid venue ids are specified in the International Standard ISO 10383.
        /// 
        /// XIPO    ->HELEX ELECTRONIC BOOK BUILDING
        /// XATH    ->ATHENS EXCHANGE S.A. CASH MARKET
        /// XADE    ->ATHENS EXCHANGE S.A. DERIVATIVES MARKET
        /// PBGR    ->PIRAEUS BANK - SYSTEMATIC INTERNALISER
        /// HOTC    ->HELLENIC EXCHANGE OTC MARKET
        /// HESP    ->HENEX S.A
        /// HEDE    ->HENEX FINANCIAL ENERGY MARKET - DERIVATIVES MARKET
        /// HDAT    ->ELECTRONIC SECONDARY SECURITIES MARKET (HDAT)
        /// ERBX    ->EUROBANK - SYSTEMATIC INTERNALISER
        /// ENAX    ->ATHENS EXCHANGE ALTERNATIVE MARKET
        /// ASEX    ->ATHENS STOCK EXCHANG
        /// ABFI    ->ALPHA BANK - SYSTEMATIC INTERNALISER
        /// AAPA    ->ATHENS EXCHANGE - APA
        /// </summary>
        public string SecurityExchange { get; private set; }
        /// <summary>
        /// A single character alphanumeric field indicating the trading market
        /// </summary>
        public char MarketID { get; private set; }
        /// <summary>
        /// BoardID: (OASIS): A single character alpha field that identifies the trading board
        /// Possible Values:
        /// “M” Main board
        /// “S” Special conditions board
        /// “B” Report Only board
        /// “F” Forced sales board
        /// </summary>
        public char BoardID { get; private set; }
        /// <summary>
        /// A single character alphanumeric field indicating the market/board status. 
        /// Possible values :
        /// For the main board:
        ///     ”P” Pre-Call
        ///     “J” Calculated projected opening price
        ///     “T” Continuous/Auction event
        ///     “C” Closing price trading
        ///     “R” Run-off
        ///     “E” End of trading
        ///     “H” Halt
        ///     “S” Stop (Used only in Auction Market)
        ///     “N” No Orders accepted until the next Status change(Used only for XNET interface)
        /// For the other boards:
        ///     “O” Open
        ///     “E” End
        /// </summary>
        public char TradingSessionID { get; private set; }

        /*
            TradSesStatus (Tag = 340, Type: int)
            State of the trading session.

         * 1 = Halted
         * 2 = Open
         * 3 = Closed
         * 4 = Pre-Open
         * 5 = Pre-Close
         */
        public char TradSesStatus { get; private set; }
    }
}
