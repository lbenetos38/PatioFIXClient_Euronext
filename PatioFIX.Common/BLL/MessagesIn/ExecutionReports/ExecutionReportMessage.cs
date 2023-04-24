using PatioFIX.Common.BLL.MessagesIn;
using PatioFIX.Common.FixSupport;
using System;
using System.Collections.Generic;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionReportMessage
    {
        PartiesContainer m_parties = new PartiesContainer("ExecutionReportMessage", Globals.FixClient.ValidateDuplicatePartyRole, Globals.FixClient.ValidateRepeatingGroupEntryCount);

        /// <summary>
        /// 
        /// </summary>
        void EmptyValues()
        {
            this.OrderID = String.Empty;
            this.OrigClOrdID = String.Empty;
            this.ClOrdID = String.Empty;
            this.ExecID = default;
            this.ExecRefID = default;
            this.ExecType = default;

            m_parties.EmptyValues();

            this.Account = default;
            this.Currency = default;
            this.LastQty = default;
            this.LastPx = default;
            this.OrderCapacity = default;
            this.SecurityID = default;
            this.SecurityIDSource = default;
            this.SecurityExchange = default;

            this.OrdType = default;
            this.TimeInForce = default;
            this.ExpireDate = String.Empty;
            this.OrdStatus = default;
            this.ExecRestatementReason = default;
            this.Text = string.Empty;
            this.Price = default;
            this.StopPx = default;
            this.OrderQty = default;
            this.PositionEffect = default;
            this.SettlType = default;
            this.MaxShow = default;
            this.TradingSessionID = default;
            this.LeavesQty = default;
            this.CumQty = default;
            this.AvgPx = default;
            this.Side = default;
            this.LastLiquidityInd = default;
            this.TransactTime = default;
            this.GrossTradeAmt = default;

            this.MktBidPx = default;
            this.MktOfferPx = default;
            this.BidSize = default;
            this.OfferSize = default;

            this.OrigSource = default;
            this.BoardID = 'M';
            this.OrderRelFlag = default;
            this.OrderRefID = string.Empty;
            this.GOIFlag = default;
            this.StopSymbol = default;
            this.SecurityStatus = default;
            this.StopSymbolType = default;
            this.ΑΤΗΕΧTradeType = default;
            this.RejectReasonCode = default;
            this.CurrentCreditValue = default;
            this.MBListID = default;
            this.CancelReasonCode = default;
            this.OrderOrigination = default;

            this.NoOrderAttributes = 0;
            this.Attributes.Clear();

            this.NoTrdRegPublications = default;
            this.TradePublicationReasons = default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        public void ParseMessage(FIXMessage message, Logger logger)
        {
            EmptyValues();

            /*
             * Υπαρχουν περιπτωσεις που το OrderID ειναι ενα κενο string διοτι ο FIX Server δεν κανει
             * δεκτη την νεα εντολη (Rejection), και αφου δεν εχει δημιουργηθει στο συστημα του ATHEX,
             * μας στελνει κενο string.
             * Στις αρχικες δοκιμες εστελνε σε αυτες τις περιπτωσεις αντι για κενο string ενα 'NONE'
             */
            this.OrderID = message[Tags.OrderID].AsString.Trim();
            if (message.Contains(Tags.OrigClOrdID)) this.OrigClOrdID = message[Tags.OrigClOrdID].AsString;
            if (message.Contains(Tags.ClOrdID)) this.ClOrdID = message[Tags.ClOrdID].AsString;
            this.ExecID = message[Tags.ExecID].AsString;
            if (message.Contains(Tags.ExecRefID)) this.ExecRefID = message[Tags.ExecRefID].AsString;
            this.ExecType = message[Tags.ExecType].AsChar;

            m_parties.ParseParties(message, logger);

            this.Account = message[Tags.Account].AsString;
            if (message.Contains(Tags.Currency)) this.Currency = message[Tags.Currency].AsString;
            if (message.Contains(Tags.LastQty)) this.LastQty = message[Tags.LastQty].AsFloat;
            if (message.Contains(Tags.LastPx)) this.LastPx = message[Tags.LastPx].AsFloat;
            if (message.Contains(Tags.OrderCapacity)) this.OrderCapacity = message[Tags.OrderCapacity].AsChar;

            if (this.ExecType != /*Rejected*/'8')
            {
                /*
                 * Not supplied if ExecType = 8
                 * Τα μηνυματα 35=8^150=8 γινονται 'Rejection ("TR")'
                 */
                ParseSecurity(message, logger);
            }

            if (message.Contains(Tags.OrdType)) this.OrdType = message[Tags.OrdType].AsChar;
            if (message.Contains(Tags.TimeInForce)) this.TimeInForce = message[Tags.TimeInForce].AsChar;
            if (message.Contains(Tags.ExpireDate)) this.ExpireDate = message[Tags.ExpireDate].AsString;
            if (message.Contains(Tags.OrdStatus)) this.OrdStatus = message[Tags.OrdStatus].AsChar;
            if (message.Contains(Tags.ExecRestatementReason)) this.ExecRestatementReason = message[Tags.ExecRestatementReason].AsInt;
            if (message.Contains(Tags.Text)) this.Text = message[Tags.Text].AsString;
            if (message.Contains(Tags.Price)) this.Price = message[Tags.Price].AsFloat;
            if (message.Contains(Tags.StopPx)) this.StopPx = message[Tags.StopPx].AsFloat;
            if (message.Contains(Tags.OrderQty)) this.OrderQty = message[Tags.OrderQty].AsFloat;
            if (message.Contains(Tags.PositionEffect)) this.PositionEffect = message[Tags.PositionEffect].AsChar;
            if (message.Contains(Tags.SettlType)) this.SettlType = message[Tags.SettlType].AsChar;
            if (message.Contains(Tags.MaxShow)) this.MaxShow = message[Tags.MaxShow].AsFloat;
            if (message.Contains(Tags.TradingSessionID)) this.TradingSessionID = message[Tags.TradingSessionID].AsChar;
            if (message.Contains(Tags.LeavesQty)) this.LeavesQty = message[Tags.LeavesQty].AsFloat;
            if (message.Contains(Tags.CumQty)) this.CumQty = message[Tags.CumQty].AsFloat;
            if (message.Contains(Tags.AvgPx)) this.AvgPx = message[Tags.AvgPx].AsFloat;
            if (message.Contains(Tags.Side)) this.Side = message[Tags.Side].AsChar;
            if (message.Contains(Tags.LastLiquidityInd)) this.LastLiquidityInd = message[Tags.LastLiquidityInd].AsInt;
            this.TransactTime = message[Tags.TransactTime].AsString;
            if (message.Contains(Tags.GrossTradeAmt)) this.GrossTradeAmt = message[Tags.GrossTradeAmt].AsFloat;


            if (message.Contains(Tags.MktBidPx)) this.MktBidPx = message[Tags.MktBidPx].AsFloat;
            if (message.Contains(Tags.MktOfferPx)) this.MktOfferPx = message[Tags.MktOfferPx].AsFloat;
            if (message.Contains(Tags.BidSize)) this.BidSize = message[Tags.BidSize].AsFloat;
            if (message.Contains(Tags.OfferSize)) this.OfferSize = message[Tags.OfferSize].AsFloat;

            if (message.Contains(CustomTags.OrigSource)) this.OrigSource = message[CustomTags.OrigSource].AsChar;
            if (message.Contains(CustomTags.BoardID)) this.BoardID = message[CustomTags.BoardID].AsChar;
            if (message.Contains(CustomTags.OrderRelFlag)) this.OrderRelFlag = message[CustomTags.OrderRelFlag].AsInt;
            if (message.Contains(CustomTags.OrderRefID)) this.OrderRefID = message[CustomTags.OrderRefID].AsString;
            if (message.Contains(CustomTags.GOIFlag)) this.GOIFlag = message[CustomTags.GOIFlag].AsChar;
            if (message.Contains(CustomTags.StopSymbol)) this.StopSymbol = message[CustomTags.StopSymbol].AsString;
            if (message.Contains(CustomTags.SecurityStatus)) this.SecurityStatus = message[CustomTags.SecurityStatus].AsChar;
            if (message.Contains(CustomTags.StopSymbolType)) this.StopSymbolType = message[CustomTags.StopSymbolType].AsString;
            if (message.Contains(CustomTags.ΑΤΗΕΧTradeType)) this.ΑΤΗΕΧTradeType = message[CustomTags.ΑΤΗΕΧTradeType].AsString;
            if (message.Contains(CustomTags.RejectReasonCode)) this.RejectReasonCode = message[CustomTags.RejectReasonCode].AsString;
            if (message.Contains(CustomTags.CurrentCreditValue)) this.CurrentCreditValue = message[CustomTags.CurrentCreditValue].AsFloat;
            if (message.Contains(CustomTags.MBListID)) this.MBListID = message[CustomTags.MBListID].AsString;
            if (message.Contains(CustomTags.CancelReasonCode)) this.CancelReasonCode = message[CustomTags.CancelReasonCode].AsChar;
            if (message.Contains(CustomTags.OrderOrigination)) this.OrderOrigination = message[CustomTags.OrderOrigination].AsChar;

            ParseOrderAttributes(message, logger);

            ParserTradePublicationReasons(message, logger);

        }




        void ParseSecurity(FIXMessage message, Logger logger)
        {
            SecurityID = message[Tags.SecurityID].AsString;                       //REQUIRED
            SecurityIDSource = message[Tags.SecurityIDSource].AsChar;             //REQUIRED
            SecurityExchange = message[Tags.SecurityExchange].AsString;           //REQUIRED
        }


        /// <summary>
        /// The OrderAttributeGrp component provides additional attributes about the order.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        void ParseOrderAttributes(FIXMessage message, Logger logger)
        {
            this.NoOrderAttributes = 0;
            this.Attributes.Clear();

            if (message.Contains(CustomTags.NoOrderAttributes))
            {
                this.NoOrderAttributes = message[CustomTags.NoOrderAttributes].AsInt;

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

        void ParserTradePublicationReasons(FIXMessage message, Logger logger)
        {
            if (message.Contains(CustomTags.NoTrdRegPublications))
            {
                this.NoTrdRegPublications = message[CustomTags.NoTrdRegPublications].AsInt;

                if (this.NoTrdRegPublications > 0)
                {
                    this.TradePublicationReasons = new List<TradePublicationReasonComponent>();
                    for (int idx = 0; idx < this.NoTrdRegPublications; idx++)
                    {
                        var item = new TradePublicationReasonComponent();
                        item.TrdRegPublicationType = message[CustomTags.TrdRegPublicationType, idx].AsString;
                        item.TrdRegPublicationReason = message[CustomTags.TrdRegPublicationReason, idx].AsString;

                        this.TradePublicationReasons.Add(item);
                    }
                }
            }
        }


        /// <summary>
        /// Αυτο ειναι πεδίο του ATHEX
        /// Προκύπτει απο το OrderID (Tag = 37, Type: String)
        /// REQUIRED
        /// </summary>
        public string OrderNumber
        {
            get
            {
                if (this.OrderID == "NONE" || String.IsNullOrEmpty(this.OrderID))
                {
                    return "NONE";
                }
                else
                {
                    //To OrderID περιέχει το OrderNumber + OrderDate
                    return this.OrderID.Substring(0, 8);
                }
            }
        }
        /// <summary>
        /// Αυτο ειναι πεδίο του ATHEX
        /// Προκύπτει απο το OrderID (Tag = 37, Type: String)
        /// REQUIRED
        /// </summary>
        public string OrderDate
        {
            get
            {
                if (this.OrderID == "NONE" || String.IsNullOrEmpty(this.OrderID))
                {
                    return "NONE";
                }
                else
                {
                    //To OrderID περιέχει το OrderNumber + OrderDate
                    return this.OrderID.Substring(8, 8);
                }
            }
        }



        /// <summary>
        /// OrderID (Tag = 37, Type: String)
        /// REQUIRED
        /// </summary>
        internal string OrderID;
        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// </summary>
        internal string OrigClOrdID;
        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// </summary>
        internal string ClOrdID;
        /// <summary>
        /// (Tag = 17, Type: String)
        /// Unique identifier of Execution Report (8) message as assigned by sell-side (ATHEX) (will be 0 (zero) for ExecType (150) =I (Order Status)). 
        /// REQUIRED
        /// </summary>
        internal string ExecID;
        /// <summary>
        /// ExecRefID (Tag = 19, Type: String)
        /// Reference identifier used with Trade Cancel and Trade Correct execution types.
        /// </summary>
        protected string ExecRefID;
        /// <summary>
        /// ExecType (Tag = 150, Type: char)
        /// </summary>
        internal char ExecType;

        /// <summary>
        /// 
        /// </summary>
        internal PartiesContainer Parties => m_parties;


        /// <summary>
        /// Account (Tag = 1, Type: String)
        /// REQUIRED
        /// </summary>
        internal string Account;

        /// <summary>
        /// Currency (Tag = 15, Type: Currency)
        /// </summary>
        protected string Currency;

        /// <summary>
        ///LastQty (Tag = 32, Type: Qty)
        /// </summary>
        internal double LastQty;

        /// <summary>
        ///LastPx (Tag = 31, Type: Price)
        /// </summary>
        protected double LastPx;

        /// <summary>
        ///OrderCapacity (Tag = 528, Type: char)
        /// </summary>
        protected char OrderCapacity;

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
        /// OrdType (Tag = 40, Type: char)
        /// </summary>
        internal char OrdType;

        /// <summary>
        /// TimeInForce (Tag = 59, Type: char)
        /// Not supplied if ExecType is /*Trade (partial fill or fill)*/F, /*Canceled*/4, /*Replace*/5, /*Suspended*/9
        /// </summary>
        internal char TimeInForce;
        /// <summary>
        /// ExpireDate (Tag = 432, Type: LocalMktDate)
        /// </summary>
        internal string ExpireDate;

        /// <summary>
        /// OrdStatus (Tag = 39, Type: char)
        /// </summary>
        internal char OrdStatus;

        /// <summary>
        /// ExecRestatementReason (Tag = 378, Type: int)
        /// </summary>
        protected int ExecRestatementReason;

        /// <summary>
        /// Text (Tag = 58, Type: String)
        /// </summary>
        internal string Text;

        /// <summary>
        /// Price (Tag = 44, Type: Price)
        /// </summary>
        internal double Price;
        /// <summary>
        /// StopPx (Tag = 99, Type: Price)
        /// </summary>
        internal double StopPx;
        /// <summary>
        /// OrderQty (Tag = 38, Type: Qty)
        /// </summary>
        internal double OrderQty;

        /// <summary>
        /// PositionEffect (Tag = 77, Type: char)
        /// </summary>
        internal char PositionEffect;
        /// <summary>
        /// SettlType (Tag = 63, Type: char)
        /// </summary>
        internal char SettlType;

        /// <summary>
        /// MaxShow (Tag = 210, Type: Qty)
        /// </summary>
        internal double MaxShow;

        /// <summary>
        /// TradingSessionID (Tag = 336, Type: String)
        /// </summary>
        internal char TradingSessionID;

        /// <summary>
        /// LeavesQty (Tag = 151, Type: Qty)
        /// </summary>
        internal double LeavesQty;
        /// <summary>
        /// CumQty (Tag = 14, Type: Qty)
        /// </summary>
        internal double CumQty;
        /// <summary>
        /// AvgPx (Tag = 6, Type: Price)
        /// </summary>
        internal double AvgPx;
        /// <summary>
        /// Side (Tag = 54, Type: char)
        /// </summary>
        internal char Side;
        /// <summary>
        /// LastLiquidityInd (Tag = 851, Type: int)
        /// </summary>
        protected int LastLiquidityInd;
        /// <summary>
        /// TransactTime (Tag = 60, Type: UTCTimestamp)
        /// </summary>
        protected string TransactTime;
        /// <summary>
        /// GrossTradeAmt (Tag = 381, Type: Amt)
        /// Total amount traded (e.g. CumQty (14) * AvgPx (6) ) expressed in units of currency. 
        /// </summary>
        internal double GrossTradeAmt;




        /// <summary>
        /// MktBidPx (Tag = 645, Type: Price)
        /// </summary>
        internal double MktBidPx;
        /// <summary>
        /// MktOfferPx (Tag = 646, Type: Price)
        /// </summary>
        internal double MktOfferPx;
        /// <summary>
        /// BidSize (Tag = 134, Type: Qty)
        /// </summary>
        internal double BidSize;
        /// <summary>
        /// OfferSize (Tag = 135, Type: Qty)
        /// </summary>
        internal double OfferSize;


        /// <summary>
        /// 5501, 
        /// </summary>
        internal char OrigSource;
        /// <summary>
        /// 5506 Char
        /// </summary>
        internal char BoardID;
        /// <summary>
        /// 5509, int
        /// </summary>
        protected int OrderRelFlag;
        /// <summary>
        /// 5510, int
        /// </summary>
        internal string OrderRefID;
        /// <summary>
        /// 5512, Boolean, 'Y','N'
        /// </summary>
        protected char GOIFlag;
        /// <summary>
        /// 5521
        /// </summary>
        protected string StopSymbol;
        /// <summary>
        /// 5522, char
        /// </summary>
        internal char SecurityStatus;
        /// <summary>
        /// 5527, char
        /// </summary>
        protected string StopSymbolType;
        /// <summary>
        /// 5529
        /// </summary>
        internal string ΑΤΗΕΧTradeType;
        /// <summary>
        /// 5532
        /// </summary>
        internal string RejectReasonCode;
        /// <summary>
        /// 5545
        /// </summary>
        internal double CurrentCreditValue;
        /// <summary>
        /// 5561
        /// </summary>
        protected string MBListID;
        /// <summary>
        /// 5508
        /// </summary>
        internal char CancelReasonCode;
        /// <summary>
        /// 1724
        /// </summary>
        protected char OrderOrigination;

        internal int NoOrderAttributes;
        internal IList<OrderAttributeComponent> Attributes = new List<OrderAttributeComponent>();

        protected int NoTrdRegPublications;
        IList<TradePublicationReasonComponent> TradePublicationReasons;
    }
}
