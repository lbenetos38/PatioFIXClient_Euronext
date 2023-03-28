using PatioFIX.Common.FixSupport;
using System.Globalization;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Order Entry Confirmation ("TB")
    /// </summary>
    class OrderEntryConfirmationMessage : IODLMessage, IFixParserToODL
    {
        ExecutionReportMessage m_executionReportMessage = new ExecutionReportMessage();


        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου OrderEntryConfirmationMessage Instance
        /// Εδω εχουμε ενα Execution Report με ExecType ισο με '0' 	(New)
        /// Αυτο το λαμβανουμε σαν επιβεβαιωση της αποστολης νεας εντολής (35=D)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            this.AutoDisclosedVolume = 0;
            this.Currency = string.Empty;
            this.GOIFlag = 'N';
            this.ListID = string.Empty;
            this.CommodityHedgingFlag = 'N';
            this.ConditionVolume = 0;
            this.ShortSellFlag = 'N';
            this.SpecialConditions = 'N';
            this.SpecialInstructions = string.Empty;
            this.OrderType = 'N';
            this.StopSecurityID = string.Empty;
            #endregion


            #region βαζουμε το embedded  ExecutionReportMessage να διαβασει  το FIX Message
            m_executionReportMessage.ParseMessage(message, logger);
            #endregion


            this.OriginalPriceType = ParsingHelpers.GetOrdType(message, logger);
            this.OrderLifetime = ParsingHelpers.GetTimeInForce(message, logger);


            var _ordStatus = m_executionReportMessage.OrdStatus;
            this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(_ordStatus);
            if (
                _ordStatus != OrdStatus.New &&
                _ordStatus != OrdStatus.Not_Released &&
                _ordStatus != OrdStatus.Inactive)
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{_ordStatus}'");
            }


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
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Orders -> [OrderClientID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.ClientIdentificationCode.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _clientID))
            {
                this.ClientID = _clientID;
            }
            else
            {
                this.ClientID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> ClientIdentificationCode.PartyID '{m_executionReportMessage.ClientIdentificationCode.PartyID}' cannot be Parsed as decimal");
            }
            /*
             * Περιμενω οτι το InvestmentDecisionID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Orders -> [OrderInvestmentDecisionID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.InvestmentDecisionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _investmentDecisionID))
            {
                this.InvestmentDecisionID = _investmentDecisionID;
            }
            else
            {
                this.InvestmentDecisionID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> InvestmentDecisionWithinFirm.PartyID '{m_executionReportMessage.InvestmentDecisionWithinFirm.PartyID}' cannot be Parsed as decimal");
            }
            /*
             * Περιμενω οτι το ExecutionWithinFirmID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Orders -> [OrderExecutionWithinFirmId] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.ExecutionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _executionWithinFirmID))
            {
                this.ExecutionWithinFirmID = _executionWithinFirmID;
            }
            else
            {
                this.ExecutionWithinFirmID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> ExecutionWithinFirm.PartyID '{m_executionReportMessage.ExecutionWithinFirm.PartyID}' cannot be Parsed as decimal");
            }
            /*
             * Περιμενω οτι το NonExecutingBrokerID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Orders -> [OrderNonExecutingBrokerID] [decimal](18, 0) NULL)
             */
            if (decimal.TryParse(m_executionReportMessage.NonExecutingBroker.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _nonExecutingBrokerID))
            {
                this.NonExecutingBrokerID = _nonExecutingBrokerID;
            }
            else
            {
                this.NonExecutingBrokerID = null;
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> NonExecutingBroker.PartyID '{m_executionReportMessage.NonExecutingBroker.PartyID}' cannot be Parsed as decimal");
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
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Entry_Confirmation;
        public string MemberID => m_executionReportMessage.ExecutingFirm.PartyID;
        public string TraderID => m_executionReportMessage.EnteringTrader.PartyID;
        public string VenueID => m_executionReportMessage.SecurityExchange;
        public char OrderType { get; private set; }
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
        public string CSDAccountID => m_executionReportMessage.Account;
        public char GOIFlag { get; private set; }
        public char ShortSellFlag { get; private set; }
        public string SecurityID => m_executionReportMessage.SecurityID;
        public char SecurityIDSource => m_executionReportMessage.SecurityIDSource;
        public string Currency { get; private set; }
        public double Price => m_executionReportMessage.Price;
        public double Volume => m_executionReportMessage.OrderQty;
        public double DisclosedVolume => m_executionReportMessage.MaxShow;
        public double AutoDisclosedVolume { get; private set; }
        public double LeavesQuantity => m_executionReportMessage.LeavesQty;
        public double AveragePrice => m_executionReportMessage.AvgPx;
        public double ConditionVolume { get; private set; }
        public char OrderLifetime { get; private set; }
        public char SpecialConditions { get; private set; }
        public char OriginalPriceType { get; private set; }
        public string StopSecurityID { get; private set; }
        public double StopPrice => m_executionReportMessage.StopPx;
        public string ExpirationDate => m_executionReportMessage.ExpireDate;

        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// Αυτο είναι το δικο μας Orders.OrderID
        /// </summary>
        public string ClOrdID => m_executionReportMessage.ClOrdID;
        public string OrderNote => m_executionReportMessage.Text;
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
        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        /// Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// </summary>
        public char FixOrderStatus => m_executionReportMessage.OrdStatus;
        public double CurrentCreditValue => m_executionReportMessage.CurrentCreditValue;
        public string ListID { get; private set; }
        public char DirectElectronicAccess { get; private set; }

        /// <summary>
        /// m_executionReportMessage.ClientIdentificationCode.PartyID;
        /// </summary>
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
        /// <summary>
        /// m_executionReportMessage.InvestmentDecisionWithinFirm.PartyID;
        /// </summary>
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
        /// <summary>
        /// m_executionReportMessage.ExecutionWithinFirm.PartyID
        /// </summary>
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
        /// <summary>
        /// m_executionReportMessage.NonExecutingBroker.PartyID;
        /// </summary>
        public decimal? NonExecutingBrokerID { get; private set; }


        public char TradingCapacity { get; private set; }
        public char LiquidityProvision { get; private set; }
        public string Timestamp { get; private set; }
        public char AlgoFlag { get; private set; }
        public char CommodityHedgingFlag { get; private set; }
        public string SpecialInstructions { get; private set; }
    }
}
