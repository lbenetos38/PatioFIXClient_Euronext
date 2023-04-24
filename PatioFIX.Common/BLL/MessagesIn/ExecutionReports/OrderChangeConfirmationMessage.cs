using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Order Change Confirmation ("TD")
    /// </summary>
    class OrderChangeConfirmationMessage : IODLMessage, IFixParserToODL
    {
        ExecutionReportMessage m_executionReportMessage = new ExecutionReportMessage();


        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου OrderChangeConfirmationMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            this.ChangedAutoDisclosedVolume = 0;
            this.Currency = string.Empty;
            this.ChangedGOIFlag = 'N';
            this.ListID = string.Empty;
            this.ChangedCommodityHedgingFlag = 'N';
            this.SpecialConditions = 'N';
            this.ChangedSpecialInstructions = string.Empty;
            this.ChangedShortSellFlag = 'N';
            #endregion

            #region βαζουμε το embedded  ExecutionReportMessage να διαβασει  το FIX Message
            m_executionReportMessage.ParseMessage(message, logger);
            #endregion

            this.ChangedOriginalPriceType = ParsingHelpers.GetOrdType(message, logger);
            this.ChangedLife = ParsingHelpers.GetTimeInForce(message, logger);


            var _ordStatus = m_executionReportMessage.OrdStatus;
            this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(_ordStatus);
            if (
                _ordStatus != OrdStatus.New &&
                _ordStatus != OrdStatus.PartiallyFilled &&
                _ordStatus != OrdStatus.Inactive &&
                _ordStatus != OrdStatus.Not_Released/*Δεσμευμενη, δλδ σε περιπτπωση ATC εντολης*/)
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderChangeConfirmationMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{_ordStatus}'");
            }

            ChangedDirectElectronicAccess = ParsingHelpers.GetOrderOrigination(message, logger);


            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Change_Confirmation;
        public string MemberID => m_executionReportMessage.Parties.ExecutingFirm.PartyID;
        public string TraderID => m_executionReportMessage.Parties.EnteringTrader.PartyID;
        public string VenueID => m_executionReportMessage.SecurityExchange;
        public char BoardID => m_executionReportMessage.BoardID;
        public string SecurityID => m_executionReportMessage.SecurityID;
        public char SecurityIDSource => m_executionReportMessage.SecurityIDSource;
        public string Currency { get; private set; }

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

        public double ChangedPrice => m_executionReportMessage.Price;
        public double ChangedVolume => m_executionReportMessage.OrderQty;
        public double ChangedDisclosedVolume => m_executionReportMessage.MaxShow;
        public double ChangedAutoDisclosedVolume { get; private set; }
        public double LeavesQuantity => m_executionReportMessage.LeavesQty;
        public double AveragePrice => m_executionReportMessage.AvgPx;
        public string ChangedCSDAccountID => m_executionReportMessage.Account;
        public char ChangedGOIFlag { get; private set; }
        public char ChangedShortSellFlag { get; private set; }
        public char ChangedOriginalPriceType { get; private set; }
        public char ChangedLife { get; private set; }
        public string ChangedExpirationDate => m_executionReportMessage.ExpireDate;
        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// Το τελευταιο ενεργο OrderID-ChngID-CancelID (ετσι οπως το ξερει το ATHEX)
        /// </summary>
        public string OrigClOrdID => m_executionReportMessage.OrigClOrdID;

        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// Αυτο είναι το δικο μας Changes.ChngID
        /// </summary>
        public string ClOrdID => m_executionReportMessage.ClOrdID;
        public string ChangedOrderNote => m_executionReportMessage.Text;
        public string ChangedClearingMemberID => m_executionReportMessage.Parties.ClearingFirm.PartyID;
        public char ChangedPositionEffect => m_executionReportMessage.PositionEffect;
        public char ChangedSettlType => m_executionReportMessage.SettlType;
        /// <summary>
        /// A single character alphanumeric type indicating the source of the Order. Possible values :
        /// ‘C’ CTCI –FIX
        ///	‘M’ ORAMA-ETW
        /// ‘R’ EMRW(ATHEX Supervision Application).
        /// ' ' OASIS
        /// </summary>
        public char OrigSource => m_executionReportMessage.OrigSource;
        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        ///  Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// </summary>
        public char FixOrderStatus => m_executionReportMessage.OrdStatus;
        public double CurrentCreditValue => m_executionReportMessage.CurrentCreditValue;
        public string ListID { get; private set; }
        public char SpecialConditions { get; private set; }
        public char ChangedDirectElectronicAccess { get; private set; }

        #region Parties
        /// <summary>
        /// 
        /// </summary>
        public decimal? ClientID => m_executionReportMessage.Parties.ClientID;
        /// <summary>
        /// 
        /// </summary>
        public char ClientIDQualifier => m_executionReportMessage.Parties.ClientIDQualifier;

        /// <summary>
        /// 
        /// </summary>
        public decimal? InvestmentDecisionID => m_executionReportMessage.Parties.InvestmentDecisionID;
        /// <summary>
        /// 
        /// </summary>
        public char InvestmentDecisionIDQualifier => m_executionReportMessage.Parties.InvestmentDecisionIDQualifier;

        /// <summary>
        /// 
        /// </summary>
        public decimal? ExecutionWithinFirmID => m_executionReportMessage.Parties.ExecutionWithinFirmID;
        /// <summary>
        /// 
        /// </summary>
        public char ExecutionWithinFirmIDQualifier => m_executionReportMessage.Parties.ExecutionWithinFirmIDQualifier;

        /// <summary>
        /// 
        /// </summary>
        public decimal? NonExecutingBrokerID => m_executionReportMessage.Parties.NonExecutingBrokerID;
        #endregion

        public string Timestamp { get; private set; }
        public char ChangedCommodityHedgingFlag { get; private set; }
        public string ChangedSpecialInstructions { get; private set; }
    }
}
