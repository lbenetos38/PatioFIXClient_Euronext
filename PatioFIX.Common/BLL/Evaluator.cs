using PatioFIX.Common.BLL;
using PatioFIX.Common.BLL.Evaluators;
using System.Diagnostics;

namespace PatioFIX.Common
{
    /// <summary>
    /// Προσοχή: Δεν είναι Thread Safe by design!
    /// Ενας Evaluator ανα worker thread
    /// 
    /// Ο λόγος έιναι ότι περιέχει ένα MessageParser το οποιο δεν είναι thread Safe. Ανατρεξτε εκει
    /// για περισσότερες λεπτομέρειες
    /// </summary>
    public sealed class Evaluator : BaseManager
    {
        const int _TOTAL_MESSAGES = 41;           //Η max τιμη του enumeration ODLMessageType, που τώρα είναι το 40 + 1;
        readonly MessageParser m_parser = null;
        readonly Logger theLogger = null;
        readonly BaseEvaluator[] m_evaluators = new BaseEvaluator[_TOTAL_MESSAGES];
        readonly Stopwatch watch = new Stopwatch();


        /// <summary>
        /// 
        /// </summary>
        public Evaluator()
        {
            m_parser = new MessageParser();
            theLogger = new Logger("Evaluator");


            #region create instances of all available evaluators
            m_evaluators[(int)ODLMessageTypeEnum.Unknown] = new UnknownEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Order_Entry] = new OrderEntryEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Order_Edit] = new OrderEditEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Order_Change] = new OrderChangeEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Trade_Report_Entry] = new TradeReportEntryEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Order_Mass_Cancel] = new OrderMassCancelEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Hit_n_Take_Order_Entry] = new HitnTakeOrderEntryEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Order_Mass_Cancel_Confirmation] = new OrderMassCancelConfirmationEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Order_Entry_Confirmation] = new OrderEntryConfirmationEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Order_Edit_Confirmation] = new OrderEditConfirmationEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Order_Change_Confirmation] = new OrderChangeConfirmationEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.New_Trade_Confirmation] = new NewTradeConfirmationEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Mass_Cancel] = new QuoteMassCancelEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Trade_Capture_Report] = new TradeCaptureReportEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Rejection] = new RejectionEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.OrderCancelReject] = new OrderCancelRejectEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Credit_Limit_Information] = new CreditLimitInformationEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Security_Status] = new SecurityStatusEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Security_Price] = new SecurityPriceEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Market_Status] = new MarketStatusEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.System_Status] = new SystemStatusEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Hit_n_Take_Order_Information] = new HitnTakeOrderInformationEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Reserved2] = new Reserved2Evaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Reserved3] = new Reserved3Evaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.DSS_Entry] = new DSSEntryEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.DSS_Entry_Confirmation] = new DSSEntryConfirmationEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.DSS_Trade] = new DSSTradeEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.DSS_Broadcast] = new DSSBroadcastEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Entry_Change] = new QuoteEntryChangeEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Cancel] = new QuoteCancelEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Status_Report] = new QuoteStatusReportEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Request] = new QuoteRequestEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Request_Confirmation] = new QuoteRequestConfirmationEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Request_Execution] = new QuoteRequestExecutionEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Request_Info] = new QuoteRequestInfoEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Responsibility_Suspend_Resume] = new QuoteResponsibilitySuspendResumeEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Alarm] = new QuoteAlarmEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Exchange_Notes] = new ExchangeNotesEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Quote_Mass_Cancel_Confirmation] = new QuoteMassCancelConfirmationEvaluator(this.OdlDal);
            //m_evaluators[(int)ODLMessageType.Business_Message_Reject] = new BusinessRejectEvaluator(this.OdlDal);
            m_evaluators[(int)ODLMessageTypeEnum.Ignored_Message] = new IgnoredEvaluator(this.OdlDal);
            #endregion
        }



        /// <summary>
        /// Κανει evaluation το συγκεκριμενο fixInMessage. Το μετατρεπει στο αντιστοιχο ODL μηνυμα, και στην συνεχεια
        /// καλει τον σωστο για αυτον τον τυπο evaluator για να αποθηκευτει στο σύστημα μας.
        /// </summary>
        /// <param name="fixInMessage"></param>
        /// <returns></returns>
        public EvaluationResult EvaluateMessage(FIXInMessage fixInMessage)
        {
            #region error handling
            if (fixInMessage.ODLMessageType == ODLMessageTypeEnum.Error)
            {
                MetricsProxy.Instance.OnParsingWarning();
                theLogger.Warning($"MessageParser.GetMessageType() RETURNED_ERROR for '{fixInMessage}'");
                return EvaluationResult.FailedOther;
            }
            if (fixInMessage.ODLMessageType == ODLMessageTypeEnum.Unknown)
            {
                MetricsProxy.Instance.OnParsingWarning();
                theLogger.Warning($"MessageParser.GetMessageType() RETURNED_UNKNOWN for '{fixInMessage}'");
                return EvaluationResult.FailedOther;
            }
            if ((int)fixInMessage.ODLMessageType < 0 || (int)fixInMessage.ODLMessageType > _TOTAL_MESSAGES)
            {
                MetricsProxy.Instance.OnParsingWarning();
                theLogger.Warning($"MessageParser.GetMessageType() RETURNED_OUT_OF_RANGE for '{fixInMessage}'");
                return EvaluationResult.FailedOther;
            }
            #endregion


            return _Evaluate(fixInMessage, fixInMessage.ODLMessageType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixInMessage"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        EvaluationResult _Evaluate(FIXInMessage fixInMessage, ODLMessageTypeEnum messageType)
        {
            /*
			 * 
			 */
            MetricsProxy.Instance.OnReceive(messageType);


            /*
             * Σπαμε το μηνυμα και παίρνουμε το αντιστοιχο msgObject:
             */
            var odlMessage = m_parser.GetODLMessageObject(fixInMessage.Message, messageType);
            if (odlMessage == null)
            {
                MetricsProxy.Instance.OnParsingWarning();
                theLogger.Warning($"m_parser.GetODLMessageObject() RETURNED_NULL_OBJECT for '{fixInMessage}'");
                if (m_parser.LastException != null)
                {
                    theLogger.Warning(m_parser.LastException.Message);
                }
                return EvaluationResult.FailedOther;
            }

            /*
             * Τραβαμε απο τον πίνακα με τους evaluators αυτον που εντιστοιχεί στον τύπο του μηνύματος μας:
             */
            var evl = m_evaluators[(int)messageType];
            if (evl == null)
            {
                MetricsProxy.Instance.OnParsingWarning();
                theLogger.Warning($"NO_REGISTERED_EVALUATOR_FOR ({messageType}) '{fixInMessage}'");
                return EvaluationResult.FailedOther;
            }


            watch.Restart();
            var evalResult = evl.Evaluate(fixInMessage, odlMessage);
            watch.Stop();
            MetricsProxy.Instance.OnEvaluationIime(messageType, watch.ElapsedTicks);

            return evalResult;
        }


        /// <summary>
        /// Yπαρχουν καποιοι τύποι μηνύματος που τους αγνοούμε 100% είτε για admin είτε για broker
        /// Υπαρχουν μηνύματα που αγνοεί μονο ο Broker
        /// Υπαρχουν μηνύματα που αγνοεί μονο ο Administrator
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool DoWeIgnoreMessageType(ODLMesssageSource source, ODLMessageTypeEnum type)
        {
            if (type == ODLMessageTypeEnum.Ignored_Message)
            {
                return true;
            }

            #region Μηπως αγνοούμε τελειως τετοιου είδους μηνύματα?
            if (
                   type == ODLMessageTypeEnum.Order_Entry || type == ODLMessageTypeEnum.Order_Edit || type == ODLMessageTypeEnum.Order_Change ||

                   type == ODLMessageTypeEnum.Trade_Report_Entry || type == ODLMessageTypeEnum.Order_Mass_Cancel ||
                   type == ODLMessageTypeEnum.Hit_n_Take_Order_Entry || type == ODLMessageTypeEnum.Order_Mass_Cancel_Confirmation ||
                   type == ODLMessageTypeEnum.Reserved2 || type == ODLMessageTypeEnum.Reserved3 ||
                   type == ODLMessageTypeEnum.DSS_Entry || type == ODLMessageTypeEnum.DSS_Entry_Confirmation ||
                   type == ODLMessageTypeEnum.DSS_Trade || type == ODLMessageTypeEnum.DSS_Broadcast ||
                   type == ODLMessageTypeEnum.Quote_Entry_Change || type == ODLMessageTypeEnum.Quote_Cancel ||
                   type == ODLMessageTypeEnum.Quote_Status_Report || type == ODLMessageTypeEnum.Quote_Request ||
                   type == ODLMessageTypeEnum.Quote_Request_Confirmation || type == ODLMessageTypeEnum.Quote_Request_Execution ||
                   type == ODLMessageTypeEnum.Quote_Request_Info || type == ODLMessageTypeEnum.Quote_Responsibility_Suspend_Resume ||
                   type == ODLMessageTypeEnum.Quote_Mass_Cancel_Confirmation || type == ODLMessageTypeEnum.Quote_Alarm || type == ODLMessageTypeEnum.Quote_Mass_Cancel
                   )
            {
                return true;
            }
            #endregion


            #region Μήπως ο Broker αγνοεί τέτοιου είδους μηνύματα?
            if (source == ODLMesssageSource.Broker)
            {
                if (type == ODLMessageTypeEnum.Exchange_Notes)
                    return true;
                if (type == ODLMessageTypeEnum.Hit_n_Take_Order_Information)
                    return true;

                if (type == ODLMessageTypeEnum.Security_Status)
                    return true;
                if (type == ODLMessageTypeEnum.Security_Price)
                    return true;
                if (type == ODLMessageTypeEnum.Credit_Limit_Information)
                    return true;
                if (type == ODLMessageTypeEnum.Market_Status)
                    return true;
                if (type == ODLMessageTypeEnum.System_Status)
                    return true;

                if (type == ODLMessageTypeEnum.New_Trade_Confirmation)
                    return true;

                if (type == ODLMessageTypeEnum.Trade_Capture_Report)
                    return true;
            }
            #endregion


            #region Μήπως ο Administrator αγνοεί τέτοιου είδους μηνύματα?
            if (source == ODLMesssageSource.Administrator)
            {

            }
            #endregion


            return false;
        }

    }
}
