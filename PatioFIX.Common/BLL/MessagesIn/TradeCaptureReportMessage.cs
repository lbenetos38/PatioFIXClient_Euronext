using PatioFIX.Common.BLL.MessagesIn;
using PatioFIX.Common.FixSupport;
using System.Collections.Generic;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Η παρακατω class μοιαζει σε πολλα σημέια με την NewTradeConfirmationMessage (TF).
    /// Στην βαση μας θα καταχωρηθει σαν ενα απλο Trade 
    /// </summary>
    class TradeCaptureReportMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Αναπαριστα ενα side ενος "Trade Capture Report"
        /// </summary>
        internal class TradeSide
        {
            PartiesContainer m_parties = new PartiesContainer("TradeCaptureReportMessage", Globals.FixClient.ValidateDuplicatePartyRole, Globals.FixClient.ValidateRepeatingGroupEntryCount);


            public void EmptyValues()
            {
                this.Side = default;
                this.Account = default;
                this.OrderOrigination = default;
                this.TradingCapacity = default;

                m_parties.EmptyValues();

                this.PositionEffect = default;

                this.NoOrderAttributes = 0;
                this.Attributes.Clear();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            /// <param name="logger"></param>
            /// <param name="instance"></param>
            public void ParseFixMessage(FIXMessage message, Logger logger, int instance = 0)
            {
                #region Side (Tag = 54, Type: char)
                var _cvalue = message[Tags.Side, instance].AsChar;
                if (_cvalue == '1'/*Buy*/)
                    this.Side = 'B';
                else if (_cvalue == '2'/*Sell*/)
                    this.Side = 'S';
                else if (_cvalue == '5'/*Sell short*/)
                    this.Side = 'S';
                else if (_cvalue == 'R'/*Buy to cover*/)
                    this.Side = 'B';
                else
                    throw new PtFixException($"tag54 (Side) has UNSUPPORTED_VALUE of '{_cvalue}'");
                #endregion

                if (message.Contains(Tags.Account, instance)) this.Account = message[Tags.Account, instance].AsString;
                if (message.Contains(CustomTags.OrderOrigination, instance)) this.OrderOrigination = message[CustomTags.OrderOrigination, instance].AsChar;
                TradingCapacity = ParsingHelpers.GetOrderCapacityAsTradingCapacity(message, logger);


                m_parties.ParseParties(message, logger, instance);


                if (message.Contains(Tags.PositionEffect, instance)) this.PositionEffect = message[Tags.PositionEffect, instance].AsChar;

                ParseOrderAttributes(message, logger, instance);
            }



            /// <summary>
            /// The OrderAttributeGrp component provides additional attributes about the order.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="logger"></param>
            void ParseOrderAttributes(FIXMessage message, Logger logger, int instance)
            {
                this.NoOrderAttributes = 0;
                this.Attributes.Clear();

                if (message.Contains(CustomTags.NoOrderAttributes, instance))
                {
                    this.NoOrderAttributes = message[CustomTags.NoOrderAttributes, instance].AsInt;

                    if (this.NoOrderAttributes > 0)
                    {
                        for (int idx = 0; idx < this.NoOrderAttributes; idx++)
                        {
                            var attr = new OrderAttributeComponent();
                            attr.OrderAttributeType = message[CustomTags.OrderAttributeType, idx].AsInt;//Required if NoOrderAttributes(2593) > 0.
                            attr.OrderAttributeValue = message[CustomTags.OrderAttributeValue, idx].AsChar;//Required if NoOrderAttributes(2593) > 0.

                            this.Attributes.Add(attr);
                        }
                    }
                }
            }



            /// <summary>
            /// Side (Tag = 54, Type: char)
            /// </summary>
            public char Side { get; private set; }

            /// <summary>
            /// Account (Tag = 1, Type: String)
            /// REQUIRED
            /// </summary>
            public string Account { get; private set; }

            /// <summary>
            /// 1724
            /// </summary>
            public char OrderOrigination { get; private set; }

            /// <summary>
            ///OrderCapacity (Tag = 528, Type: char)
            /// </summary>
            public char TradingCapacity { get; private set; }



            /// <summary>
            /// NoPartyIDs (Tag = 453, Type: NumInGroup)
            /// </summary>
            public int NoPartyIDs => m_parties.NoPartyIDs;

            #region Parties
            /// <summary>
            /// 1 Executing Firm
            /// </summary>
            public string ExecutingFirmID => m_parties.ExecutingFirm.PartyID;

            /// <summary>
            /// 3 Client ID (MIFID II: Client identification code)
            /// </summary>
            public decimal? ClientID => m_parties.ClientID;
            /// <summary>
            /// 3 Client ID (MIFID II: Client identification code)
            /// </summary>
            public char ClientIDQualifier => m_parties.ClientIDQualifier;

            /// <summary>
            /// 122 Investment Decision Maker (MIFID II: Investment decision within firm)
            /// </summary>
            public decimal? InvestmentDecisionID => m_parties.InvestmentDecisionID;
            /// <summary>
            /// 
            /// </summary>
            public char InvestmentDecisionIDQualifier => m_parties.InvestmentDecisionIDQualifier;

            /// <summary>
            /// 4 Clearing Firm
            /// </summary>
            public string ClearingFirmID => m_parties.ClearingFirm.PartyID;

            /// <summary>
            /// 12 Executing trader (MIFID II: Execution within firm)
            /// </summary>
            public decimal? ExecutionWithinFirmID => m_parties.ExecutionWithinFirmID;
            /// <summary>
            /// 12 Executing trader (MIFID II: Execution within firm)
            /// </summary>
            public char ExecutionWithinFirmIDQualifier => m_parties.ExecutionWithinFirmIDQualifier;


            /// <summary>
            /// 26 Correspondent broker (MIFID II: Non-executing broker)
            /// </summary>
            public decimal? NonExecutingBrokerID => m_parties.NonExecutingBrokerID;

            /// <summary>
            /// 36 Entering trader (Trader ID)
            /// </summary>
            public string EnteringTraderID => m_parties.EnteringTrader.PartyID;
            #endregion


            /// <summary>
            /// PositionEffect (Tag = 77, Type: char)
            /// </summary>
            public char PositionEffect;

            /// <summary>
            /// 
            /// </summary>
            public int NoOrderAttributes;
            /// <summary>
            /// 
            /// </summary>
            public IList<OrderAttributeComponent> Attributes { get; } = new List<OrderAttributeComponent>();

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (this.Side == /*Buy*/'B')
                    return $"[Buy, Firm={this.ExecutingFirmID}, Trader={this.EnteringTraderID}]";
                else
                    return $"[Sell, Firm={this.ExecutingFirmID}, Trader={this.EnteringTraderID}]";
            }
        }



        void EmptyValues()
        {
            this.TradeReportID = default;
            this.TrdMatchID = default;
            this.TradeReportRefID = default;

            this.TradeReportTransType = TradeReportTransTypeEnum.Unknown;
            this.TradeReportType = TradeReportTypeEnum.Unknown;


            this.LastQty = default;
            this.LastPx = default;
            this.SecurityID = default;
            this.SecurityIDSource = default;
            this.SecurityExchange = default;

            this.MatchStatus = MatchStatusEnum.Unknown;
            this.NoSides = default;

            this.OurSide.EmptyValues();
            this.OtherSide.EmptyValues();

            this.BoardID = 'M';
            this.Timestamp = string.Empty;
            this.ATHEXTradeType = default;
            this.CurrentCreditValue = default;
            this.PreviouslyReported = default;
            this.DirectElectronicAccess = default;
        }



        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου TradeCaptureReportMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            EmptyValues();


            this.TradeReportID = message[Tags.TradeReportID].AsString.Trim();
            if (message.Contains(Tags.TrdMatchID)) this.TrdMatchID = message[Tags.TrdMatchID].AsString.Trim();
            if (message.Contains(Tags.TradeReportRefID)) this.TradeReportRefID = message[Tags.TradeReportRefID].AsString.Trim();

            #region TradeReportTransType (Tag = 487, Type: int)
            int _ivalue = message[Tags.TradeReportTransType].AsInt;
            if (_ivalue == 0)
                this.TradeReportTransType = TradeReportTransTypeEnum.New;
            else if (_ivalue == 1)
                this.TradeReportTransType = TradeReportTransTypeEnum.Cancel;
            else if (_ivalue == 2)
                this.TradeReportTransType = TradeReportTransTypeEnum.Replace;
            else
                throw new PtFixException($"tag487 (TradeReportTransType) has UNSUPPORTED_VALUE of '{_ivalue}'");
            #endregion

            #region TradeReportType (Tag = 856, Type: int)
            _ivalue = message[Tags.TradeReportType].AsInt;
            if (_ivalue == 0)
                this.TradeReportType = TradeReportTypeEnum.Submit;
            else if (_ivalue == 1)
                this.TradeReportType = TradeReportTypeEnum.Alleged;
            else if (_ivalue == 2)
                this.TradeReportType = TradeReportTypeEnum.Accept;
            else if (_ivalue == 3)
                this.TradeReportType = TradeReportTypeEnum.Decline;
            else if (_ivalue == 5)
                this.TradeReportType = TradeReportTypeEnum.Expired;
            else if (_ivalue == 6)
                this.TradeReportType = TradeReportTypeEnum.TradeReportCancel;
            else
                throw new PtFixException($"tag856 (TradeReportType) has UNSUPPORTED_VALUE of '{_ivalue}'");
            #endregion


            if (message.Contains(Tags.LastQty)) this.LastQty = message[Tags.LastQty].AsFloat;
            if (message.Contains(Tags.LastPx)) this.LastPx = message[Tags.LastPx].AsFloat;

            ParseSecurity(message, logger);

            #region MatchStatus (Tag = 573, Type: char)
            if (message.Contains(Tags.MatchStatus))
            {
                char _cvalue = message[Tags.MatchStatus].AsChar;
                if (_cvalue == '0')
                    this.MatchStatus = MatchStatusEnum.Matched;
                else if (_cvalue == '1')
                    this.MatchStatus = MatchStatusEnum.UnMatched;
                else
                    throw new PtFixException($"tag573 (MatchStatus) has UNSUPPORTED_VALUE of '{_cvalue}'");
            }
            #endregion


            this.NoSides = message[Tags.NoSides].AsInt;
            if (this.NoSides == /*one side*/1 || this.NoSides == /*both sides*/2)
            {
                this.OurSide.ParseFixMessage(message, logger, 0);

                if (this.NoSides == /*both sides*/2)
                {
                    this.OtherSide.ParseFixMessage(message, logger, 1);
                }
            }
            else
            {
                throw new PtFixException($"tag552 (NoSides) has UNSUPPORTED_VALUE of '{this.NoSides}'");
            }

            DirectElectronicAccess = ParsingHelpers.GetOrderOrigination(message, logger);


            if (message.Contains(CustomTags.BoardID)) this.BoardID = message[CustomTags.BoardID].AsChar;
            this.ATHEXTradeType = message[CustomTags.ATHEXTradeType].AsString;
            if (message.Contains(CustomTags.CurrentCreditValue)) this.CurrentCreditValue = message[CustomTags.CurrentCreditValue].AsFloat;
            this.PreviouslyReported = message[Tags.PreviouslyReported].AsChar;

            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            return this;
        }


        void ParseSecurity(FIXMessage message, Logger logger)
        {
            SecurityID = message[Tags.SecurityID].AsString;                       //REQUIRED
            SecurityIDSource = message[Tags.SecurityIDSource].AsChar;             //REQUIRED
            SecurityExchange = message[Tags.SecurityExchange].AsString;           //REQUIRED
        }




        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Trade_Capture_Report;

        /// <summary>
        /// TradeReportID (Tag = 571, Type: String)
        /// </summary>
        internal string TradeReportID;
        /// <summary>
        /// TrdMatchID (Tag = 880, Type: String)
        /// </summary>
        internal string TrdMatchID;
        /// <summary>
        /// TradeReportRefID (Tag = 572, Type: String)
        /// Reference identifier used with CANCEL and REPLACE transaction types.
        /// </summary>
        internal string TradeReportRefID;
        /// <summary>
        /// TradeReportTransType (Tag = 487, Type: int)
        /// Identifies Trade Capture Report (AE) message transaction type 
        /// </summary>
        internal TradeReportTransTypeEnum TradeReportTransType;
        /// <summary>
        /// TradeReportType (Tag = 856, Type: int)
        /// 
        /// Type of Trade Report:
        ///     '0' 	Submit
        ///     '1' 	Alleged
        ///     '2' 	Accept
        ///     '3' 	Decline
        ///     '4' 	Addendum
        ///     '5' 	No/Was
        ///     '6' 	Trade Report Cancel
        /// </summary>
        internal TradeReportTypeEnum TradeReportType;
        /// <summary>
        ///LastQty (Tag = 32, Type: Qty)
        ///Quantity (e.g. shares) bought/sold on this (last) fill.
        /// </summary>
        internal double LastQty;

        /// <summary>
        ///LastPx (Tag = 31, Type: Price)
        ///Price of this (last) fill.
        /// </summary>
        internal double LastPx;

        /// <summary>
        /// SecurityID (Tag = 48, Type: String)
        /// Not supplied if ExecType is /*Rejected*/8
        /// </summary>
        internal string SecurityID;
        /// <summary>
        /// SecurityIDSource (Tag = 22, Type: String)
        /// Not supplied if ExecType is /*Rejected*/8
        /// </summary>
        internal char SecurityIDSource;
        /// <summary>
        /// SecurityExchange (Tag = 207, Type: Exchange)
        /// Not supplied if ExecType is /*Rejected*/8
        /// </summary>
        internal string SecurityExchange;
        /// <summary>
        /// MatchStatus (Tag = 573, Type: char)
        /// he status of this trade with respect to matching or comparison.
        /// </summary>
        internal MatchStatusEnum MatchStatus;
        /// <summary>
        /// NoSides (Tag = 552, Type: NumInGroup)
        /// Number of Sides (54) repeating group instances. 
        /// </summary>
        internal int NoSides;


        internal TradeSide OurSide { get; } = new TradeSide();
        internal TradeSide OtherSide { get; } = new TradeSide();


        /// <summary>
        /// 5506 Char
        /// </summary>
        internal char BoardID;

        /// <summary>
        /// TransactTime (Tag = 60, Type: UTCTimestamp)
        /// </summary>
        public string Timestamp { get; private set; }

        public string TradeDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Timestamp) == false && this.Timestamp.Length >= 16)
                {
                    return Timestamp.Substring(0, 8);
                }

                return string.Empty;
            }
        }
        public string TradeTime
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Timestamp) == false && this.Timestamp.Length >= 16)
                {
                    return Timestamp.Substring(8, 8);
                }

                return string.Empty;
            }
        }
        public string OrderNumber => this.TradeReportID;


        /// <summary>
        /// 
        /// </summary>
        internal string ATHEXTradeType;
        /// <summary>
        /// 5545
        /// </summary>
        internal double CurrentCreditValue;

        /// <summary>
        /// PreviouslyReported (Tag = 570, Type: Boolean)
        /// Indicates if the trade capture report was previously reported to the counterparty
        /// </summary>
        internal char PreviouslyReported;

        internal char DirectElectronicAccess;


        public override string ToString()
        {
            if (this.NoSides == 2)
                return $"{this.MatchStatus}|{this.TradeReportType}|{this.TradeReportTransType}, {this.SecurityID} {this.LastQty}@{this.LastPx}, side1:{this.OurSide}, side2: {this.OtherSide}, ReportID={this.TradeReportID}";
            else
                return $"{this.MatchStatus}|{this.TradeReportType}|{this.TradeReportTransType}, {this.SecurityID} {this.LastQty}@{this.LastPx}, side1:{this.OurSide}, ReportID={this.TradeReportID}";
        }
    }
}
