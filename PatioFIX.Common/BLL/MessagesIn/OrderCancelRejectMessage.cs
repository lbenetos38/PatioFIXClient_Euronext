using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// 
    /// </summary>
    class OrderCancelRejectMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου OrderCancelRejectMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            //this.MemberID = string.Empty;
            //this.TraderID = string.Empty;
            //this.SecurityID = string.Empty;
            //this.SecurityIDSource = default(char);
            //this.CSDAccountID = string.Empty;
            #endregion


            ExchangeOrderID = message[Tags.OrderID].AsString;           //REQUIRED
            ClOrdID = message[Tags.ClOrdID].AsString;                   //REQUIRED
            this.OrigClOrdID = message[Tags.OrigClOrdID].AsString;      //REQUIRED

            this.FixOrderStatus = message[Tags.OrdStatus].AsChar;       //REQUIRED
            this.ODLOrderStatus = ParsingHelpers.map_FIXOrdStatus_To_ODLOrderStatus(this.FixOrderStatus);
            if (this.FixOrderStatus != OrdStatus.Rejected)
            {
                /*
                 * Μας εχει ερθει καποιο OrdStatus μη αναμενομενο
                 */
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"OrderCancelRejectMessage -> tag39 (OrdStatus) has UNANTICIPATED_VALUE of '{this.FixOrderStatus}'");
            }
            /*
             * Identifies the type of request that a Order Cancel Reject (9) is in response to. 
             * 1 	Order Cancel Request (F)
             * 2 	Order Cancel/Replace Request (G)
             */
            CxlRejResponseTo = message[Tags.CxlRejResponseTo].AsChar;   //REQUIRED

            if (message.Contains(Tags.CxlRejReason))
                CxlRejReason = message[Tags.CxlRejReason].AsInt;
            if (message.Contains(Tags.Text)) this.RejectReason = message[Tags.Text].AsString;

            if (message.Contains(Tags.TransactTime))
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            else
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;



            if (message.Contains(CustomTags.RejectReasonCode))
                RejectReasonCode = message[CustomTags.RejectReasonCode].AsString;
            if (message.Contains(Tags.SecurityExchange))
                SecurityExchange = message[Tags.SecurityExchange].AsString;


            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.OrderCancelReject;

        /// <summary>
        /// OrderID (Tag = 37, Type: String)
        /// Unique identifier for Order as assigned by sell-side (ATHEX).
        /// </summary>
        public string ExchangeOrderID { get; private set; }
        /// <summary>
        /// ClOrdID (Tag = 11, Type: String)
        /// Unique identifier for Order as assigned by the buy-side (EQUITIES)
        /// </summary>
        public string ClOrdID { get; private set; }
        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// ClOrdID (11) of the previous order (NOT the initial order of the day) as assigned 
        /// by the EQUITIES, used to identify the previous order in cancel and cancel/replace requests. 
        /// </summary>
        public string OrigClOrdID { get; private set; }
        /// <summary>
        /// CxlRejResponseTo (Tag = 434, Type: char)
        /// Identifies the type of request that a Order Cancel Reject (9) is in response to. 
        ///     '1' 	Order Cancel Request (F)
        ///     '2' 	Order Cancel/Replace Request (G)
        /// </summary>
        public char CxlRejResponseTo { get; private set; }
        /// <summary>
        /// CxlRejReason (Tag = 102, Type: int)
        /// Code to identify reason for cancel rejection.
        /// </summary>
        public int CxlRejReason { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string RejectReason { get; private set; }
        /// <summary>
        /// A 3-character numeric field used to indicate to a member firm 
        /// the reason that a requested action could not take place
        /// “001” Incorrect Message Type
        /// “002” Incorrect Member ID
        /// “003” Member not active
        /// .....
        /// “036” Order already matched
        /// “037” Order already canceled
        /// “038” Order suspended
        /// .....
        /// </summary>
        public string RejectReasonCode { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string Timestamp { get; private set; }


        /// <summary>
        /// Αυτο ειναι το legacy ODL OrderStatus
        /// </summary>
        public string ODLOrderStatus { get; private set; }
        /// <summary>
        /// Αυτο ειναι το πραγματικο FIX OrdStatus (Tag39)
        /// OrdStatus (Tag = 39, Type: char)
        ///     '0' 	New
        ///     '1' 	Partially filled
        ///     '2' 	Filled
        ///     '3' 	Done for day
        ///     '4' 	Canceled
        ///     '6' 	Pending Cancel(e.g.result of Order Cancel Request)
        ///     '7' 	Stopped
        ///     '8' 	Rejected
        ///     '9' 	Suspended
        ///     'A' 	Pending New
        ///     'B' 	Calculated
        ///     'C' 	Expired
        ///     'D' 	Accepted for bidding
        ///     'E' 	Pending Replace(e.g.result of Order Cancel/Replace Request)
        /// </summary>
        public char FixOrderStatus { get; private set; }
        public string SecurityExchange { get; set; }

        public string MemberID;
        public string TraderID;

        public string CSDAccountID;
        public string MemberOrderNumber;
        public string SecurityID;
    }
}
