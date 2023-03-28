using PatioFIX.Common.FixSupport.Transport;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class FixClient : IFixClient
    {
        bool _disposedValue;

        #region local state
        readonly Logger theLogger = new Logger("FixClient");
        readonly FixConfiguration m_settings;
        readonly SessionId m_sessionId;
        readonly TCPConnection m_connector;
        readonly MyCallbackTimer _timer;
        readonly SessionState m_state;
        readonly FIXMessageWriter m_outbound;
        readonly IClock _clock = new RealTimeClock();
        readonly ManualResetEvent _receiveLogoutEvent = new ManualResetEvent(false);
        readonly object _clientLock = new object();
        bool m_isStarted = false;
        DateTime m_lastFixStopDT = DateTime.MinValue;
        #endregion


        public event Action<FIXMessage, ODLMesssageSource> NewMessageEvent;
        public event Action DisconnectEvent;
        public event Action<int, int> NewThrottlingPolicy;



        /// <summary>
        /// Μας λεει εαν υπαρχει η συνδεση στο φυσικο επιπεδο (TCP Socket)
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (m_connector == null)
                {
                    return false;
                }
                if (m_connector.IsConnected == false)
                {
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Μας λεει εαν μπορουμε να στείλουμε στον FIX Server πακέτα
        /// </summary>
        /// <param name="targetConnection"></param>
        /// <returns></returns>
        public bool CanSend(string targetConnection)
        {
            if (m_connector == null)
            {
                return false;
            }
            if (m_connector.IsConnected == false)
            {
                return false;
            }
            if (m_state.IsLoggedOn == false)
            {
                return false;
            }
            if (m_state.Synchronizing_Pending == true || m_state.Synchronizing == true)
            {
                return false;
            }
            if (m_state.Server_Synchronizing == true)
            {
                return false;
            }
            if (m_state.ForceStopInProcess == true)
            {
                return false;
            }

            return true;
        }

        public bool IsStarted
        {
            get
            {
                lock (_clientLock)
                {
                    return m_isStarted;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public DateTime LastFixStopDT => m_lastFixStopDT;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="sessionsRootPath"></param>
        public FixClient(FixConfiguration settings, string sessionsRootPath)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(sessionsRootPath)) throw new ArgumentNullException(nameof(sessionsRootPath));

            /*
             * Θελουμε η περιοδος του _timer μας (SessionTimerInterval) να μην ειναι
             * μικροτερη απο 2 δευτερολεπτα, για να εχουν ικανο χρονικο διαστημα μεταξύ
             * τους τα διαφορα tasks που εκτελει
             */
            if (settings.SessionTimerInterval < 2000)
            {
                throw new PtFixException($"SessionTimerInterval cannot be smaller than 2000 ms!");
            }
            m_settings = settings;
            m_sessionId = new SessionId(settings.SenderCompID, settings.TargetCompID);


            theLogger.Info($".ctor, session = '{m_sessionId.Id}'");


            m_connector = new TCPConnection(m_settings);
            m_connector.OnDisconnect += OnDisconnect;
            m_connector.OnNewMessage += OnNewMessage;

            _timer = new MyCallbackTimer(m_settings.SessionTimerInterval, OnSessionTimer, "SessionTimer");


            m_state = new SessionState(m_connector, settings, sessionsRootPath, m_sessionId);

            m_outbound = new FIXMessageWriter(m_settings.MaxMessageLength);

            MetricsProxy.Instance.OnFIXClientNewInstance();
        }


        public void Dispose()
        {
            if (!_disposedValue)
            {
                theLogger?.Info("Dispose()");

                ForceStop();

                m_connector.Dispose();

                m_state.Dispose();


                _disposedValue = true;
            }
        }

        void ResetInnerState()
        {
            _receiveLogoutEvent.Reset();
            _timer.Reset();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStatus"></param>
        /// <exception cref="Exception"></exception>
        public void Start(ClientStatus clientStatus)
        {
            if (_disposedValue)
            {
                MetricsProxy.Instance.OnFIXClientError();
                throw new Exception("FixClient instance is disposed");
            }

            if (m_isStarted == false)
            {
                lock (_clientLock)  //Start, Stop, OnSessionTimer, _forceStopImlementation
                {
                    if (m_isStarted == false)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
                        else
                            theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'.");

                        ResetInnerState();


                        if (m_state.CreationTime.DayOfYear != clientStatus.DayOfYear)
                        {
                            theLogger.Warning($"FIXState is old (m_state.CreationTime.DayOfYear={m_state.CreationTime.DayOfYear} != clientStatus.DayOfYear={clientStatus.DayOfYear})");
                            /*
                             * Το SessionState δεν ειναι της σημερνής ημέρας.
                             */
                            this.m_state.ResetMessageStore();
                        }

                        /*
                         * Στο clientStatus εχουμε το πραγματικο inboundSequenceNumber που εχουμε επεξεργαστει.
                         * Αποθηκευεται συνεχως στον πίνακα PatioFIXClients_State
                         */
                        if (clientStatus.DisableDataLayer == false)
                        {
                            var _nextInboundSeqNum = clientStatus.ETS_LastMsgSeqNum + 1;
                            if (m_state.NextInboundSeqNum != _nextInboundSeqNum)
                            {
                                m_state.ShowInfo();
                                theLogger.Info($"ClientStatus says NextInboundSeqNum = {_nextInboundSeqNum}, but m_state.NextInboundSeqNum = {m_state.NextInboundSeqNum}!");

                                m_state.NextInboundSeqNum = _nextInboundSeqNum;
                                theLogger.Info($"m_state.NextInboundSeqNum changed to {m_state.NextInboundSeqNum}!!");
                            }
                        }
                        else
                        {
                            theLogger.Info($"ClientStatus says DisableDataLayer = true, so m_state.NextInboundSeqNum is from our cache!");
                        }


                        //Ξεκιναμε τον timer
                        _timer.AuthoritativeStart(true);

                        m_isStarted = true;
                        MetricsProxy.Instance.OnFIXClientStarted(true);
                    }
                }
            }
            theLogger.Verbose("Start() leaving...");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Stop()
        {
            if (_disposedValue)
            {
                MetricsProxy.Instance.OnFIXClientError();
                throw new Exception("FixClient instance is disposed");
            }

            if (m_isStarted == true)
            {
                lock (_clientLock)  //Start, Stop, OnSessionTimer, _forceStopImlementation
                {
                    if (m_isStarted == true)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
                        else
                            theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'.");


                        //Σταματαμε τον timer....
                        _timer.AuthoritativeStop();

                        //Στελνουμε logout, και περιμενουμε 5 sec maximum
                        if (m_connector.IsConnected)
                        {
                            if (InitiateLogout())
                            {
                                /*
                                 * We wait twice the HeartBtInt(108) fro a logout (35=5) acknowledgement
                                 * before terminating the trapsnport layer connection
                                 */
                                if (_receiveLogoutEvent.WaitOne((int)(m_state.HeartbeatInterval * 2)) == false)
                                {
                                    theLogger.Warning("Our Logout did not acknowledged from remote peer!");
                                }
                            }
                        }

                        m_connector.Stop();
                        m_state.ResetState();

                        m_isStarted = false;
                        m_lastFixStopDT = DateTime.Now;
                        MetricsProxy.Instance.OnFIXClientStarted(false);
                    }
                }
            }
            theLogger.Verbose("Stop() leaving...");
        }

        /// <summary>
        /// 
        /// </summary>
        void ForceStop(bool switchThread = false, bool fromTCPConnection = false)
        {
            if (_disposedValue)
            {
                MetricsProxy.Instance.OnFIXClientError();
                throw new Exception("FixClient instance is disposed");
            }

            m_state.ForceStopInProcess = true;

            if (switchThread)
            {
                /*
                 * ειμαστε μεσα στο νημα του TCPConnector, και θελουμε να το ελευθερωσουμε...
                 */
                ThreadPool.QueueUserWorkItem((obj =>
                {
                    _ForceStopImlementation(true, fromTCPConnection);
                }));
            }
            else
            {
                _ForceStopImlementation(false, fromTCPConnection);
            }
        }

        void _ForceStopImlementation(bool switchedThread, bool fromTCPConnection)
        {
            if (m_isStarted == true)
            {
                lock (_clientLock)  //Start, Stop, OnSessionTimer, _forceStopImlementation
                {
                    if (m_isStarted == true)
                    {
                        theLogger.Info($"_ForceStopImlementation(switchedThread={switchedThread}, fromTCPConnection={fromTCPConnection})");

                        //Σταματαμε τον timer....
                        _timer.AuthoritativeStop();

                        if (m_state.SentLogout == true && m_state.ReceivedLogout == false)
                        {
                            /*
                            * Εχουμε στειλει logout (SendFatalLogout) αλλα δεν εχουμε λαβει απο την απεναντι μερια logout
                            * Αρα εχουμε ερθει εδω απο την OnNewMessage()
                            * 
                            * Βεβαιως υπαρχει η περιπτωση η αλλη μερια να εκλεισε απλα το socket και να μην στειλει ποτε logout. 
                            * Σε αυτη την περιπτωση ο TCPConnector θα καλεσει το δικο μας OnDisconnect και θα καλεσει ξανα την 
                            * ForceStop σε νεο νημα. Δεν μας ενοχλει αυτο και το αφηνουμε ως εχει...
                            */
                            theLogger.Info("There is a pending remote logout...");
                            _receiveLogoutEvent.WaitOne(400);
                        }

                        if (fromTCPConnection == false)
                        {
                            m_connector.Stop();
                        }

                        m_state.ResetState();

                        m_isStarted = false;
                        m_lastFixStopDT = DateTime.Now;
                        MetricsProxy.Instance.OnFIXClientStarted(false);
                    }
                }
            }
        }


        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
        /// O TCPConnector αποσυνδεθηκε, χωρις να το ζητησουμε εμεις
        /// </summary>
        /// <param name="fatalException"></param>
        void OnDisconnect(int errorCode)
        {
            ///!!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
            theLogger.Info($"OnDisconnect( errorCode = {errorCode} )...");

            ForceStop(switchThread: true, fromTCPConnection: true);

            DisconnectEvent?.Invoke();
        }


        /// <summary>
        /// Ελεγχουμε το εισερχομενο SequenceNumber εαν ειναι το αναμενομενο
        /// </summary>
        /// <param name="_msgSeqNum"></param>
        /// <returns></returns>
        /// <exception cref="PtFixFatalException"></exception>
        bool _IsInboundSeqNumExpected(long _msgSeqNum)
        {

            if (_msgSeqNum < m_state.NextInboundSeqNum)
            {
                /*
                 * If is less than NextInboundSeqNum, then a Logout(35=5) message should be sent assuming that an error exists in the 
                 * state of either the initiator or acceptor FIX session processor, followed by a termination of the transport layer connection
                 */
                var reason = $"InboundSeqNum (34={_msgSeqNum}) too low, expecting {m_state.NextInboundSeqNum} but received {_msgSeqNum}";
                throw new PtFixFatalException(reason);
            }
            else if (_msgSeqNum > m_state.NextInboundSeqNum)
            {
                /*
                 * message recovery must be performed.
                 */
                theLogger.Warning($"InboundSeqNum  (34={_msgSeqNum}) too high, expecting {m_state.NextInboundSeqNum}, but received {_msgSeqNum}");
                m_state.Synchronizing_Pending = true;
                MetricsProxy.Instance.OnFIXClientInboundSeqNumTooHigh();
                return false;
            }


            m_state.NextInboundSeqNum++;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_msgSeqNum"></param>
        /// <param name="callee"></param>
        void _UpdateSynchronizingStatus(int _msgSeqNum, int callee)
        {
            if (m_state.Synchronizing == true)
            {
                if (m_state.Synchronizing_to_inboundSeqNum <= _msgSeqNum)
                {
                    theLogger.Info($"SendResendRequest::(Synchronizing FINISHED.1----> FROM {m_state.Synchronizing_from_inboundSeqNum} to {m_state.Synchronizing_to_inboundSeqNum}), (_msgSeqNum={_msgSeqNum}, callee={callee})");
                    m_state.Synchronizing = false;
                    m_state.Synchronizing_Pending = false;
                }
            }
        }
        /// <summary>
        /// Μας λεει εαν αυτο το inbound (εισερχομενο) message ειναι 
        /// ενα session level μηνυμα
        /// </summary>
        /// <param name="inbound"></param>
        /// <returns></returns>
        bool _IsSessionMessage(FIXMessage inbound)
        {
            var msgType = inbound.MsgType[0];
            if (msgType == '0') //Heartbeat (0)
                return true;
            if (msgType == '1') //Test Request (1)
                return true;
            if (msgType == '2') //Resend Request (2)
                return true;
            if (msgType == '3') //Reject (3)
                return true;
            if (msgType == '4') //Sequence Reset (4)
                return true;
            if (msgType == '5') //Logout (5)
                return true;
            if (msgType == 'A' && inbound.MsgType.Length == 1) //Logon (A)
                return true;

            return false;
        }

        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
        /// 1). Πρεπει να επιστρεψουμε αμεσα, οσο ειμαστε εδω δεν λαμβανουμε νεα incoming FIX Messages
        /// 2). To inbound FIXMessage, δεν ειναι δικο μας, πρεπει να το αντιγραψουμε εαν το βαλουμε στο QUEUE
        /// </summary>
        /// <param name="inbound"></param>
        void OnNewMessage(FIXMessage inbound)
        {
            ///!!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
            try
            {
                if (m_state.ForceStopInProcess == true)
                {
                    theLogger.Warning($"OnNewMessage::ForceStopInProcess::THROW_AWAY_MESSAGE {inbound}");
                    m_state.ShowInfo();
                    MetricsProxy.Instance.OnFIXClientThrowAwayMessage();
                    return;
                }


                /*
                 * Το MsgSeqNum ειναι REQUIRED TAG, αλλα δεν το ελεγχουμε exlicitly. 
                 * Εαν δεν υπαρχει θα εχει την τιμη -1, και θα ρίξουμε
                 * ένα PtFixFatalException ("InboundSeqNum (34=0) too low, expecting...")
                 */
                var _msgSeqNum = inbound.MsgSeqNum;

                if (m_settings.LogInboundMessages)
                {
                    #region  Αποθηκευουμε το μηνυμα που δεχτηκαμε στο IMessageStore
                    m_state.SaveInbound(_msgSeqNum, inbound.m_rawBytes, inbound.Length);
                    #endregion
                }

                //Logging:
                //theLogger.Info($"{m_sessionId.Id}.in [<] - {inbound}");
                theLogger.Info($"in [<] - {inbound}");

                /*
                 * Ελεγχουμε το BeginString <8> field να ειναι ιδιο με το m_configuration.Version
                 */
                if (!inbound[8].Is(m_settings.Version))
                    throw new Exception($"Unexpected BeginString: {inbound}");


                if (!inbound.ContainsTag52)
                    throw new PtFixRejectException(_msgSeqNum, "Required tag SendingTime<52> is missing");
                if (!inbound.ContainsTag49)
                    throw new PtFixRejectException(_msgSeqNum, "Required tag SenderCompID<49> is missing");
                if (!inbound.ContainsTag56)
                    throw new PtFixRejectException(_msgSeqNum, "Required tag TargetCompID<56> is missing");

                /*
                 * To SenderCompID <49> field, του Server που μας στελνει το μηνυμα, πρεπει να ειναι ιδιο με το δικο μας TargetCompID
                 */
                if (!inbound[49].Is(m_settings.TargetCompID))
                    throw new PtFixFatalProtocolException($"Rejected communication with {inbound[56].AsString}_{inbound[49].AsString}. Unexpected TargetCompID: {inbound[49].AsString}.");
                /*
                 * Το TargetCompID <56> field, του Server που μας στελνει το μηνυμα, πρεπει να ειναι ιδιο με το δικο μας SenderCompID
                 */
                if (!inbound[56].Is(m_settings.SenderCompID))
                    throw new PtFixFatalProtocolException($"Rejected communication with {inbound[56].AsString}_{inbound[49].AsString}. Unexpected SenderCompID: {inbound[56].AsString}.");
                /*
                 * ειμαστε initiators. Για να λαβουμε καποιο μηνυμα πρεπει εμεις να εχουμε στειλει το πρωτο μήνυμα
                 */
                if (m_state.SentLogon == false && m_state.ReceivedLogon == false)
                {
                    throw new PtFixFatalProtocolException($"Rejected communication with {inbound[56].AsString}_{inbound[49].AsString}. Received message before Logon.");
                }

                //Ανανεωνουμε το InboundTimestamp αφου μολις λαβαμε νεο innbound message:
                m_state.InboundTimestamp = _clock.Time;



                /*
                 * Ελεγχουμε το MsgSeqNum του μηνυματος που μας ηρθε (και ταυτοχρονα αυξανουμε το NextInboundSeqNum):
                 * -Εαν ειναι μικροτερο απο το αναμενομενο, τοτε θα παρουμε ενα PtFixFatalException
                 * -Εαν ειναι μεγαλυτερο θα κανουμε message-recovery
                 * -To συνηθισμενο ειναι να ειναι το αναμενομενο
                 */
                var _isInSeqNumOK = _IsInboundSeqNumExpected(_msgSeqNum);//message recovery must be performed??


                /*
                 * Processing inbound possible duplicate messages (PossDup(43) set to “Y”)
                 * 
                 * A FIX session processor, upon receipt of an inadvertently (or improperly) retransmitted session layer 
                 * message as identified by the PossDupFlag(43) set to “Y”, should perform sequence number processing 
                 * (increment NextNumIn) only and avoid processing the session layer message.
                 */
                if (inbound.PossDupFlag == true)
                {
                    if (_IsSessionMessage(inbound))
                    {
                        /*
                         * Εαν ειμαστε σε κατασταση Synchronizing τα 'Sequence Reset (4)' τα αφηνουμε να περασουν...
                         */
                        if (m_state.Synchronizing == false || inbound.MsgType[0] != '4')
                        {
                            theLogger.Warning("OnNewMessage:: Inbound Message IGNORED (Has PossDupFlag == 'Y', and is a sesion level message)");
                            MetricsProxy.Instance.OnFIXClientIgnoredMessage();
                            return;
                        }
                    }
                }


                if (m_state.SentLogon == true && m_state.ReceivedLogon == false)
                {
                    _onNewMessage_CompleteLogon(inbound, _isInSeqNumOK, _msgSeqNum);
                    MetricsProxy.Instance.OnFIXClientState(m_state);
                }
                else
                {
                    if (_onNewMesssage_OtherMessages(inbound, _isInSeqNumOK, _msgSeqNum))
                    {
                        MetricsProxy.Instance.OnFIXClientState(m_state);
                    }
                }


            }
            catch (PtFixNonFatalException ex)
            {
                MetricsProxy.Instance.OnFIXClientWarning();
                theLogger.Warning($"OnNewMessage::(NonFatalException): {ex.Message}, {inbound}");
            }
            catch (PtFixRejectException ex)
            {
                MetricsProxy.Instance.OnFIXClientWarning();
                theLogger.Warning($"OnNewMessage::(RejectException): {ex.RejectText}, {inbound}");
                SendReject(ex);
            }
            catch (PtFixFatalException ex)
            {
                MetricsProxy.Instance.OnFIXClientError();
                theLogger.Error($"OnNewMessage::(FatalException): {ex.Message}, {inbound}");

                SendFatalLogout(ex.Message);
                ForceStop(switchThread: true);
            }
            catch (PtFixFatalProtocolException ex)
            {
                MetricsProxy.Instance.OnFIXClientError();
                theLogger.Error($"OnNewMessage::(FatalProtocolException): {ex.Message}, {inbound}");

                ForceStop(switchThread: true);
            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnFIXClientError();
                theLogger.Error($"OnNewMessage::(Exception): {ex.Message}, {inbound}");

                SendFatalLogout(ex.Message);
                ForceStop(switchThread: true);
            }
        }
        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
        /// </summary>
        /// <param name="inbound"></param>
        /// <param name="_isInSeqNumOK"></param>
        /// <param name="_msgSeqNum"></param>
        /// <exception cref="Exception"></exception>
        void _onNewMessage_CompleteLogon(FIXMessage inbound, bool _isInSeqNumOK, int _msgSeqNum)
        {
            var _msgType = inbound.MsgType;

            /*
             * Περιμενουμε το πρωτο μηνυμα μετα το InitiateLogon
             * Μπορει να ειναι ένα logon 'A' ή ένα Logout '5'
             * Και τα δυο εχουν μηκος ενα χαρακτηρα
             */
            #region HandleLogon
            if (_msgType.Length != 1)
            {
                throw new Exception("Unexpected first message received #1 (expected logon/logout)");
            }

            char _mtc = _msgType[0];
            if (_mtc != /*Logon*/'A' && _mtc != /*Logout*/'5')
            {
                throw new Exception("Unexpected first message received #2 (expected logon/logout)");
            }


            if (_mtc == 'A')
            {
                //Logon
                if (!inbound[Tags.HeartBtInt].Is(m_settings.HeartbeatInterval)) throw new Exception("Unexpected heartbeat interval received");
                if (!inbound[Tags.EncryptMethod].Is(0)) throw new Exception("Unexpected encryption method received");
                if (inbound.Contains(Tags.ResetSeqNumFlag))
                {
                    if (inbound[Tags.ResetSeqNumFlag].Is("Y")) throw new Exception("Unexpected ResetSeqNumFlag on logon received");
                }

                m_state.ReceivedLogon = true;                       //ηρθε το login
                theLogger.Info($"{m_sessionId.Id} logged on successfully");

                if (_isInSeqNumOK == false)
                {
                    SendResendRequest(_msgSeqNum);    //start synchronizing....
                }

                m_state.ShowInfo();
            }
            else if (_mtc == '5')
            {
                //Logout
                bool _forceStop = true;
                string reason = string.Empty;
                if (inbound.Contains(Tags.Text))
                {
                    reason = inbound[Tags.Text].AsString;
                }
                MetricsProxy.Instance.OnFIXClientFailedLogin();
                theLogger.Error($"Logon denied by FIX Server. Reason = {reason}");

                //Τωρα ελεγχουμε μηπως χρειαζεται να αλλαξουμε το δικο μας NextOutboundSeqNum
                if (reason.Contains("MsgSeqNum too low"))
                {
                    var idx1 = reason.IndexOf("expecting");
                    var idx2 = reason.IndexOf("but received");

                    try
                    {
                        var _nextOutboundSeqNum = Int32.Parse(reason.Substring(idx1 + 10, idx2 - idx1 - 10));

                        theLogger.Info($"Fix Server says NextOutboundSeqNum must be {_nextOutboundSeqNum}");
                        if (_nextOutboundSeqNum <= m_state.NextOutboundSeqNum)
                        {
                            MetricsProxy.Instance.OnFIXClientWarning();
                            theLogger.Warning($"Unaccepted NextOutboundSeqNum, Currently={m_state.NextOutboundSeqNum}, bigger or eaqual to {_nextOutboundSeqNum}");
                        }
                        else
                        {
                            m_state.NextOutboundSeqNum = _nextOutboundSeqNum;
                            m_state.SentLogon = false;
                            _forceStop = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MetricsProxy.Instance.OnFIXClientError();
                        theLogger.Error($"Exception occured during NextOutboundSeqNum calibration..., message={ex.Message}");
                        theLogger.Error(ex);
                    }
                }

                if (_forceStop)
                {
                    //Τωρα σταματαμε τον FixClient
                    ForceStop(switchThread: true);
                }
            }
            #endregion

        }
        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop!!!!!!!!!
        /// </summary>
        /// <param name="inbound"></param>
        /// <param name="_isInSeqNumOK"></param>
        /// <param name="_msgSeqNum"></param>
        bool _onNewMesssage_OtherMessages(FIXMessage inbound, bool _isInSeqNumOK, int _msgSeqNum)
        {
            /*
             * Τo MsgType των 'Administration Messages' εχει μηκος ακριβως ενα (1) char και δεν πρεπει να μπερδευτουμε με MsgType(s) που ξεκινανε απο ιδιους χαρακτηρες.
             * Για παραδειγμα για το MsgType 'A' υπαρχει και MsgTypes σαν 'AA','AB','AI' και παει λεγοντας.
             * Εαν κοιταξουμε τυφλα μονο το πρωτο char (inbound.MsgType[0]) χωρις ταυτοχρονα να ελεγχουμε και το length του MsgType, θα την πατησουμε οπως και 
             * συνεβηκε στις δοκιμες.
             * 
             * Ετσι στο _msgTypeFirstChar θα βαλουμε τον πρωτο χαρακτηρα του MsgType του μηνυματος που λαβαμε μονο οταν εχει (το MsgType) length 1. Ειδαλλως
             * αφηνουμε το χαρακτηρα '@' που δεν αντιστοιχει σε καποιο τυπο 'Administration Message' και ετσι θα συνεχισει την πορεια του σαν 'Application Message'
             * 
             * Προσοχη, αυτο δεν σημαινει οιι δεν υπαρχουν 'Application Messages' με μηκος MsgType μονο ενα (1) χαρακτηρα. Για αυτο το λογο πρωτα ψαχνουμε μην τυχων
             * εχουμε λαβει 'Administration Message' και μετα το χειριζομαστε σαν 'Application Message'
             */
            char _msgTypeFirstChar = inbound.MsgType.Length != 1 ? '@' : inbound.MsgType[0];


            if (_isInSeqNumOK == false)
            {
                //1. Εαν δεν εχουμε στειλει, ηδη "Resend Request (MsgType = 2)" το κανουμε τωρα
                SendResendRequest(_msgSeqNum);    //start synchronizing....


                if (_msgTypeFirstChar == '2')
                {
                    //2a. Κοιταμε εαν το μηνυμα που παραλαβαμε μήπως είναι ενα "Resend Request (MsgType = 2)" απο την απεναντι πλευρα
                    HandleResendRequest(inbound);
                }
                else
                {
                    //2b. Τα υπολοιπα μηνυματα το πεταμε (εαν ειναι application Message θα το ξαναλαβουμε
                    theLogger.Warning($"OnNewMessage::THROW_AWAY_MESSAGE {inbound}");
                    m_state.ShowInfo();
                    MetricsProxy.Instance.OnFIXClientThrowAwayMessage();
                }

                return true;
            }

            #region Handle Admin & Session Messages...
            _UpdateSynchronizingStatus(_msgSeqNum, callee: 1);
            m_state.TestRequestPending = false;



            if (_msgTypeFirstChar == '0')
            {
                HandleHeartbeat(inbound);
            }
            else if (_msgTypeFirstChar == '1')
            {
                HandleTestRequest(inbound);
                m_state.ShowInfo();
            }
            else if (_msgTypeFirstChar == '2')
            {
                HandleResendRequest(inbound);
                m_state.ShowInfo();
            }
            else if (_msgTypeFirstChar == '3')
            {
                HandleRejectReqest(inbound);
                m_state.ShowInfo();
            }
            else if (_msgTypeFirstChar == '4')
            {
                HandleSequenceReset(inbound);
                m_state.ShowInfo();
            }
            else if (_msgTypeFirstChar == '5')
            {
                HandleLogout(inbound);
                m_state.ShowInfo();
            }
            else if (_msgTypeFirstChar == 'A')
            {
                HandleLogon(inbound);
            }
            else
            {
                /*
                 * Application Message
                 * We construct a new copy of inbound (because inbound is not ours and is 
                 * always the same, see TCPConnection)
                 */
                var _message = new FIXMessage(inbound);

                /*
                 * 
                 */
                NewMessageEvent?.Invoke(_message, m_settings.ClientRole);

                /*
                 * Εαν το μηνυμα ειναι ενα MessageNote με ThrottlingParameters,
                 * τοτε το κοινοποιούμε σε τυχων ενδιαφερόμενους....
                 */
                if (inbound.IsThrottlingParameters)
                {
                    NewThrottlingPolicy?.Invoke(inbound.TransPerSecond, inbound.OutstandingMsgs);
                }

                return false;
            }

            #endregion

            return true;
        }




        /// <summary>
        /// Εδω ειναι η 'καρδιά' του FIXClient. Αυτη ειναι η μεθοδος που εκτελει ο _timer (SessionTimer) μας
        /// Σε αυτη την μεθοδο γινονται τα εξης
        ///     -Εαν δεν εχουμε ηδη συνδεθει με το remote peer τοτε προσπαθουμε να συνδεθούμε
        ///     -InitiateLogon
        ///     -Ελεγχος εαν λαβαμε Logon απο το remote peer
        ///     -Αποστολη Heartbeat/TestRequest
        ///     
        /// H μεθοδος εκτελειται περιοδικα απο ένα νήμα του ThreadPool
        /// </summary>
        /// <param name="state"></param>
        void OnSessionTimer(Object state)
        {
            try
            {
                #region Guard (just in case....)
                if (m_isStarted == false)
                {
                    /*Αυτο κανονικα δεν θα συμβει ποτε. Οι μεδθοδοι Start() & Stop() φροντιζουν να ανοιγοκλεινουν τον timer μας*/
                    MetricsProxy.Instance.OnFIXClientError();
                    theLogger.Error("OnSessionTimer() called, but m_isStarted == false");
                    return;
                }
                #endregion

                if (m_state.ForceStopInProcess == true)
                {
                    /*Εχουμε μπει σε φαση ForceStop, και δεν προλαβαν να μας σταματησουν...*/
                    return;
                }



                if (_timer.NumberOfHeartBeats % 20 == 0)
                {
                    m_state.ShowInfo();
                }
                MetricsProxy.Instance.OnFIXClientSessionTimerHeartBeat();


                //Εαν δεν ειμαστε συνδεδεμενοι, συνδεομαστε
                if (m_connector.IsConnected == false)
                {
                    if (_clock.Time.Subtract(m_state.InboundTimestamp).TotalMilliseconds >= m_settings.TCPReconnectInterval)
                    {
                        lock (_clientLock)  //Start, Stop, OnSessionTimer
                        {
                            if (m_connector.IsConnected == false)
                            {
                                m_state.ShowInfo();
                                m_state.CountOfConnections++;
                                m_state.LastConnectionTime = _clock.Time;
                                if (m_connector.Connect() == false)
                                {
                                    m_state.CountOfFailedConnections++;
                                    return;
                                }
                                else
                                {
                                    m_state.ShowInfo();
                                }
                            }
                        }
                    }

                    return;//και να συνδεθουμε, δεν στελνουμε αμεσως το Logon, για να εχει ελαχιστο χρονο να ξεκινησει το TCPConnection._ReceiveLoop() 
                }



                //Στελνουμε το Logon
                if (m_state.SentLogon == false)
                {
                    InitiateLogon();
                    return;
                }


                //πήραμε το Logon??
                if (m_state.ReceivedLogon == false)
                {
                    if (_clock.Time - m_state.OutboundTimestamp > TimeSpan.FromSeconds(10))
                    {
                        throw new PtFixNotReceivedLogon("Logon response not received on time");
                    }
                    return;
                }


                //Heartbeat
                if (m_state.HeartbeatInterval > 0)
                {
                    if ((_clock.Time - m_state.OutboundTimestamp).TotalMilliseconds > m_state.HeartbeatInterval)
                    {
                        SendHeartbeat();
                    }

                    var lastInboundInterval = (_clock.Time - m_state.InboundTimestamp).TotalMilliseconds;
                    if (lastInboundInterval > m_state.HeartbeatTimeoutMin)
                    {
                        if (lastInboundInterval > m_state.HeartbeatTimeoutMax)
                        {
                            throw new Exception($"Did not receive any messages for too long (lastInboundInterval={lastInboundInterval} > HeartbeatTimeoutMax={m_state.HeartbeatTimeoutMax} )");
                        }

                        if (!m_state.TestRequestPending) SendTestRequest();
                    }
                }

            }
            catch (PtFixNotReceivedLogon ex)
            {
                MetricsProxy.Instance.OnFIXClientFailedLogin();
                theLogger.Error(ex.Message);

                ForceStop();
            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnFIXClientError();
                theLogger.Error(ex.Message);

                ForceStop();
            }
            finally
            {
                _timer.Start();
            }
        }





        #region Session Senders
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InitiateLogon()
        {
            theLogger.Verbose("InitiateLogon");


            m_outbound
                .Clear()
                .Set(Tags.HeartBtInt, m_settings.HeartbeatInterval)     //HeartBtInt
                .Set(Tags.EncryptMethod, 0)                             //EncryptMethod
                .Set(Tags.ResetSeqNumFlag, "N");                         //ResetSeqNumFlag


            if (!string.IsNullOrEmpty(m_settings.Username))
            {
                m_outbound.Set(Tags.Username, m_settings.Username);
            }
            if (!string.IsNullOrEmpty(m_settings.Password))
            {
                m_outbound.Set(Tags.Password, m_settings.Password);
            }

            //
            //m_outbound.Set(Tags.NewPassword, "ebF1xUs#R2");

            try
            {
                m_state.SentLogon = true;
                Send("A", m_outbound);              //MsgType : Logon (A)
            }
            catch
            {
                m_state.SentLogon = false;
                throw;
            }
        }

        /// <summary>
        /// The Heartbeat (0) monitors the status of the communication link and identifies 
        /// when the last of a string of messages was not received.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SendHeartbeat()
        {
            //theLogger.Verbose("SendHeartbeat");

            m_outbound.Clear();
            Send("0", m_outbound);
        }

        /// <summary>
        /// The Test Request (1) message forces a heartbeat from the opposing applicatio
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SendTestRequest()
        {
            theLogger.Verbose("SendTestRequest");

            m_outbound.Clear().Set(112, _clock.Time.Ticks);
            Send("1", m_outbound);

            m_state.TestRequestPending = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inboundSeqNum">Αυτο ειναι το Sequence Number του μηνυματος που μολις παραλαβαμε</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool SendResendRequest(int inboundSeqNum)
        {
            if (m_state.Synchronizing)
            {
                theLogger.Info("SendResendRequest() called but we have already send a 'Resend Request'");
                return false;
            }


            theLogger.Info($"SendResendRequest::(Synchronizing START----> FROM {m_state.NextInboundSeqNum} to {inboundSeqNum})");

            //Resend Request (MsgType = 2)
            m_outbound.Clear()
                .Set(7, m_state.NextInboundSeqNum)      //BeginSeqNo  - Message sequence number of first message in range to be resent
                .Set(16, 0);                            //EndSeqNo  - Message sequence number of last message in range to be resent, 0 ->all messages subsequent to a particular message
            Send("2", m_outbound);

            m_state.Synchronizing = true;
            m_state.Synchronizing_from_inboundSeqNum = m_state.NextInboundSeqNum;
            m_state.Synchronizing_to_inboundSeqNum = inboundSeqNum;
            m_state.Synchronizing_Pending = false;
            m_state.Synchronizing_StartTime = _clock.Time;

            MetricsProxy.Instance.OnFIXClientSendResendRequest();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        void SendReject(PtFixRejectException ex)
        {
            m_outbound.Clear();
            //MsgSeqNum <34> of rejected message 
            m_outbound.Set(Tags.RefSeqNum, ex.RefSeqNum);
            //Where possible, message to explain reason for rejection
            if (string.IsNullOrEmpty(ex.RejectText) == false)
            {
                m_outbound.Set(Tags.Text, ex.RejectText);
            }
            //The MsgType <35> of the FIX message being referenced. 
            if (string.IsNullOrEmpty(ex.RefMsgType) == false)
            {
                m_outbound.Set(Tags.RefMsgType, ex.RefMsgType);
            }
            if (ex.RejectReason != SessionRejectReason.NotSet)
            {
                m_outbound.Set(Tags.SessionRejectReason, (int)ex.RejectReason);
            }


            Send("3", m_outbound);


            MetricsProxy.Instance.OnFIXClientSendRejection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        void SendFatalLogout(string reason)
        {
            theLogger.Verbose($"SendFatalLogout (reason = '{reason}')");

            m_outbound
                .Clear()
                .Set(Tags.Text, reason);
            Send("5", m_outbound);
            m_state.SentLogout = true;
        }

        /// <summary>
        /// 
        /// </summary>
        bool InitiateLogout()
        {
            if (!m_state.IsLoggedOn)
            {
                theLogger.Info("InitiateLogout() called but IS NOT LoggedOn ...");
                return false;
            }

            theLogger.Info("InitiateLogout");

            // Send a logout message
            Send("5", m_outbound.Clear());
            m_state.SentLogout = true;
            return true;
        }
        #endregion



        #region Session Handlers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleHeartbeat(FIXMessage inbound)
        {
            //theLogger.Verbose("HandleHeartbeat");

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleTestRequest(FIXMessage inbound)
        {
            // Prepare and send a heartbeat (with the test request id)
            m_outbound.Clear().Set(112, inbound[112].AsString);

            Send("0", m_outbound);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleRejectReqest(FIXMessage inbound)
        {
            var refSeqNum = inbound[Tags.RefSeqNum].AsLong;     //MsgSeqNum (34) of rejected message 
            var refMsgType = string.Empty;                      //The MsgType(35) of the FIX message being referenced. 
            var sessionRejectReason = -1;                       //Code to identify reason for a session-level Reject (3) message. 
            var text = string.Empty;

            if (inbound.Contains(Tags.RefMsgType))
            {
                refMsgType = inbound[Tags.RefMsgType].AsString;
            }
            if (inbound.Contains(Tags.SessionRejectReason))
            {
                sessionRejectReason = inbound[Tags.SessionRejectReason].AsInt;
            }
            if (inbound.Contains(Tags.Text))
            {
                text = inbound[Tags.Text].AsString;
            }


            MetricsProxy.Instance.OnFIXClientReject();
            theLogger.Warning($"REJECT Received for RefSeqNum = {refSeqNum}, sessionRejectReason = '{sessionRejectReason}', {text}");
        }

        /// <summary>
        /// The Resend Request (2) can be used to request 
        /// a single message, 
        /// a range of messages or 
        /// all messages subsequent to a particular message. 
        /// </summary>
        /// <param name="inbound"></param>
        void HandleResendRequest(FIXMessage inbound)
        {
            bool useGapFilling = false;
            var beginSeqNo = inbound[7].AsLong;     //Message sequence number of first message in range to be resent
            var endSeqNo = inbound[16].AsLong;      //Message sequence number of last message in range to be resent

            if (beginSeqNo == endSeqNo)
            {
                //To request a single message: BeginSeqNo (7) = EndSeqNo (16)
                theLogger.Warning($"Resend_Request received for a single message, BeginSeqNo={beginSeqNo} - EndSeqNo={endSeqNo}");
            }
            else if (endSeqNo == 0)
            {
                //To request all messages subsequent to a particular message: BeginSeqNo (7) = first message of range, EndSeqNo (16) = 0 (represents infinity) . 
                theLogger.Warning($"Resend_Request received for all messages, BeginSeqNo={beginSeqNo} - EndSeqNo={endSeqNo}");
            }
            else
            {
                //To request a range of messages: BeginSeqNo (7) = first message of range, EndSeqNo (16) = last message of range 
                theLogger.Warning($"Resend_Request received for a range of messages, BeginSeqNo={beginSeqNo} - EndSeqNo={endSeqNo}");
            }


            /*
            * Ψαχνουμε να βρουμε εαν θα στειλουμε τα messages η θα στειλουμε απλα ενα 
            */
            if (m_settings.LogOutboundMessages == false)
                useGapFilling = true;
            if (m_settings.UseAlwaysResetGapFilling == true)
                useGapFilling = true;

            if (useGapFilling == true)
            {
                /*
                    * Δεν ξαναστελνουμε messages, αλλα ζηταμε απο τον server να κανει 
                    * ενα Sequence Reset (gap filling)
                    */
                theLogger.Warning($"Sending SequenceReset (35=4), NewSeqNo={m_state.NextOutboundSeqNum}");
                m_outbound.Clear()
                    .Set(123, "Y")                          //GapFillFlag - Indicates that the Sequence Reset (4) message is replacing administrative or application messages which will not be resent. 
                    .Set(36, m_state.NextOutboundSeqNum);   //NewSeqNo  - New sequence number

                Send("4", m_outbound, beginSeqNo);          //Προσοχη για αυτο το μηνυμα το OutboundSeqNum ειναι αυτο που περιμενει ο απεναντι

                return;
            }




            m_state.Server_Synchronizing = true;
            m_state.Server_Synchronizing_StartTime = _clock.Time;
            var currSeqNo = beginSeqNo;
            var _endSeqNo = endSeqNo == 0 ? m_state.NextOutboundSeqNum : endSeqNo;

            ThreadPool.QueueUserWorkItem((x =>
            {

                try
                {
                    #region RETRANSMITTING ALL messages
                    /*
                     * Θα στειλουμε ολα τα αποθηκευμενα outbound messages απο το beginSeqNo που ζητησε ο server
                     * και οπου βρισκουμε κενα θα στελνουμε Sequence Reset (Gap Filling)
                     */
                    var source = new FIXMessage(m_settings.MaxMessageLength, m_settings.MaxMessageFields, false);

                    IList<string> messages = m_state.GetOutbound((int)beginSeqNo, (int)_endSeqNo);
                    theLogger.Info("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    theLogger.Info($"!!!!!!!!!!!!!!!!!!!!!!!! RETRANSMITTING ALL messages from SeqNum={beginSeqNo} to SeqNum={_endSeqNo}, STARTED !!!!!!!!!!!!!!!!!!!!!!!!!!!");

                    foreach (var message in messages)
                    {
                        /*
                         * Το message στο store ειναι με την μορφη string. 
                         * To μετατρεπουμε σε bytes και το κανουμε parsing για να παρουμε τα πεδία με τις τιμες τους.
                         */
                        var tbuffer = CharEncoding.DefaultEncoding.GetBytes(message);
                        source.Parse(tbuffer, 0, tbuffer.Length, theLogger);


                        var origMsgType = source[Tags.MsgType].AsString;      //To MessageType του αποθηκευμενου message
                        /*
                         * Δεν στελνουμε ξανα Logon(35 = A), Logout(35 = 5), ResendRequest(35 = 2), HeartBeat(35 = 0), TestRequest(35 = 1) και SequenceReset(35 = 4)
                         * Αν και προσεχουμε να μην αποθηκευουμε τετοιου ειδους μηνυματα στο store, εχουμε και εδω ενα επιπλεον ελεγχο
                         */
                        if (origMsgType == "A" || origMsgType == "5" || origMsgType == "2" || origMsgType == "0" || origMsgType == "1" || origMsgType == "4")
                        {
                            //Το αγνοουμε και παμε στο επομενο
                            continue;
                        }


                        var origMsgNo = source[Tags.MsgSeqNum].AsLong;        //Το SequenceNumber του αποθηκευμενου message
                        if (currSeqNo > origMsgNo)
                        {
                            /*
                             * Δεν πρεπει να βρεθουμε σε μια τετοια κατασταση. Προς στιγμην γρφουμε στο log και παμε στο επομενο
                             */
                            MetricsProxy.Instance.OnFIXClientWarning();
                            theLogger.Warning($"While retransmitting, an unexpected situation arose 'currSeqNo > origMsgNo' => {currSeqNo} > {origMsgNo}!!!");
                            continue;
                        }
                        /*
                         * Μηπως πρεπει να στειλω ενα GAP Filling πρωτα??
                         */
                        if (currSeqNo < origMsgNo)
                        {
                            #region Send Sequence Reset (Gap Filling)
                            m_outbound.Clear();
                            m_outbound.Set(Tags.PossDupFlag, 'Y');
                            //m_outbound.Set(Tags.OrigSendingTime, source[Tags.SendingTime].AsString);
                            m_outbound.Set(Tags.GapFillFlag, 'Y');
                            m_outbound.Set(Tags.NewSeqNo, origMsgNo);
                            #endregion

                            Send("4", m_outbound, currSeqNo);
                            currSeqNo = origMsgNo;
                        }
                        /*
                         * Στελνουμε το αποθηκευμενο μηνυμα τωρα:
                         */
                        if (currSeqNo == origMsgNo)
                        {
                            #region Retransmit message
                            m_outbound.Clear();
                            m_outbound.Set(Tags.OrigSendingTime, source[Tags.SendingTime].AsString);
                            m_outbound.Set(Tags.PossDupFlag, 'Y');          //Indicates possible retransmission of message with this sequence number
                            //m_outbound.Set(Tags.PossResend, 'Y');           //Indicates that message may contain information that has been sent under another sequence number

                            for (int i = 0; i < source.FieldCount - 1; i++)
                            {
                                var field = source.m_fields[i];
                                if (
                                        field.Tag == Tags.BeginString || field.Tag == Tags.BodyLength || field.Tag == Tags.MsgType || field.Tag == Tags.MsgSeqNum ||
                                        field.Tag == Tags.SenderCompID || field.Tag == Tags.TargetCompID || field.Tag == Tags.TargetSubID || field.Tag == Tags.SenderSubID ||
                                        field.Tag == Tags.SendingTime || field.Tag == Tags.CheckSum
                                 )
                                {
                                    continue;
                                }

                                m_outbound.Set(field.Tag, field.AsString);
                            }

                            Send(origMsgType, m_outbound, origMsgNo);
                            #endregion
                            currSeqNo++;
                            Thread.Sleep(20);           //Επιτηδες για να μην πνιξουμε τον ATHENS FIX SERVER.....
                        }
                    }

                    /*
                     * Κοιταμε μηπως υπαρχει ακομα Gap filing:
                     */
                    if (currSeqNo < _endSeqNo)
                    {
                        #region Send Sequence Reset (Gap Filling)
                        m_outbound.Clear();
                        m_outbound.Set(Tags.PossDupFlag, 'Y');
                        //m_outbound.Set(Tags.OrigSendingTime, source[Tags.SendingTime].AsString);
                        m_outbound.Set(Tags.GapFillFlag, 'Y');
                        m_outbound.Set(Tags.NewSeqNo, _endSeqNo);
                        #endregion

                        Send("4", m_outbound, currSeqNo);
                    }

                    theLogger.Info($"!!!!!!!!!!!!!!!!!! RETRANSMITTING COMPLETED, RETRANSMITTING COMPLETED, RETRANSMITTING COMPLETED !!!!!!!!!!!!!!!!!!!!!!!");
                    theLogger.Info("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    #endregion
                }
                catch (Exception ex)
                {
                    MetricsProxy.Instance.OnFIXClientError();
                    theLogger.Error(ex.Message);
                    theLogger.Error($"!!!!!!!!!!!! RETRANSMITTING FAILED, RETRANSMITTING FAILED, RETRANSMITTING FAILED, RETRANSMITTING FAILED !!!!!!!!!!!!!!");
                    theLogger.Error("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
                finally
                {
                    m_state.Server_Synchronizing = false;
                    MetricsProxy.Instance.OnFIXClientState(m_state);
                    m_state.ShowInfo();
                }

            }));
        }

        /// <summary>
        /// Sequence Reset <4> message
        /// The Sequence Reset message has two modes: Gap Fill mode and Reset mode.
        /// </summary>
        /// <param name="inbound"></param>
        /// <exception cref="PtFixNonFatalException"></exception>
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleSequenceReset(FIXMessage inbound)
        {

            #region Structure Validation
            /*
             *          123 GapFillFlag  NON REQUIRED
             *          36  NewSeqNo     REQUIRED
             */
            if (!inbound.Contains(Tags.NewSeqNo))        //REQUIRED
            {
                var rejectException = new PtFixRejectException(inbound[Tags.MsgSeqNum].AsInt, "Required tag NewSeqNo<36> is missing");
                rejectException.RejectReason = SessionRejectReason.Required_tag_missing;
                rejectException.RefMsgType = inbound[Tags.MsgType].AsString;

                throw rejectException;
            }
            #endregion


            //Read NewSeqNo & GapFillFlag
            int newSeqNum = inbound[Tags.NewSeqNo].AsInt;
            var gapFillFlag = false;
            if (inbound.Contains(Tags.GapFillFlag))
            {
                if (inbound[Tags.GapFillFlag].Is("Y"))
                    gapFillFlag = true;
            }


            if (gapFillFlag == true)
            {
                /*
                 * Gap Fill mode is used in response to a Resend Request < 2 > when one or more messages must be skipped
                 * The MsgSeqNum <34> should represent the beginning MsgSeqNum <34> in the GapFill range because the 
                 * remote side is expecting that next message sequence number
                 */
                if (newSeqNum < m_state.NextInboundSeqNum)
                {
                    theLogger.Error($"SequenceReset (Gap Fill mode) message INVALID. (Bad NewSeqNo, NewSeqNo={newSeqNum} <= m_state.NextInboundSeqNum={m_state.NextInboundSeqNum})");

                    var rejectException = new PtFixRejectException(inbound[Tags.MsgSeqNum].AsInt, $"Atempt to lower sequence number, invalid NewSeqNo ({newSeqNum})");
                    rejectException.RejectReason = SessionRejectReason.Value_is_incorrect_out_of_range_for_this_tag;
                    rejectException.RefMsgType = inbound[Tags.MsgType].AsString;

                    throw rejectException;
                }

                // Accept the new sequence number
                m_state.NextInboundSeqNum = newSeqNum;

                theLogger.Warning($"SequenceReset (Gap Fill mode) message. NextInboundSeqNum expected: {m_state.NextInboundSeqNum}");
                _UpdateSynchronizingStatus(newSeqNum, callee: 2);
            }
            else
            {
                /*
                 * Reset mode involves specifying an arbitrarily higher new sequence number to be expected by the receiver of the 
                 * Sequence Reset <4>-Reset mode message, and is used to reestablish a FIX session after an unrecoverable application failure. 
                 */
                theLogger.Warning("SequenceReset (Reset mode) message.");
                throw new Exception("Unsupported sequence reset received (hard reset)");
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleLogon(FIXMessage inbound)
        {
            throw new Exception("Logon message received while already logged on");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void HandleLogout(FIXMessage inbound)
        {
            m_state.ReceivedLogout = true;
            m_state.ReceivedLogon = false;
            m_state.SentLogon = false;

            if (m_state.SentLogout == false)
            {
                // Send a logout message
                Send("5", m_outbound.Clear());
            }
            else
            {
                _receiveLogoutEvent.Set();
            }
        }
        #endregion




        /// <summary>
        /// Αποστολη application level μηνυματων με παραλληλη
        /// καταγραφη στο StoreMessage για πιθανο retransmission
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string messageType, FIXMessageWriter message)
        {
            try
            {
                message.Prepare(m_settings.Version, messageType, m_state.NextOutboundSeqNum, _clock.Time, m_settings.SenderCompID, m_settings.TargetCompID);

                //Στελνουμε το μηνυμα μας..
                if (m_connector.Send(message.Buffer, 0, message.Length))
                {
                    //Αποθηκευουμε το μηνυμα που προκειται να στειλουμε στο IMessageStore
                    m_state.SaveOutbound(m_state.NextOutboundSeqNum, message.Buffer, message.Length);

                    //theLogger.Info($"{m_sessionId.Id}.out [>] - {message}");
                    theLogger.Info($"out [>] - {message}");

                    m_state.NextOutboundSeqNum++;
                    m_state.OutboundTimestamp = _clock.Time;
                    return true;
                }
                else
                {
                    MetricsProxy.Instance.OnFIXClientError();
                    theLogger.Error($"SendMessage failed for [>] - {message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnFIXClientError();
                theLogger.Error($"SendMessage failed for [>] - {message}");
                theLogger.Error(ex);
            }
            return false;
        }

        #region Local Send Utilities
        /// <summary>
        /// Αποστολη session level μηνυματων
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Send(string messageType, FIXMessageWriter message)
        {
            message.Prepare(m_settings.Version, messageType, m_state.NextOutboundSeqNum, _clock.Time, m_settings.SenderCompID, m_settings.TargetCompID);

            //Στελνουμε το μηνυμα μας..
            if (m_connector.Send(message.Buffer, 0, message.Length) == true)
            {
                //theLogger.Info($"{m_sessionId.Id}.out [>] - {message}");
                theLogger.Info($"out [>] - {message}");
            }
            else
            {
                MetricsProxy.Instance.OnFIXClientError();
                throw new Exception($"Send failed for [>] - {message}");
            }


            m_state.NextOutboundSeqNum++;
            m_state.OutboundTimestamp = _clock.Time;
        }

        /// <summary>
        /// Αποστολη session level μηνυματων με explicit OutboundSeqNum
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="seqNum"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Send(string messageType, FIXMessageWriter message, long seqNum)
        {
            message.Prepare(m_settings.Version, messageType, seqNum, _clock.Time, m_settings.SenderCompID, m_settings.TargetCompID);

            //Στελνουμε το μηνυμα μας..
            if (m_connector.Send(message.Buffer, 0, message.Length) == true)
            {
                //theLogger.Info($"{m_sessionId.Id}.out [>] - {message}");
                theLogger.Info($"out [>] - {message}");
            }
            else
            {
                MetricsProxy.Instance.OnFIXClientError();
                throw new Exception($"Send failed for [>] - {message}");
            }


            m_state.OutboundTimestamp = _clock.Time;
        }
        #endregion
    }
}
