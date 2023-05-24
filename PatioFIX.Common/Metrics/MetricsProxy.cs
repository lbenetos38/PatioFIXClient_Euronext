using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.FixSupport;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace PatioFIX.Common
{


    /// <summary>
    /// 
    /// </summary>
    internal class MetricsProxy
    {
        readonly System.Object m_lockObj = new System.Object();
        static Logger theLogger = new Logger("MetricsProxy");
        readonly IPEndPoint m_serverEP = null;
        readonly System.Threading.Timer m_timer;
        readonly Int32 m_timerInterval = 1000;
        readonly System.Object m_socketLock = new System.Object();
        Socket m_socket = null;
        LocalStateDTO m_localCopyOfState = new LocalStateDTO();
        readonly byte[] m_emptyBytesArray = Array.Empty<byte>();

        public static readonly MetricsProxy Instance = new MetricsProxy();

        /// <summary>
        /// Ενας counter που μετραει ποσες φορες ο m_timer εχει τρεξει
        /// </summary>
        public Int32 TimerBeats { get; internal set; } = 0;

        /// <summary>
        /// Μας λεει εαν ειμαστε συνδεδεμενοι με τον AggregatorServer
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (m_socket != null)
                {
                    lock (m_socketLock)
                    {
                        if (m_socket != null)
                        {
                            if (m_socket.Connected)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        MetricsProxy()
        {
            if (Globals.Configuration.EnableMonitoring)
            {
                var serverIP = Globals.Configuration.AggregatorServerIP;
                var port = Globals.Configuration.AggregatorServerPort;

                /*
				* Διαβαζουμε την serverIP και φτιαχνουμε ενα πληρες IPEndPoint που δειχνει που
				* βρισκεται ο Aggregator Server που θα λαμβανει τα metrics μας
				*/
                if (!IPAddress.TryParse(serverIP, out IPAddress m_serverIP))
                {
                    throw new ArgumentException($"Invalid serverIP {serverIP}");
                }
                m_serverEP = new IPEndPoint(m_serverIP, port);



                /*
				 * Και σε αυτο το σημειο φτιαχνουμε ενα timer το οποιο θα προσπαθει
				 * συνεχως να συνδεθεί με τον Aggregator Server:
				 */
                m_timer = new Timer((state) =>
                {
                    this.TimerBeats++;
                    try
                    {
                        OnTimer();
                    }
                    catch (Exception ex)
                    {
                        theLogger.Warning($"OnTimer() threw exception: {ex.Message}");
                    }
                    finally
                    {
                        m_timer.Change(m_timerInterval, Timeout.Infinite);
                    }
                }, null, 0, Timeout.Infinite);
            }
        }



        /// <summary>
        /// Εδω ερχεται ο timer καθε m_timerInterval ms, και προσπαθει να συνδεθει με τον AggregatorServer μας...
        /// </summary>
        [SkipLocalsInit]
        void OnTimer()
        {
            if (m_socket == null)
            {
                lock (m_socketLock)
                {
                    try
                    {
                        var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _socket.NoDelay = true;
                        _socket.SendBufferSize = 16384;

                        try
                        {
                            _socket.Connect(m_serverEP);
                            theLogger.Info($"Connected to AggregateServer at {_socket.RemoteEndPoint}...");
                        }
                        catch
                        {
                            theLogger.Verbose($"Couldn't connect to AggregateServer at {m_serverEP}....");

                            _socket.Close();
                            _socket = null;
                            throw;
                        }

                        try
                        {
                            /*
                            * Στελνουμε ενα πρωτο πακετο για να του πουμε ποιοι ειμαστε (Admin ή Broker)
                            */
                            Span<byte> buffer = stackalloc byte[19];
                            buffer.Clear();
                            buffer[0] = 0x01;                   //SOH - START OF HEADING
                            buffer[1] = 0x40;                   //@
                            buffer[2] = 0x50;                   //P
                            buffer[3] = 0x40;                   //@
                            buffer[4] = (byte)MetricKeysEnumeration.Hello;
                            buffer[5] = (Globals.ClientRole == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
                            buffer[6] = (byte)(m_localCopyOfState.SentLogon == true ? 1 : 0);
                            buffer[7] = (byte)(m_localCopyOfState.ReceivedLogon == true ? 1 : 0);
                            buffer[8] = (byte)(m_localCopyOfState.SentLogout == true ? 1 : 0);
                            buffer[9] = (byte)(m_localCopyOfState.ReceivedLogout == true ? 1 : 0);
                            buffer[10] = (byte)(m_localCopyOfState.Synchronizing == true ? 1 : 0);
                            buffer[11] = (byte)(m_localCopyOfState.Server_Synchronizing == true ? 1 : 0);
                            buffer[12] = (byte)(m_localCopyOfState.TestRequestPending == true ? 1 : 0);
                            buffer[13] = (byte)(m_localCopyOfState.ForceStopInProcess == true ? 1 : 0);
                            buffer[14] = (byte)(m_localCopyOfState.IsTCPConnected == true ? 1 : 0);
                            buffer[15] = (byte)(m_localCopyOfState.IsFixClientStarted == true ? 1 : 0);
                            buffer[18] = 0x04;                  //EOT - END OF TRANSMISSION
                            _socket.Send(buffer);
                            /*
                            * Στελνουμε NextOutboundSeqNum, NextInboundSeqNum
                            */
                            var _value1 = BitConverter.GetBytes(m_localCopyOfState.NextOutboundSeqNum);     //4 bytes
                            var _value2 = BitConverter.GetBytes(m_localCopyOfState.NextInboundSeqNum);	    //4 bytes
                            buffer[4] = (byte)MetricKeysEnumeration.FIXClientState2;
                            buffer[6] = _value1[0];
                            buffer[7] = _value1[1];
                            buffer[8] = _value1[2];
                            buffer[9] = _value1[3];
                            buffer[10] = _value2[0];
                            buffer[11] = _value2[1];
                            buffer[12] = _value2[2];
                            buffer[13] = _value2[3];
                            _socket.Send(buffer);
                        }
                        finally
                        {
                            m_socket = _socket;
                        }

                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode == 10061)
                        {
                            //No connection could be made because the target machine actively refused it.
                            theLogger.Verbose($"OnTimer:: {ex.Message}");
                        }
                        else
                        {
                            theLogger.Error($"OnTimer:: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        theLogger.Error($"OnTimer:: {ex.Message}");
                    }
                }

                return;
            }
        }

        /// <summary>
        /// Στελνει μια μετρηση στον AggregatorServer
        /// </summary>
        /// <param name="buffer">To buffer που περιεχει την μετρηση μας, καταλληλα κωδικοποιημενη</param>
        /// <returns></returns>
        [SkipLocalsInit]
        bool SendToAggregatorServer(Span<byte> buffer)
        {
            try
            {
                if (m_socket != null)
                {
                    int numOfBytes = m_socket.Send(buffer);

                    return true;
                }
            }
            catch (SocketException ex)
            {
                theLogger.Error($"SendToAggregatorServer: {ex.Message}");
            }
            catch (Exception ex)
            {
                theLogger.Error($"SendToAggregatorServer: {ex.Message}");
            }

            /*
			 * Για να ειμαστε εδω, φαγαμε καποιο σκασιμο.
			 * Κλεινουμε και κανουμε dispose και null το m_socket
			 * Ο Timer μας ,σε λιγο θα προσπαθησει να συνδεθει ξανα....
			 */
            if (m_socket != null)
            {
                lock (m_socketLock)
                {
                    if (m_socket != null)
                    {
                        m_socket.Shutdown(SocketShutdown.Both);
                        m_socket.Close();
                        m_socket.Dispose();//Maybe is has benn called from Close...
                        m_socket = null;

                        theLogger.Verbose("SendToAggregatorServer: Disconnected from AggregatorServer...");
                    }
                }
            }
            return false;
        }



        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, int value1 = 0, int value2 = 0)
        {
            Span<byte> buffer = stackalloc byte[19];
            var _value1 = BitConverter.GetBytes(value1);	    //4 bytes
            var _value2 = BitConverter.GetBytes(value2);	    //4 bytes


            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = _value1[0];
            buffer[7] = _value1[1];
            buffer[8] = _value1[2];
            buffer[9] = _value1[3];
            buffer[10] = _value2[0];
            buffer[11] = _value2[1];
            buffer[12] = _value2[2];
            buffer[13] = _value2[3];
            buffer[14] = 0;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;
            buffer[18] = 0x04;                  //EOT - END OF TRANSMISSION

            SendToAggregatorServer(buffer);
        }
        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, LocalStateDTO state)
        {
            Span<byte> buffer = stackalloc byte[19];


            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = (byte)(state.SentLogon == true ? 1 : 0);
            buffer[7] = (byte)(state.ReceivedLogon == true ? 1 : 0);
            buffer[8] = (byte)(state.SentLogout == true ? 1 : 0);
            buffer[9] = (byte)(state.ReceivedLogout == true ? 1 : 0);
            buffer[10] = (byte)(state.Synchronizing == true ? 1 : 0);
            buffer[11] = (byte)(state.Server_Synchronizing == true ? 1 : 0);
            buffer[12] = (byte)(state.TestRequestPending == true ? 1 : 0);
            buffer[13] = (byte)(state.ForceStopInProcess == true ? 1 : 0);
            buffer[14] = 0;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;
            buffer[18] = 0x04;                  //EOT - END OF TRANSMISSION

            SendToAggregatorServer(buffer);
        }
        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, ODLMessageTypeEnum mtype, long value1 = 0)
        {
            Span<byte> buffer = stackalloc byte[19];
            var _value1 = BitConverter.GetBytes(value1);	    //8 bytes


            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = _value1[0];
            buffer[7] = _value1[1];
            buffer[8] = _value1[2];
            buffer[9] = _value1[3];
            buffer[10] = _value1[4];
            buffer[11] = _value1[5];
            buffer[12] = _value1[6];
            buffer[13] = _value1[7];
            buffer[14] = (byte)mtype;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;
            buffer[18] = 0x04;                  //EOT - END OF TRANSMISSION

            SendToAggregatorServer(buffer);
        }
        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, string value1, string value2 = null)
        {
            var _string1 = Encoding.ASCII.GetBytes(value1);
            var _string2 = value2 != null ? Encoding.ASCII.GetBytes(value2) : m_emptyBytesArray;

            Span<byte> buffer = stackalloc byte[19 + (_string1.Length + 1) + (_string2.Length + 1)];


            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = 0;
            buffer[7] = 0;
            buffer[8] = 0;
            buffer[9] = 0;
            buffer[10] = 0;
            buffer[11] = 0;
            buffer[12] = 0;
            buffer[13] = 0;
            buffer[14] = 0;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;


            int _next_idx = 18;
            #region add area,controller και action
            for (int i = 0; i < _string1.Length; i++)
            {
                buffer[_next_idx++] = _string1[i];
            }
            buffer[_next_idx++] = (byte)';';


            for (int i = 0; i < _string2.Length; i++)
            {
                buffer[_next_idx++] = _string2[i];
            }
            buffer[_next_idx++] = (byte)';';
            #endregion


            buffer[_next_idx++] = 0x04;                  //EOT - END OF TRANSMISSION


            SendToAggregatorServer(buffer);
        }
        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, MarketStatusMessage marketStatus)
        {
            Span<byte> buffer = stackalloc byte[19];



            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = (byte)marketStatus.SecurityExchange[0];
            buffer[7] = (byte)marketStatus.SecurityExchange[1];
            buffer[8] = (byte)marketStatus.SecurityExchange[2];
            buffer[9] = (byte)marketStatus.SecurityExchange[3];
            buffer[10] = (byte)marketStatus.MarketID;
            buffer[11] = (byte)marketStatus.BoardID;
            buffer[12] = (byte)marketStatus.TradingSessionID;
            buffer[13] = (byte)marketStatus.TradSesStatus;
            buffer[14] = 0;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;
            buffer[18] = 0x04;                  //EOT - END OF TRANSMISSION

            SendToAggregatorServer(buffer);
        }
        [SkipLocalsInit]
        void _prepBufferAndSend(ODLMesssageSource origin, MetricKeysEnumeration key, SecurityStatusMessage securityStatus)
        {
            var _exchange = Encoding.ASCII.GetBytes(securityStatus.SecurityExchange);
            var _securityID = Encoding.ASCII.GetBytes(securityStatus.SecurityID);

            Span<byte> buffer = stackalloc byte[19 + (_exchange.Length + 1) + (_securityID.Length + 1)];


            buffer[0] = 0x01;                   //SOH - START OF HEADING
            buffer[1] = 0x40;                   //@
            buffer[2] = 0x50;                   //P
            buffer[3] = 0x40;                   //@
            buffer[4] = (byte)key;
            buffer[5] = (origin == ODLMesssageSource.Administrator) ? (byte)0 : (byte)1;
            buffer[6] = (byte)securityStatus.SecurityStatus;
            buffer[7] = (byte)securityStatus.PhaseID;
            buffer[8] = (byte)securityStatus.MarketID;
            buffer[9] = 0;
            buffer[10] = 0;
            buffer[11] = 0;
            buffer[12] = 0;
            buffer[13] = 0;
            buffer[14] = 0;
            buffer[15] = 0;
            buffer[16] = 0;
            buffer[17] = 0;


            int _next_idx = 18;
            #region add area,controller και action
            for (int i = 0; i < _exchange.Length; i++)
            {
                buffer[_next_idx++] = _exchange[i];
            }
            buffer[_next_idx++] = (byte)';';


            for (int i = 0; i < _securityID.Length; i++)
            {
                buffer[_next_idx++] = _securityID[i];
            }
            buffer[_next_idx++] = (byte)';';
            #endregion


            buffer[_next_idx++] = 0x04;                  //EOT - END OF TRANSMISSION


            SendToAggregatorServer(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Warmup()
        {

        }

        #region TCPConnection
        public void OnTCPIncomingMessage(int numOfBytes)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPIncomingMessage, numOfBytes);
                }
            }
        }
        public void OnTCPOutcomingMessage(int numOfBytes)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPOutcomingMessage, numOfBytes);
                }
            }
        }
        public void OnTCPWarning()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPWarning);
                }
            }
        }
        public void OnTCPError()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPError);
                }
            }
        }
        public void OnTCPConnect()
        {
            m_localCopyOfState.OnTCPConnect();
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPConnect);
                }
            }
        }
        public void OnTCPDisconnect()
        {
            m_localCopyOfState.OnTCPDisconnect();
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.TCPDisconnect);
                }
            }
        }
        #endregion


        #region FIXClient
        public void OnFIXClientNewInstance()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientNewInstance);
                }
            }
        }
        public void OnFIXClientStarted(bool value)
        {
            m_localCopyOfState.OnFIXClientStarted(value);
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientStarted, value ? 1 : 0);
                }
            }
        }
        public void OnFIXClientInboundSeqNumTooHigh()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientInboundSeqNumTooHigh);
                }
            }
        }
        public void OnFIXClientThrowAwayMessage()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientThrowAwayMessage);
                }
            }
        }
        public void OnFIXClientIgnoredMessage()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientIgnoredMessage);
                }
            }
        }

        public void OnFIXClientSendResendRequest()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientSendResendRequest);
                }
            }
        }
        public void OnFIXClientSendRejection()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientSendRejection);
                }
            }
        }


        public void OnFIXClientState(SessionState state)
        {
            m_localCopyOfState.SetState(state);
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientState1, m_localCopyOfState);
                }
            }
        }
        public void OnFIXClientState(int NextOutboundSeqNum, int NextInboundSeqNum)
        {
            m_localCopyOfState.SetSeqNums(NextOutboundSeqNum, NextInboundSeqNum);
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientState2, NextOutboundSeqNum, NextInboundSeqNum);
                }
            }
        }
        public void OnFIXClientSessionTimerHeartBeat()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientSessionTimerHeartBeat);
                }
            }
        }


        public void OnFIXClientFailedLogin()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientFailedLogin);
                }
            }
        }
        public void OnFIXClientWarning()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientWarning);
                }
            }
        }
        public void OnFIXClientError()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientError);
                }
            }
        }
        public void OnFIXClientReject()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.FIXClientReject);
                }
            }
        }
        #endregion


        public void OnParsingWarning()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.ParsingWarning);
                }
            }
        }
        public void OnParsingError()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.ParsingError);
                }
            }
        }

        public void OnRejectionsWarning()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.RejectionsWarning);
                }
            }
        }
        /// <summary>
        /// Χρησιμοποιείται αποκλειστικά απο το Evaluator
        /// </summary>
        /// <param name="mtype"></param>
        /// <param name="elapsedTicks"></param>
        internal void OnEvaluationIime(ODLMessageTypeEnum mtype, long elapsedTicks)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.EvaluationIime, mtype, elapsedTicks);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtype"></param>
        public void OnReceive(ODLMessageTypeEnum mtype)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.Receive, mtype);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtype"></param>
        public void OnSend(ODLMessageTypeEnum mtype)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.Send, mtype);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        public void OnError(string source, string type = null)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.Error, source, type);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        public void OnWarning(string source, string type = null)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.Warning, source, type);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnControllerHeartBeat()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.ControllerHeartBeat);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void OnEventsListenerHeartBeat()
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.EventsListenerHeartBeat);
                }
            }
        }

        public void UnConfirmedPoolMessages(int value)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.UnConfirmedPoolMessages, value);
                }
            }
        }



        public void OnMarketStatus(MarketStatusMessage marketStatus)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.MarketStatus, marketStatus);
                }
            }
        }
        public void OnSecurityStatus(SecurityStatusMessage securityStatus)
        {
            if (IsConnected)
            {
                lock (m_lockObj)
                {
                    _prepBufferAndSend(Globals.ClientRole, MetricKeysEnumeration.SecurityStatus, securityStatus);
                }
            }
        }


        public void ResetMetrics()
        {

        }
    }


}
