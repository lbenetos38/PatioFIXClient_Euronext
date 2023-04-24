using PatioFIX.Common.FixSupport;


namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Order Edit Confirmation ("TC")
    /// </summary>
    class OrderEditConfirmationMessage : IODLMessage, IFixParserToODL
    {
        ExecutionReportMessage m_executionReportMessage = new ExecutionReportMessage();


        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου OrderEditConfirmationMessage Instance
        /// Εδω εχουμε ενα Execution Report με ExecType ισο με 'D' 	Restated
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            this.Currency = string.Empty;
            this.ListID = string.Empty;
            #endregion


            #region βαζουμε το embedded  ExecutionReportMessage να διαβασει  το FIX Message
            m_executionReportMessage.ParseMessage(message, logger);
            #endregion


            /*
             * εδω το EditType πρεπει να δωσουμε τις εξης τιμες:
             *      ‘C’ Cancel order
             *      ‘S’ Suspend (deactivate) order
             *      ‘U’ Unsuspend (activate) order.
             */
            var _ordStatus = m_executionReportMessage.OrdStatus;
            if (_ordStatus == OrdStatus.New || _ordStatus == OrdStatus.Not_Released)
            {
                EditType = 'U';
                ODLOrderStatus = "O";//Open
            }
            else if (_ordStatus == OrdStatus.PartiallyFilled)
            {
                EditType = 'U';
                ODLOrderStatus = "O";//Open
            }
            else if (_ordStatus == OrdStatus.Inactive)
            {
                EditType = 'S';
                ODLOrderStatus = "I";//Inactive
            }
            else if (_ordStatus == OrdStatus.Canceled)
            {
                EditType = 'C';
                ODLOrderStatus = "X";//Cancel
            }
            else if (_ordStatus == OrdStatus.Expired)
            {
                EditType = 'C';
                ODLOrderStatus = "EP";//GTC, GTD expired status
            }
            else
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(_ordStatus);

                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderEditConfirmationMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{_ordStatus}'");
            }


            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;


            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Edit_Confirmation;

        public string MemberID => m_executionReportMessage.Parties.ExecutingFirm.PartyID;
        public string TraderID => m_executionReportMessage.Parties.EnteringTrader.PartyID;
        public string SecurityExchange => m_executionReportMessage.SecurityExchange;
        public string SecurityID => m_executionReportMessage.SecurityID;
        public char SecurityIDSource => m_executionReportMessage.SecurityIDSource;


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
        /// OrigClOrdID (Tag = 41, Type: String)
        /// Το προηγουμενο ενεργο Orders::OrderID-ChngID-CancelID
        /// </summary>
        public string OrigClOrdID => m_executionReportMessage.OrigClOrdID;

        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// Αυτο είναι ειτε το Cancels..CancelID
        /// </summary>
        public string ClOrdID => m_executionReportMessage.ClOrdID;
        public string CSDAccountID => m_executionReportMessage.Account;
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

        public string ExpirationDate => m_executionReportMessage.ExpireDate;
        /// <summary>
        /// 
        /// </summary>
        public double LeavesQuantity => m_executionReportMessage.LeavesQty;
        /// <summary>
        /// Denotes the disclosed volume of the order.
        /// Required if ExecType is not 8/F/G/H
        /// </summary>
        public double DisclosedVolume => m_executionReportMessage.MaxShow;
        /// <summary>
        /// 
        /// </summary>
        public double AveragePrice => m_executionReportMessage.AvgPx;
        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        /// Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// </summary>
        public char FixOrderStatus => m_executionReportMessage.OrdStatus;
        /// <summary>
        /// A single character alphanumeric type indicating the source of the Order. Possible values :
        /// ‘C’ CTCI –ODL
        ///	‘M’ ORAMA-ETW
        /// ‘R’ EMRW(ATHEX Supervision Application).
        /// ' ' OASIS
        /// </summary>
        public char CancelSource => m_executionReportMessage.OrigSource;
        public char CancelReasonCode => m_executionReportMessage.CancelReasonCode;
        /// <summary>
        /// ‘C’ Cancel order.
        /// ‘S’ Suspend (deactivate) order.
        /// ‘U’ Unsuspend (activate) order.
        /// </summary>
        public char EditType { get; private set; }
        public double CurrentCreditValue => m_executionReportMessage.CurrentCreditValue;
        public string OrderNote => m_executionReportMessage.Text;
        public string ListID { get; private set; }
        public string Timestamp { get; private set; }
    }
}
