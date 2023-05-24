using PatioFIX.Common;
using System.Threading;

namespace PatioFIX.WatchDog.Metrics
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class BaseMetrics
    {
        readonly protected System.Object m_lockObj = new System.Object();
        protected MetricDataPoint m_accumulator;
        readonly protected Logger theLogger = null;


        /// <summary>
        /// 
        /// </summary>
        protected BaseMetrics(string ownerName)
        {
            theLogger = new Logger(ownerName);
        }


        public void OnWatchDogConnected()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.WatchDog_Connected = true;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnWatchDogDisconnected()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.WatchDog_Connected = false;
                m_accumulator.TCP_IsConnected = 0;
                m_accumulator.FIXCLIENT_Started = 0;
                ResetSessionState();
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }

        public void OnSetRemoteClients(int numberOfClients)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.WatchDog_Clients = numberOfClients;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }


        void ResetSessionState()
        {
            m_accumulator.SESSIONSTATE_SentLogon = 0;
            m_accumulator.SESSIONSTATE_ReceivedLogon = 0;
            m_accumulator.SESSIONSTATE_SentLogout = 0;
            m_accumulator.SESSIONSTATE_ReceivedLogout = 0;
            m_accumulator.SESSIONSTATE_Synchronizing = 0;
            m_accumulator.SESSIONSTATE_ServerSynchronizing = 0;
            m_accumulator.SESSIONSTATE_TestRequestPending = 0;
            m_accumulator.SESSIONSTATE_ForceStopInProcess = 0;
        }


        #region TCPConnection
        public void OnTCPIncomingMessage(int numOfBytes)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_ReceivedMessages++;
                m_accumulator.TCP_ReceivedBytes += numOfBytes;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnTCPOutcomingMessage(int numOfBytes)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_SendMessages++;
                m_accumulator.TCP_SendBytes += numOfBytes;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnTCPWarning()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_Warnings++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnTCPError()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_Errors++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnTCPConnect()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_IsConnected = 1;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnTCPDisconnect()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TCP_IsConnected = 0;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        #endregion


        #region FIXClient
        public void OnFIXClientNewInstance()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_InstanceCounter++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientStarted(bool value)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                if (value == true)
                {
                    m_accumulator.FIXCLIENT_Started = 1;
                }
                else
                {
                    m_accumulator.FIXCLIENT_Started = 0;
                    /*
                     * Κανουμε reset ολο το 
                     */
                    ResetSessionState();
                }
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientInboundSeqNumTooHigh()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_InboundSeqNumTooHigh++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientThrowAwayMessage()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_ThrowAwayMessages++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientIgnoredMessage()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_IgnoredMessages++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }

        public void OnFIXClientSendResendRequest()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_ResendRequests++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientSendRejection()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_Rejections++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientState(bool sentLogon, bool receivedLogon, bool sentLogout, bool receivedLogout, bool synchronizing, bool server_Synchronizing, bool testRequestPending, bool forceStopInProcess)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.SESSIONSTATE_SentLogon = sentLogon == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_ReceivedLogon = receivedLogon == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_SentLogout = sentLogout == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_ReceivedLogout = receivedLogout == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_Synchronizing = synchronizing == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_ServerSynchronizing = server_Synchronizing == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_TestRequestPending = testRequestPending == true ? 1 : 0;
                m_accumulator.SESSIONSTATE_ForceStopInProcess = forceStopInProcess == true ? 1 : 0;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientState(int NextOutboundSeqNum, int NextInboundSeqNum)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.SESSIONSTATE_NextInboundSeqNum = NextInboundSeqNum;
                m_accumulator.SESSIONSTATE_NextOutboundSeqNum = NextOutboundSeqNum;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientSessionTimerHeartBeat()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_SessionTimerHeartbeats++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }


        public void OnFIXClientFailedLogin()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_FailedLogins++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientWarning()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_Warnings++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientError()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_Errors++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnFIXClientReject()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.FIXCLIENT_Rejects++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        #endregion


        public void OnParsingWarning()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.Parsing_Warnings++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        public void OnParsingError()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.Parsing_Errors++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }

        public void OnRejectionsWarning()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.Rejection_Warnings++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }


        /// <summary>
        /// Χρησιμοποιείται αποκλειστικά απο το Evaluator, και η κλήση της,
        /// είνια ήδη μεσα σε ένα if (Globals.EnableMonitorServer) {} block
        /// </summary>
        /// <param name="mtype"></param>
        /// <param name="elapsedTicks"></param>
        internal void OnEvaluationIime(ODLMessageTypeEnum mtype, long elapsedTicks)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                EvaluationIime(mtype, elapsedTicks);
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        void EvaluationIime(ODLMessageTypeEnum mtype, long elapsedTicks)
        {
            if (mtype == ODLMessageTypeEnum.Order_Entry_Confirmation)
            {
                m_accumulator.Order_Entry_Confirmation_count++;
                m_accumulator.Order_Entry_Confirmation_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Edit_Confirmation)
            {
                m_accumulator.Order_Edit_Confirmation_count++;
                m_accumulator.Order_Edit_Confirmation_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Change_Confirmation)
            {
                m_accumulator.Order_Change_Confirmation_count++;
                m_accumulator.Order_Change_Confirmation_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.New_Trade_Confirmation)
            {
                m_accumulator.New_Trade_Confirmation_count++;
                m_accumulator.New_Trade_Confirmation_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Rejection)
            {
                m_accumulator.Rejection_count++;
                m_accumulator.Rejection_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.OrderCancelReject)
            {
                m_accumulator.OrderCancelReject_count++;
                m_accumulator.OrderCancelReject_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Credit_Limit_Information)
            {
                m_accumulator.Credit_Limit_Information_count++;
                m_accumulator.Credit_Limit_Information_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Security_Status)
            {
                m_accumulator.Security_Status_count++;
                m_accumulator.Security_Status_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Security_Price)
            {
                m_accumulator.Security_Price_count++;
                m_accumulator.Security_Price_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Market_Status)
            {
                m_accumulator.Market_Status_count++;
                m_accumulator.Market_Status_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.System_Status)
            {
                m_accumulator.System_Status_count++;
                m_accumulator.System_Status_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Exchange_Notes)
            {
                m_accumulator.Exchange_Notes_count++;
                m_accumulator.Exchange_Notes_sum += elapsedTicks;
            }
            else if (mtype == ODLMessageTypeEnum.Ignored_Message)
            {
                m_accumulator.Ignored_count++;
                m_accumulator.Ignored_sum += elapsedTicks;
            }
            else
            {

            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtype"></param>
        public void OnReceive(ODLMessageTypeEnum mtype)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                Receive(mtype);
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        void Receive(ODLMessageTypeEnum mtype)
        {
            m_accumulator.Received++;

            if (mtype == ODLMessageTypeEnum.Order_Entry_Confirmation)
            {
                m_accumulator.Order_Entry_Confirmation++;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Edit_Confirmation)
            {
                m_accumulator.Order_Edit_Confirmation++;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Change_Confirmation)
            {
                m_accumulator.Order_Change_Confirmation++;
            }
            else if (mtype == ODLMessageTypeEnum.New_Trade_Confirmation)
            {
                m_accumulator.New_Trade_Confirmation++;
            }
            else if (mtype == ODLMessageTypeEnum.Rejection)
            {
                m_accumulator.Rejection++;
            }
            else if (mtype == ODLMessageTypeEnum.OrderCancelReject)
            {
                m_accumulator.OrderCancelRejection++;
            }
            else if (mtype == ODLMessageTypeEnum.Credit_Limit_Information)
            {
                m_accumulator.Credit_Limit_Information++;
            }
            else if (mtype == ODLMessageTypeEnum.Security_Status)
            {
                m_accumulator.Security_Status++;
            }
            else if (mtype == ODLMessageTypeEnum.Security_Price)
            {
                m_accumulator.Security_Price++;
            }
            else if (mtype == ODLMessageTypeEnum.Market_Status)
            {
                m_accumulator.Market_Status++;
            }
            else if (mtype == ODLMessageTypeEnum.System_Status)
            {
                m_accumulator.System_Status++;
            }
            else if (mtype == ODLMessageTypeEnum.Exchange_Notes)
            {
                m_accumulator.Exchange_Notes++;
            }
            else
            {
                m_accumulator.Unknown++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtype"></param>
        public void OnSend(ODLMessageTypeEnum mtype)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                Send(mtype);
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        void Send(ODLMessageTypeEnum mtype)
        {
            m_accumulator.Send++;

            if (mtype == ODLMessageTypeEnum.Order_Entry)
            {
                m_accumulator.Order_Entry++;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Change)
            {
                m_accumulator.Order_Change++;
            }
            else if (mtype == ODLMessageTypeEnum.Order_Edit)
            {
                m_accumulator.Order_Edit++;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        public void OnError(string source, string type = null)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.Application_Errors++;
                if (source == "EventsListener")
                {
                    m_accumulator.Errors_EventsListener++;
                }
                else if (source == "OrdersDispatcher")
                {
                    m_accumulator.Errors_OrdersDispatcher++;
                }
                else if (source == "TheController")
                {
                    m_accumulator.Errors_TheController++;
                }
                else if (source == "GetOutboundMessages")
                {
                    m_accumulator.Errors_GetOutboundMessages++;
                }
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        public void OnWarning(string source, string type = null)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.Application_Warnings++;

                if (source == "EventsListener")
                {
                    m_accumulator.Warnings_EventsListener++;
                }
                else if (source == "OrdersDispatcher")
                {
                    m_accumulator.Warnings_OrdersDispatcher++;

                    if (type == "SENDMESSAGE_GUARD")
                    {
                        m_accumulator.Warnings_SendMessage_Guard++;
                    }
                    else if (type == "SENDMESSAGE_FAILLED")
                    {
                        m_accumulator.Warnings_SendMessage_Failled++;
                    }
                    else if (type == "SENDMESSAGE_RETRY")
                    {
                        m_accumulator.Warnings_SendMessage_Retries++;
                    }
                    else if (type == "SENDMESSAGE_EXPIRED")
                    {
                        m_accumulator.Warnings_SendMessage_Expired++;
                    }
                    else if (type == "SetOutBoundMsg_Timeout")
                    {
                        m_accumulator.Warnings_SetOutBound_Timeout++;
                    }
                    else if (type == "SetOutBoundMsg_Deadlock")
                    {
                        m_accumulator.Warnings_SetOutBound_Deadlock++;
                    }
                    else if (type == "RESTORE_FAILLED")
                    {
                        m_accumulator.Warnings_Restore_failed++;
                    }
                    else if (type == "UnSetOutBoundMsg_Timeout")
                    {
                        m_accumulator.Warnings_UnSetOutBound_Timeout++;
                    }
                    else if (type == "UnSetOutBoundMsg_Deadlock")
                    {
                        m_accumulator.Warnings_UnSetOutBound_Deadlock++;
                    }
                }
                else if (source == "TheController")
                {
                    m_accumulator.Warnings_TheController++;
                }
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnControllerHeartBeat()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.TheControllerHeartBeats++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnEventsListenerHeartBeat()
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.EventsListenerHeartBeats++;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }

        public void UnConfirmedPoolMessages(int value)
        {
            Monitor.Enter(m_lockObj);
            try
            {
                m_accumulator.UnConfirmedPool_NumOfMessages = value;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }



        public abstract MetricDataPoint ReadAccumulator();


        public virtual void ResetMetrics()
        {
            theLogger.Info("ResetMetrics()");

            Monitor.Enter(m_lockObj);
            try
            {
                /*
                 * δεν θελουμε να χασουμε τις τιμες WatchDog_Connected, WatchDog_Clients
                 */
                bool _connected = this.m_accumulator.WatchDog_Connected;
                int _clients = this.m_accumulator.WatchDog_Clients;

                m_accumulator = new MetricDataPoint();

                m_accumulator.WatchDog_Connected = _connected;
                m_accumulator.WatchDog_Clients = _clients;
            }
            finally
            {
                Monitor.Exit(m_lockObj);
            }
        }



    }
}
