using PatioFIX.Common.FixSupport;


namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Rejection ("TR")
    /// </summary>
    class RejectMessage : IODLMessage, IFixParserToODL
    {
        ExecutionReportMessage m_executionReportMessage = new ExecutionReportMessage();


        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου RejectMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL

            #endregion


            #region βαζουμε το embedded  ExecutionReportMessage να διαβασει  το FIX Message
            m_executionReportMessage.ParseMessage(message, logger);
            #endregion


            this.OriginalPriceType = ParsingHelpers.GetOrdType(message, logger, warnIfNotExists: false);
            this.OrderLifetime = ParsingHelpers.GetTimeInForce(message, logger, warnIfNotExists: false);


            var _ordStatus = m_executionReportMessage.OrdStatus;
            this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(_ordStatus);
            if (_ordStatus != OrdStatus.Rejected)
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEntryConfirmationMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{_ordStatus}'");
            }

            /*
             * Τα παρακατω 3 μπορει να εχουν και μπορει να μην εχουν τιμες.
             * Θα διορθωθουν στην πορεια στον RejectionEvaluator
             */
            this.SecurityID = m_executionReportMessage.SecurityID;
            this.SecurityIDSource = m_executionReportMessage.SecurityIDSource;
            this.SecurityExchange = m_executionReportMessage.SecurityExchange;

            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Rejection;

        public string Timestamp { get; private set; }
        public string MemberID => m_executionReportMessage.ExecutingFirm.PartyID;
        public string TraderID => m_executionReportMessage.EnteringTrader.PartyID;

        public string SecurityID { get; internal set; }
        public char SecurityIDSource { get; internal set; }
        public string SecurityExchange { get; internal set; }
        public string CSDAccountID => m_executionReportMessage.Account;

        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// Το προηγουμενο ενεργο Orders::OrderID-ChngID-CancelID
        /// </summary>
        public string OrigClOrdID => m_executionReportMessage.OrigClOrdID;
        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// Αυτο είναι ειτε το Orders::OrderID-ChngID-CancelID
        /// </summary>
        public string ClOrdID => m_executionReportMessage.ClOrdID;

        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        /// Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// </summary>
        public char FixOrderStatus => m_executionReportMessage.OrdStatus;


        #region τα παρακατω πεδια εχουν τιμες οταν η απορριψη αφορα εντολη που υπαρχει στο ATHEX (αλλαγη volume, τιμης,....)
        /// <summary>
        /// OrderID (Tag = 37, Type: String)
        /// Unique identifier for Order as assigned by sell-side (broker, exchange, ECN). 
        /// Uniqueness must be guaranteed within a single trading day. 
        /// Firms which accept multi-day orders should consider embedding a date within the OrderID (37) field to assure uniqueness across days. 
        /// <para>Οταν εχουμε μια νεα εντολη και απορριφθει, τοτε σε αυτο το πεδίο εχουμε 'NONE' ή empty string </para>
        /// </summary>
        public string ExchangeOrderID => m_executionReportMessage.OrderID;
        /// <summary>
        /// 
        /// </summary>
        public string OrderNumber => m_executionReportMessage.OrderNumber;
        public string OrderDate => m_executionReportMessage.OrderDate;
        #endregion


        #region τα παρακατω πεδια εχουν τιμες οταν η απορριψη αφορα την δημιουργία νεας εντολης
        public char OriginalPriceType { get; private set; }
        public char OrderLifetime { get; private set; }
        public double Volume => m_executionReportMessage.OrderQty;
        public double LeavesQuantity => m_executionReportMessage.LeavesQty;
        public double CumQty => m_executionReportMessage.CumQty;
        public double AveragePrice => m_executionReportMessage.AvgPx;
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
        #endregion


        public string RejectReasonCode => m_executionReportMessage.RejectReasonCode;
        public string RejectReason => m_executionReportMessage.Text;


    }
}
