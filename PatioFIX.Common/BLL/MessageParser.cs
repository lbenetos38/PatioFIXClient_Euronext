using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.FixSupport;
using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// Προσοχή: Δεν είναι Thread Safe by design for performance reasons
    /// Eνας MessageParser ανα worker thread.
    /// 
    /// Και αυτό γιατί στο parser δεν δημιουρεί νέα objects αλλα επστρεφει συνέχεια τα ίδια και τα ίδια
    /// με αλλαγμένες φυσικά τις τιμές των πεδίων τους μετά απο κάθε κλήση της GetMessageObject()
    /// </summary>
    public sealed class MessageParser
    {
        const int _TOTAL_MESSAGES = 41;           //Η max τιμη του enumeration ODLMessageType, που τώρα είναι το 40 + 1;
        readonly static Logger theLogger = new Logger("MessageParser");
        readonly IFixParserToODL[] m_messages = new IFixParserToODL[_TOTAL_MESSAGES];

        public Exception LastException { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageParser()
        {
            #region create instances of all available messages
            m_messages[(int)ODLMessageTypeEnum.Unknown] = null;
            //m_messages[(int)ODLMessageType.Order_Entry] = new OrderEntryMessage();
            //m_messages[(int)ODLMessageType.Order_Edit] = new OrderEditMessage();
            //m_messages[(int)ODLMessageType.Order_Change] = new OrderChangeMessage();
            //m_messages[(int)ODLMessageType.Trade_Report_Entry] = new TradeReportEntryMessage();
            //m_messages[(int)ODLMessageType.Order_Mass_Cancel] = new OrderMassCancelMessage();
            //m_messages[(int)ODLMessageType.Hit_n_Take_Order_Entry] = new HitnTakeOrderEntryMessage();
            //m_messages[(int)ODLMessageType.Order_Mass_Cancel_Confirmation] = new OrderMassCancelConfirmationMessage();
            m_messages[(int)ODLMessageTypeEnum.Order_Entry_Confirmation] = new OrderEntryConfirmationMessage();
            m_messages[(int)ODLMessageTypeEnum.Order_Edit_Confirmation] = new OrderEditConfirmationMessage();
            m_messages[(int)ODLMessageTypeEnum.Order_Change_Confirmation] = new OrderChangeConfirmationMessage();
            m_messages[(int)ODLMessageTypeEnum.New_Trade_Confirmation] = new NewTradeConfirmationMessage();
            //m_messages[(int)ODLMessageType.Quote_Mass_Cancel] = new QuoteMassCancelMessage();
            m_messages[(int)ODLMessageTypeEnum.Trade_Capture_Report] = new TradeCaptureReportMessage();
            m_messages[(int)ODLMessageTypeEnum.Rejection] = new RejectMessage();
            m_messages[(int)ODLMessageTypeEnum.OrderCancelReject] = new OrderCancelRejectMessage();
            m_messages[(int)ODLMessageTypeEnum.Credit_Limit_Information] = new CreditLimitInfoMessage();
            m_messages[(int)ODLMessageTypeEnum.Security_Status] = new SecurityStatusMessage();
            m_messages[(int)ODLMessageTypeEnum.Security_Price] = new SecurityPricesMessage();
            m_messages[(int)ODLMessageTypeEnum.Market_Status] = new MarketStatusMessage();
            //m_messages[(int)ODLMessageType.System_Status] = new SystemStatusMessage();
            //m_messages[(int)ODLMessageType.Hit_n_Take_Order_Information] = new HitnTakeOrderInfoMessage();
            //m_messages[(int)ODLMessageType.Reserved2] = null;
            //m_messages[(int)ODLMessageType.Reserved3] = null;
            //m_messages[(int)ODLMessageType.DSS_Entry] = new DSSEntryMessage();
            //m_messages[(int)ODLMessageType.DSS_Entry_Confirmation] = new DSSEntryConfirmationMessage();
            //m_messages[(int)ODLMessageType.DSS_Trade] = new DSSTradeMessage();
            //m_messages[(int)ODLMessageType.DSS_Broadcast] = new DSSBroadcastMessage();
            //m_messages[(int)ODLMessageType.Quote_Entry_Change] = new QuoteEntryChangeMessage();
            //m_messages[(int)ODLMessageType.Quote_Cancel] = new QuoteCancelMessage();
            //m_messages[(int)ODLMessageType.Quote_Status_Report] = new QuoteStatusReportMessage();
            //m_messages[(int)ODLMessageType.Quote_Request] = new QuoteRequestMessage();
            //m_messages[(int)ODLMessageType.Quote_Request_Confirmation] = new QuoteRequestConfirmationMessage();
            //m_messages[(int)ODLMessageType.Quote_Request_Execution] = new QuoteRequestExecutionMessage();
            //m_messages[(int)ODLMessageType.Quote_Request_Info] = new QuoteRequestInfoMessage();
            //m_messages[(int)ODLMessageType.Quote_Responsibility_Suspend_Resume] = new QuoteResponsibilitySuspendResumeMessage();
            //m_messages[(int)ODLMessageType.Quote_Alarm] = new QuoteAlarmMessage();
            m_messages[(int)ODLMessageTypeEnum.Exchange_Notes] = new ExchangeNotesMessage();
            //m_messages[(int)ODLMessageType.Quote_Mass_Cancel_Confirmation] = new QuoteMassCancelConfirmationMessage();
            //m_messages[(int)ODLMessageType.Business_Message_Reject] = new BusinessMessageReject();
            m_messages[(int)ODLMessageTypeEnum.Ignored_Message] = new IgnoredMessage();
            #endregion
        }



        /// <summary>
        /// Μας δίνει γρήγορα και με μηδενικό κόστος το MessageType του μηνύματος ετσι οπως 
        /// το γνωριζαμε απο το παλιο πρωτοκολλο ODL
        /// (Thread Safe)
        /// </summary>
        /// <param name="fixMessage">Το incoming fixMessage του οποίου το ODLMessageType θελουμε</param>
        /// <returns></returns>
        public static ODLMessageTypeEnum GetODLMessageType(FIXMessage fixMessage)
        {
            if (fixMessage == null)
                return ODLMessageTypeEnum.Error;


            var msgType = fixMessage.MsgType;


            if (msgType[0] == '8')
            {
                /*
				 * Execution Report (MsgType = 8, FIXML = ExecRpt)
				 */
                var execType = fixMessage[Tags.ExecType].AsChar;

                if (execType == '0')
                {
                    /*
                     * '0' 	New
                     * Εχουμε επιβεβαιωση οτι μια εντολη μπηκε πετυχημενα στην αογορα (market)
                     * Μπορει να είναι market, limit, Stop, Stop Limit, On Close....
                     */
                    return ODLMessageTypeEnum.Order_Entry_Confirmation;
                }
                else if (execType == '4')
                {
                    //'4' 	Canceled
                    return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC
                }
                else if (execType == '5')
                {
                    /*  '5' 	Replace
                     * 
                     *  Τετοιο FIX Message παιρνουμε 
                     *  
                     *  1)  οταν μια 'Suspended' εντολη ενεργοποιείται ξανα (Release from suspension)               -> Order_Edit_Confirmation
                     *      (σε αυτη την περιπτωση το μηνυμα εχει πιο λιγα πεδία και επιπλεον ΠΕΡΙΕΧΕΙ ExecInst (Tag = 18) ως εξης '18=q')
                     *  
                     *  ή
                     *  
                     *  2)  απο αλλαγες/τροποποιησεις (changes) της εντολης μας                                     -> Order_Change_Confirmation
                     *      (σε αυτη την περιπτωση το μηνυμα περιεχει πιο πολλα πεδία (OrdType, TimeInForce , Text, Price,PositionEffect ,SettlType) και ΔΕΝ ΠΕΡΙΕΧΕΙ το  ExecInst(18))
                     */
                    if (fixMessage.Contains(Tags.ExecInst))
                    {
                        var _execInst = fixMessage[Tags.ExecInst].AsChar;
                        if(_execInst == /*Release from suspension*/'q')
                        {
                            //Unsuspended
                            return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC
                        }

                        throw new PtException($"ExecutionReport with ExecType=Replace (150=5) -> UNEXPECTED Value for tag ExecInst '18={_execInst}'");
                    }
                    else
                    {
                        //Change
                        return ODLMessageTypeEnum.Order_Change_Confirmation;//TD
                    }
                }
                else if (execType == '8')
                {
                    //'8' 	Rejected
                    return ODLMessageTypeEnum.Rejection;//TR
                }
                else if (execType == '9')
                {
                    /*
                     * 9     Suspended
                     * 
                     * Επισης το μηνυμα περιεχει και το  ExecInst (Tag = 18) ως εξης '18=S'
                     * 
                     */
                    return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC
                }
                else if (execType == 'C')
                {
                    //C     Expired
                    //return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC ΠΡεπει να το δω πρωτα
                }
                else if (execType == 'D')
                {
                    /*
                     * 'D'  Restated ( ExecutionRpt (8) sent unsolicited by sellside (ATHEX), with ExecRestatementReason (378) set) 
                     */
                    if (fixMessage.Contains(Tags.ExecRestatementReason))
                    {
                        var _rreason = fixMessage[Tags.ExecRestatementReason].AsInt;
                        char _ordStatus = OrdStatus.Undefined;

                        if (fixMessage.Contains(Tags.OrdStatus))
                        {
                            _ordStatus = fixMessage[Tags.OrdStatus].AsChar;

                            if (_rreason == /*GT renewal / restatement (no corporate action)*/1)
                            {
                                /*
                                 * -Το λαμβανουμε για εντολες μακρας διαρκειας που ειναι ακομα ενεργες (TimeInForce == ('Good Till Cancel (GTC)','Good Till Date (GTD)')
                                 *      Τετοια μηνυματα μας ερχονται οταν ανοιγει το ATHEX, και μας ενημερωνει για αυτες τις εντολες πο υπαρχουν στο συστημα του.
                                 *      Εχουν ολα ExecRestatementReason (378) == 1
                                 */

                                if (_ordStatus == OrdStatus.New)
                                {
                                    return ODLMessageTypeEnum.Order_Entry_Confirmation;//TB
                                }
                                if (_ordStatus == OrdStatus.Inactive)
                                {
                                    return ODLMessageTypeEnum.Order_Entry_Confirmation;//TB
                                }
                                if (_ordStatus == OrdStatus.Expired)
                                {
                                    return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC
                                }

                                MetricsProxy.Instance.OnParsingWarning();
                                theLogger.Warning($"ExecutionReport with ExecType=Restated (150=D) and ExecRestatementReason (378) = 1 --> Unexpected OrdStatus (39) = '{_ordStatus}'");
                                return ODLMessageTypeEnum.Unknown;
                            }
                            else if (_rreason == /*Verbal change*/2)
                            {
                                return ODLMessageTypeEnum.Order_Edit_Confirmation;//TC
                            }
                            else if (_rreason == /*Broker option*/4)
                            {
                                /*
                                 * -Το λαμβανουμε για μια Market εντολη, Και ειναι δυο ειδων.
                                 *      1) Η Market εντολη ακυρωθηκε (γιατι δεν εγινε match)
                                 *      2) H Market εντολη μετατραπηκε σε Limit (γιατι εγινε προηγουμενως Partially filled)
                                 */

                                if (_ordStatus == OrdStatus.New)
                                {
                                    return ODLMessageTypeEnum.Order_Change_Confirmation;    //TD
                                }
                                if (_ordStatus == OrdStatus.PartiallyFilled)
                                {
                                    return ODLMessageTypeEnum.Order_Change_Confirmation;    //TD
                                }
                                if (_ordStatus == OrdStatus.Filled)
                                {
                                    return ODLMessageTypeEnum.Order_Change_Confirmation;    //TD
                                }
                                if (_ordStatus == OrdStatus.Canceled)
                                {
                                    return ODLMessageTypeEnum.Order_Edit_Confirmation;      //TC
                                }
                                if (_ordStatus == OrdStatus.Expired)
                                {
                                    return ODLMessageTypeEnum.Order_Edit_Confirmation;      //TC
                                }
                                if (_ordStatus == OrdStatus.Inactive)
                                {
                                    return ODLMessageTypeEnum.Order_Change_Confirmation;    //TD
                                }


                                MetricsProxy.Instance.OnParsingWarning();
                                theLogger.Warning($"ExecutionReport with ExecType Restated (150=D) and ExecRestatementReason (378) = 4 -> UNEXPECTED OrdStatus (39) = '{_ordStatus}'");
                                return ODLMessageTypeEnum.Unknown;
                            }


                            throw new PtException($"ExecutionReport with ExecType=Restated (150=D) -> UNSUPPORTED ExecRestatementReason (378) = '{_rreason}'");
                        }

                        throw new PtException($"ExecutionReport with ExecType=Restated (150=D) -> NO_TAG 39 (OrdStatus) found");
                    }

                    throw new PtException("ExecutionReport with ExecType=Restated (150=D) -> NO_TAG 378 (ExecRestatementReason) found");
                }
                else if (execType == 'F')
                {
                    //'F' 	Trade (partial fill or fill)
                    return ODLMessageTypeEnum.New_Trade_Confirmation;
                }
                else if (execType == 'G')
                {
                    //G     Trade Correct

                }
                else if (execType == 'H')
                {
                    //H     Trade Cancel

                }
            }
            else if (msgType[0] == '9')
            {
                /*
                 * '9' 	Order Cancel Reject (9)
                 * The Order Cancel Reject (9) message is issued by the broker upon receipt of a Cancel Request (F) or Order Cancel/Replace Request (G) message which cannot be honored. 
                 */
                return ODLMessageTypeEnum.OrderCancelReject;
            }
            else if (msgType[0] == 'A')
            {
                if (msgType.Length == 1)
                {

                }
                else if (msgType.Length == 2)
                {
                    //'AA'    Derivative Security List(AA)
                    //'AB'    New Order -Multileg(AB)
                    //'AC'    Multileg Order Cancel / Replace Request(AC)
                    //'AD'    Trade Capture Report Request(AD)
                    //'AE'    Trade Capture Report(AE)
                    if (msgType[1] == 'E')
                        return ODLMessageTypeEnum.Trade_Capture_Report;// Trade Capture Report(AE)           | Στο ODL ερχεται σαν TF μηνυμα
                    //'AF'    Order Mass Status Request(AF)
                    //'AG'    Quote Request Reject(AG)
                    //'AH'    RFQ Request(AH)
                    //'AI'    Quote Status Report(AI)
                    if (msgType[1] == 'I')
                        return ODLMessageTypeEnum.Quote_Status_Report;// Quote Status Report (AI)
                    //'AJ'    Quote Response(AJ)
                    //'AK'    Confirmation(AK)
                    //'AL'    Position Maintenance Request(AL)
                    //'AM'    Position Maintenance Report(AM)
                    //'AN'    Request for Positions(AN)
                    //'AO'    Request for Positions Ack (AO)
                    //'AP'    Position Report(AP)
                    //'AQ'    Trade Capture Report Request Ack(AQ)
                    //'AR'    Trade Capture Report Ack(AR)
                    if (msgType[1] == 'R')
                        return ODLMessageTypeEnum.Ignored_Message;// Trade Capture Report Ack(AR)       | Στο ODL ερχεται σαν TF μηνυμα
                    //'AS'    Allocation Report(AS)
                    //'AT'    Allocation Report Ack(AT)
                    //'AU'    Confirmation Ack(AU)
                    //'AV'    Settlement Instruction Request(AV)
                    //'AW'    Assignment Report(AW)
                    //'AX'    Collateral Request(AX)
                    //'AY'    Collateral Assignment(AY)
                    //'AZ'    Collateral Response(AZ)

                }
            }
            else if (msgType[0] == 'B')
            {
                if (msgType.Length == 1)
                {
                    /*
                     * News (MsgType = B, FIXML = News)
                     */
                    if (fixMessage.Contains(Tags.Headline))
                    {
                        char headLine = fixMessage[Tags.Headline].AsChar;

                        if (headLine == 'A')
                            return ODLMessageTypeEnum.Ignored_Message;//QUOTE ALARM (5.3.4)
                        if (headLine == 'W')
                            return ODLMessageTypeEnum.Ignored_Message;//QUOTE WARNING (5.3.4)
                        if (headLine == 'S')
                            return ODLMessageTypeEnum.Ignored_Message;//SUSPEND QUOTATION RESPONSIBILITY (5.3.5)
                        if (headLine == 'R')
                            return ODLMessageTypeEnum.Ignored_Message;//RESUME QUOTATION RESPONSIBILITY (5.3.5)
                        if (headLine == 'C')
                            return ODLMessageTypeEnum.Credit_Limit_Information; //(5.5.4)
                        if (headLine == 'M')
                            return ODLMessageTypeEnum.Exchange_Notes;           //(5.5.5)
                    }
                }
                else if (msgType.Length == 2)
                {
                    //'BA'    Collateral Report(BA)
                    //'BB'    Collateral Inquiry(BB)
                    //'BC'    Network Status Request(BC)
                    //'BD'    Network Status Response(BD)
                    //'BE'    User Request(BE)
                    //'BF'    User Response(BF)
                    //'BG'    Collateral Inquiry Ack(BG)
                    //'BH'    Confirmation Request(BH)
                }
            }
            else if (msgType[0] == 'R')
            {
                return ODLMessageTypeEnum.Quote_Request_Info;
            }
            else if (msgType[0] == 'f')
            {
                /*
				 * Security Status (MsgType = f, FIXML = SecStat)
				 */


                if (fixMessage.Contains(CustomTags.ATHEXMsgType))
                {
                    var athexMsgType = fixMessage[CustomTags.ATHEXMsgType].AsString;

                    if (athexMsgType == "CA")
                        return ODLMessageTypeEnum.Security_Status;
                    if (athexMsgType == "CD")
                        return ODLMessageTypeEnum.Security_Price;
                }

                if (fixMessage.Contains(CustomTags.SecurityStatus))
                    return ODLMessageTypeEnum.Security_Status;

                if (fixMessage.Contains(CustomTags.SecurityPrice))
                    return ODLMessageTypeEnum.Security_Price;
            }
            else if (msgType[0] == 'h')
            {
                /*
				 * Trading Session Status (MsgType = h, FIXML = TrdgSesStat)
				 */
                return ODLMessageTypeEnum.Market_Status;
            }
            else if (msgType[0] == 'j')
            {
                /*
				 * Business Message Reject 
				 */
                return ODLMessageTypeEnum.Business_Message_Reject;
            }



            return ODLMessageTypeEnum.Unknown;
        }


        /// <summary>
        /// Διαβαζει το fixMessage και επιστρεφει ένα IMessage οδηγουμενο απο το messageType (που εχει ήδη βρεθεί)
        /// 
        /// Προσοχή, ένα MessageParser για κάθε active thread.
        /// Και αυτό γιατί στο parser δεν δημιουρεί νέα objects αλλα επστρεφει συνέχεια τα ίδια και τα ίδια
        /// με αλλαγμένες φυσικά τις τιμές των πεδίων τους μετά απο κάθε κλήση της ParseMessage()
        /// </summary>
        /// <param name="fixMessage"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public IODLMessage GetODLMessageObject(FIXMessage fixMessage, ODLMessageTypeEnum messageType)
        {
            LastException = null;
            try
            {
                /*βρισκουμε τον parser απο τον πίνακα των parers:*/
                var msg = m_messages[(int)messageType];
                if (msg == null)
                {
                    return null;
                }

                return msg.ParseFixMessage(fixMessage, theLogger);
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }
        }
    }
}
