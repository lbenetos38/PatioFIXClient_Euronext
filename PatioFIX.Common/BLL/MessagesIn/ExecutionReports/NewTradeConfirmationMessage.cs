using PatioFIX.Common.FixSupport;
using System.Globalization;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// New Trade Confirmation ("TF")
    /// </summary>
    class NewTradeConfirmationMessage : IODLMessage, IFixParserToODL
    {
        ExecutionReportMessage m_executionReportMessage = new ExecutionReportMessage();


        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου NewTradeConfirmationMessage Instance
        /// Εδω εχουμε ενα Execution Report με ExecType ισο με 'F' 	Trade (partial fill or fill)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            this.ContraMemberID = "0000";
            this.ContraTraderID = string.Empty;
            this.Currency = string.Empty;
            this.GOIFlag = 'N';
            this.ListID = string.Empty;
            this.TradeSource = ' ';
            this.WaiverIndicator = "0000";
            this.ShortSellFlag = 'N';
            this.CommodityHedgingFlag = 'N';
            #endregion


            #region βαζουμε το embedded  ExecutionReportMessage να διαβασει  το FIX Message
            m_executionReportMessage.ParseMessage(message, logger);
            #endregion



            if (m_executionReportMessage.ExecType == 'F')
                this.TradeStatus = "  ";        //Normal Completed Trade
            else if (m_executionReportMessage.ExecType == 'G')
                this.TradeStatus = "U ";        //Changed trade (XNet)
            else if (m_executionReportMessage.ExecType == 'H')
                this.TradeStatus = "X ";        //Cancelled trade
            else
            {
                throw new PtFixException($"tag150 (ExecType) has UNSUPPORTED_VALUE of '{m_executionReportMessage.ExecType}'");
            }



            this.TradeNumber = m_executionReportMessage.ExecID.Substring(0, 6);
            /*
             * 
             */
            OrderRelFlag = ParsingHelpers.GetOrderRelFlag(message, logger);


            var _ordStatus = message[Tags.OrdStatus].AsChar;
            this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(_ordStatus);
            if (
                _ordStatus != OrdStatus.PartiallyFilled &&
                _ordStatus != OrdStatus.Filled &&
                _ordStatus != OrdStatus.Inactive)
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"NewTradeConfirmationMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{_ordStatus}'");
            }


            LastLiquidityIndicator = ParsingHelpers.GetLastLiquidityInd(message, logger);
            DirectElectronicAccess = ParsingHelpers.GetOrderOrigination(message, logger);
            TradingCapacity = ParsingHelpers.GetOrderCapacityAsTradingCapacity(message, logger);

            if (m_executionReportMessage.NoOrderAttributes > 0)
            {
                foreach (var attr in m_executionReportMessage.Attributes)
                {
                    if (attr.OrderAttributeType == /*Liquidity provision activity order*/2)
                    {
                        if (attr.OrderAttributeValue == 'Y')
                            this.LiquidityProvision = '1';
                        else
                            this.LiquidityProvision = '0';
                    }
                    else if (attr.OrderAttributeType == /*Risk Reduction*/3)
                    {

                    }
                    else if (attr.OrderAttributeType == /*Algorithmic order*/4)
                    {
                        this.AlgoFlag = attr.OrderAttributeValue;
                    }
                }
            }


            #region ClientID, InvestmentDecisionID, ExecutionWithinFirmID, NonExecutingBrokerID Decimal Value initialization
            /*
             * Περιμενω οτι το ClientID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdClientID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.ClientIdentificationCode.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _clientID))
            {
                this.ClientID = _clientID;
            }
            else
            {
                this.ClientID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"NewTradeConfirmationMessage -> ClientIdentificationCode.PartyID '{m_executionReportMessage.ClientIdentificationCode.PartyID}' cannot be Parsed as decimal");
            }

            /*
             * Περιμενω οτι το InvestmentDecisionID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdInvestmentDecisionID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.InvestmentDecisionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _investmentDecisionID))
            {
                this.InvestmentDecisionID = _investmentDecisionID;
            }
            else
            {
                this.InvestmentDecisionID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"NewTradeConfirmationMessage -> InvestmentDecisionWithinFirm.PartyID '{m_executionReportMessage.InvestmentDecisionWithinFirm.PartyID}' cannot be Parsed as decimal");
            }

            /*
             * Περιμενω οτι το ExecutionWithinFirmID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdExecutionWithinFirmId] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.ExecutionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _executionWithinFirmID))
            {
                this.ExecutionWithinFirmID = _executionWithinFirmID;
            }
            else
            {
                this.ExecutionWithinFirmID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"NewTradeConfirmationMessage -> ExecutionWithinFirm.PartyID '{m_executionReportMessage.ExecutionWithinFirm.PartyID}' cannot be Parsed as decimal");
            }

            /*
             * Περιμενω οτι το NonExecutingBrokerID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdNonExecutingBrokerID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.NonExecutingBroker.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _nonExecutingBrokerID))
            {
                this.NonExecutingBrokerID = _nonExecutingBrokerID;
            }
            else
            {
                this.NonExecutingBrokerID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"NewTradeConfirmationMessage -> NonExecutingBroker.PartyID '{m_executionReportMessage.NonExecutingBroker.PartyID}' cannot be Parsed as decimal");
            }
            #endregion


            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.New_Trade_Confirmation;

        public string MemberID => m_executionReportMessage.ExecutingFirm.PartyID;
        public string TraderID => m_executionReportMessage.EnteringTrader.PartyID;
        public string VenueID => m_executionReportMessage.SecurityExchange;
        public char BoardID
        {
            get
            {
                if (m_executionReportMessage.BoardID != default)
                    return m_executionReportMessage.BoardID;
                else
                    return 'M';
            }
        }
        /// <summary>
        /// Unique identifier for Order as assigned by Equities
        /// </summary>
        public string ClientOrderID => m_executionReportMessage.ClOrdID;
        public string CSDAccountID => m_executionReportMessage.Account;
        /// <summary>
        /// 
        /// </summary>
        public char OrderRelFlag { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderRefID => m_executionReportMessage.OrderRefID;
        public char GOIFlag { get; private set; }
        public char ShortSellFlag { get; private set; }
        public string ClearingMemberID => m_executionReportMessage.ClearingFirm.PartyID;
        public char PositionEffect => m_executionReportMessage.PositionEffect;
        public char SettlType => m_executionReportMessage.SettlType;
        /// <summary>
        /// A single character alphanumeric type indicating the source of the Order. Possible values :
        /// ‘C’ CTCI –ODL
        ///	‘M’ ORAMA-ETW
        /// ‘R’ EMRW(ATHEX Supervision Application).
        /// ' ' OASIS
        /// </summary>
        public char OrderSource => m_executionReportMessage.OrigSource;

        #region ExchangeOrderID
        /// <summary>
        /// OrderID (Tag = 37, Type: String)
        /// Unique identifier for Order as assigned by sell-side (broker, exchange, ECN). 
        /// Uniqueness must be guaranteed within a single trading day. 
        /// Firms which accept multi-day orders should consider embedding a date within the OrderID (37) field to assure uniqueness across days. 
        /// </summary>
        public string ExchangeOrderID => m_executionReportMessage.OrderID;
        public string OrderNumber => m_executionReportMessage.OrderNumber;
        public string OrderDate => m_executionReportMessage.OrderDate;
        #endregion

        public string ExpirationDate => m_executionReportMessage.ExpireDate;
        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        ///  Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// </summary>
        public char FixOrderStatus => m_executionReportMessage.OrdStatus;
        public double LeavesQuantity => m_executionReportMessage.LeavesQty;
        public double AveragePrice => m_executionReportMessage.AvgPx;
        public string SecurityID => m_executionReportMessage.SecurityID;
        public char SecurityIDSource => m_executionReportMessage.SecurityIDSource;
        public string Currency { get; private set; }
        public char Side
        {
            get
            {
                if (m_executionReportMessage.Side == '1'/*Buy*/)
                    return 'B';
                else if (m_executionReportMessage.Side == '2'/*Sell*/)
                    return 'S';
                else if (m_executionReportMessage.Side == '5'/*Sell short*/)
                    return 'S';
                else if (m_executionReportMessage.Side == 'R'/*Buy to cover*/)
                    return 'B';
                else
                    throw new PtFixException($"tag54 (Side) has UNSUPPORTED_VALUE of '{m_executionReportMessage.Side}'");
            }
        }
        public double Volume => m_executionReportMessage.LastQty;
        public double Price => m_executionReportMessage.Price;
        public string ContraMemberID { get; private set; }
        public string ContraTraderID { get; private set; }
        public string TradeNumber { get; private set; }
        public double CurrentCreditValue => m_executionReportMessage.CurrentCreditValue;
        public string ListID { get; private set; }
        public char TradeSource { get; private set; }
        public char PhaseID => m_executionReportMessage.TradingSessionID;
        public char SecurityStatus => m_executionReportMessage.SecurityStatus;
        public string TradeType => m_executionReportMessage.ΑΤΗΕΧTradeType;
        public string TradeStatus { get; private set; }
        public char LastLiquidityIndicator { get; private set; }
        /// <summary>
        /// Total amount traded (e.g. CumQty (14) * AvgPx (6) ) expressed in units of currency. 
        /// </summary>
        public double NotionalAmmount => m_executionReportMessage.GrossTradeAmt;
        public char DirectElectronicAccess { get; private set; }

        public decimal? ClientID { get; private set; }
        public char ClientIDQualifier
        {
            get
            {
                var qualifier = m_executionReportMessage.ClientIdentificationCode.PartyRoleQualifier;

                if (qualifier == /*Firm or legal entity*/23)
                    return 'L';//76
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'N';//78
                //return 'X';//88
            }
        }
        public decimal? InvestmentDecisionID { get; private set; }
        public char InvestmentDecisionIDQualifier
        {
            get
            {
                var qualifier = m_executionReportMessage.InvestmentDecisionWithinFirm.PartyRoleQualifier;

                if (qualifier == /*Algorithm*/22)
                    return 'A';//65
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'X';//78
            }
        }
        public decimal? ExecutionWithinFirmID { get; private set; }
        public char ExecutionWithinFirmIDQualifier
        {
            get
            {
                var qualifier = m_executionReportMessage.ExecutionWithinFirm.PartyRoleQualifier;

                if (qualifier == /*Algorithm*/22)
                    return 'A';//65
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'X';//78
            }
        }
        public decimal? NonExecutingBrokerID { get; private set; }

        public char TradingCapacity { get; private set; }
        public char LiquidityProvision { get; private set; }
        public string WaiverIndicator { get; private set; }
        public double BestBidPrice => m_executionReportMessage.MktBidPx;
        public double BestBidQuantity => m_executionReportMessage.BidSize;
        public double BestOfferPrice => m_executionReportMessage.MktOfferPx;
        public double BestOfferQuantity => m_executionReportMessage.OfferSize;
        public string Timestamp { get; private set; }
        public char AlgoFlag { get; private set; }
        public char CommodityHedgingFlag { get; private set; }
    }
}
