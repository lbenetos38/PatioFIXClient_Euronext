using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// SecurityStatus (CA)
    /// </summary>
    class SecurityStatusMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου SecurityStatusMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            HaltStartTime = "000000000000";
            #endregion


            SecurityID = message[Tags.SecurityID].AsString;                 //REQUIRED
            SecurityIDSource = message[Tags.SecurityIDSource].AsChar;       //REQUIRED
            SecurityExchange = message[Tags.SecurityExchange].AsString;     //REQUIRED

            //NoSecurityAltID
            if (message.Contains(Tags.SecurityAltID, instance: 0))
            {
                SecurityCode = message[Tags.SecurityAltID].AsString;
            }
            //SecurityAltIDSource 

            SecurityStatus = message[CustomTags.SecurityStatus].AsChar;     //REQUIRED
            PhaseID = message[CustomTags.PhaseID].AsChar;                   //REQUIRED
            SecurityPrice = message[CustomTags.SecurityPrice].AsFloat;      //REQUIRED
            HaltReasonCode = message[CustomTags.ΑΤΗΕΧHaltReason].AsString;  //REQUIRED
            MarketID = message[CustomTags.MarketID].AsChar;                 //REQUIRED

            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            if (HaltReasonCode != "00")
            {
                HaltStartTime = this.Timestamp.Substring(8, 12);
            }



            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Security_Status;
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
        /// 
        /// XCYS    ->CYPRUS STOCK EXCHANGE - OTC
        /// XECM    ->MTF - CYPRUS EXCHANGE 
        /// </summary>
        public string SecurityExchange { get; private set; }
        /// <summary>
        /// A 15-character alphanumeric field, indicating security’s Exchange symbol
        /// </summary>
        public string SecurityID { get; private set; }
        /// <summary>
        /// SecurityIDSource (Tag = 22, Type: String)
        /// </summary>
        internal char SecurityIDSource;

        /// <summary>
        /// A 12-character alphanumeric field, indicating Bloomberg security’s identification
        /// </summary>
        public string SecurityCode { get; private set; }
        /// <summary>
        /// A single character flag that indicates security status
        /// Possible values are:
        ///         “A” Active
        ///         “N” Not active
        ///         “S” Suspended
        ///         “H” Halted
        ///         “R” Resumed (Resumed Pre-opening of a Halt)
        /// </summary>
        public char SecurityStatus { get; private set; }
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
        public char PhaseID { get; private set; }
        /// <summary>
        /// A 9-character numeric field that contain the security price whenever “Security Status” message is sent (see also Price).
        /// Possible values:
        /// At the beginning of the Pre-Opening phase - Start of day price
        /// At the beginning of the Opening phase - Zeroes
        /// At the beginning of the Continuous phase - Opening price
        /// At the beginning of the Closing phase - Zeroes
        /// At the beginning of the Closing price phase - Closing price
        /// At the End of Trading - Closing price
        /// At a resumption - Resumed Opening price
        /// In all other cases - Last sale price
        /// </summary>
        public double SecurityPrice { get; private set; }
        /// <summary>
        /// A 2-character alphanumeric field indicating the cause of the halt or suspension.
        /// Possible values:
        /// 00 not applicable
        /// 01 Halt
        /// 02 Ceiling
        /// 03 Floor
        /// 04 War
        /// 05 Politics
        /// 06 Technical
        /// 07 Terrorism
        /// 08 Various
        /// 09 Religion
        /// 10 Celebration
        /// 11 Earthquake
        /// 12 Volatility Interruption
        /// 13 Series Expiration
        /// </summary>
        public string HaltReasonCode { get; private set; }
        /// <summary>
        /// A 12-character numeric field indicating the time that Halt/Suspend occured
        /// </summary>
        public string HaltStartTime { get; private set; }
        /// <summary>
        /// A single character alphanumeric field indicating the trading market
        /// </summary>
        public char MarketID { get; private set; }
        public string Timestamp { get; private set; }
    }
}
