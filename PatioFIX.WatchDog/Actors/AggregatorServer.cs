using PatioFIX.Common;
using PatioFIX.WatchDog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatioFIX.WatchDog
{
    class ClientState
    {
        public readonly byte[] _rcvBuffer = new byte[1024];
        public readonly byte[] _asmBuffer = new byte[1024];
        public readonly TcpClient m_client;
        public readonly System.Net.EndPoint m_remoteEndPoint;
        public int _asmIdx = -1;

        public int numOfBytes = 0;
        public int pktCounter = 0;
        public int msgCounter = 0;

        public ODLMesssageSource ClientRole { get; set; }
        public DateTime LastMessageRcvDT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public ClientState(TcpClient client)
        {
            this.m_client = client;
            this.m_remoteEndPoint = client.Client.RemoteEndPoint;
            this.ClientRole = ODLMesssageSource.Unknown;
        }
    }

    internal class AggregatorServer
    {
        readonly Logger theLogger = new Logger("AggregatorServer");
        readonly object _serverLock = new object();
        readonly ManualResetEvent _forceCancelEvent;
        readonly int m_port;
        readonly IPAddress m_ipaddress;
        bool m_isStarted = false;
        Task m_listenerTask = null;
        TcpListener m_listener = null;
        readonly IList<TcpClient> _clients = new List<TcpClient>();
        readonly Object _clients_sync;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="forceCancelEvent"></param>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public AggregatorServer(ManualResetEvent forceCancelEvent, int port, string ip = "127.0.0.1")
        {
            _forceCancelEvent = forceCancelEvent;
            m_port = port;
            m_ipaddress = IPAddress.Parse(ip);
            _clients_sync = ((System.Collections.ICollection)_clients).SyncRoot;

            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($".ctor() called by ThreadPoolThread (ManagedThreadId = {Environment.CurrentManagedThreadId}), port = {m_port}, ip = {ip}");
            else
                theLogger.Verbose($".ctor() called by '{Thread.CurrentThread.Name}', port = {m_port}, ip = {ip}");
        }

        public bool IsStarted => m_isStarted;

        /// <summary>
        /// 
        /// </summary>
        public Int32 NumberOfClients
        {
            get
            {
                lock (_clients_sync)
                {
                    return _clients.Count;
                }
            }
        }

        public void Start()
        {
            if (m_isStarted == false)
            {
                lock (_serverLock)
                {
                    if (m_isStarted == false)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Environment.CurrentManagedThreadId}).");
                        else
                            theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'.");

                        m_listenerTask = Task.Run(ListeningThread);

                        m_isStarted = true;
                    }
                }
            }
        }

        public void Stop()
        {
            if (m_isStarted == true)
            {
                lock (_serverLock)
                {
                    if (m_isStarted == true)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Environment.CurrentManagedThreadId}).");
                        else
                            theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'.");


                        theLogger.Info("Stopping TCPListener...");
                        m_listener.Stop();
                        theLogger.Info("Stopping Server Thread...");
                        m_listenerTask.Wait();


                        /*
						 * Εαν υπαρχουν TCPClients στην λιστα _clients, τα κλεινουμε ενα - ενα
						 */
                        if (NumberOfClients > 0)
                        {
                            var _copyOfClients = new List<TcpClient>();
                            lock (_clients_sync)
                            {
                                foreach (var clnt in _clients)
                                {
                                    _copyOfClients.Add(clnt);
                                }
                            }

                            foreach (var clnt in _copyOfClients)
                            {
                                if (clnt.Client != null)
                                {
                                    var remoteEndPoint = clnt.Client.RemoteEndPoint;
                                    try
                                    {
                                        theLogger.Info($"Closing client {remoteEndPoint}...");
                                        clnt.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        theLogger.Warning($"Exception while closing client {remoteEndPoint}: -> {ex.Message}");
                                    }
                                }
                            }
                        }

                        theLogger.Info("Stop() finished...");
                    }
                }
            }
        }



        void ListeningThread()
        {
            try
            {
                theLogger.Info("ListeningThread started....");
                m_listener = new TcpListener(m_ipaddress, m_port);

                // Start listening for client requests.
                m_listener.Start();

                while (_forceCancelEvent.WaitOne(0) == false)
                {
                    theLogger.Info($"ListeningThread is waiting for a connection at {m_listener.LocalEndpoint}... ");

                    TcpClient client = m_listener.AcceptTcpClient();
                    client.ReceiveBufferSize = 16384;
                    theLogger.Info($"Connected client, {client.Client.RemoteEndPoint}");

                    Task.Run(() =>
                    {
                        ReadThread(client);
                    });
                }
            }
            catch (Exception ex)
            {
                theLogger.Error($"ListeningThread: {ex.Message}");
            }
        }


        void ReadThread(TcpClient client)
        {
            ClientState state = new ClientState(client);

            try
            {
                theLogger.Info($"ReadThread<{state.m_remoteEndPoint}>: start reading bytes...");

                lock (_clients_sync)
                {
                    _clients.Add(client);
                }
                AdminMetrics.Instance.OnSetRemoteClients(_clients.Count);

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                Byte[] bytes = new Byte[2048];
                int numOfBytes;

                while ((numOfBytes = stream.Read(state._rcvBuffer, 0, state._rcvBuffer.Length)) != 0)
                {
                    if (_forceCancelEvent.WaitOne(0) == true)
                    {
                        theLogger.Info($"ReadThread<{state.m_remoteEndPoint}>:: _forceCancelEvent singnaled....");
                        break;
                    }

                    state.numOfBytes += numOfBytes;
                    state.pktCounter++;
                    _AssembleMessage(state, numOfBytes);
                }

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                theLogger.Error($"ReadThread<{state.m_remoteEndPoint}>: {ex.Message}");
            }
            finally
            {
                lock (_clients_sync)
                {
                    if (state.ClientRole == ODLMesssageSource.Administrator)
                    {
                        AdminMetrics.Instance.OnWatchDogDisconnected();
                    }
                    else if (state.ClientRole == ODLMesssageSource.Broker)
                    {
                        BrokerMetrics.Instance.OnWatchDogDisconnected();
                    }

                    _clients.Remove(client);
                }
                AdminMetrics.Instance.OnSetRemoteClients(_clients.Count);
            }
        }


        void _AssembleMessage(ClientState state, int numOfBytes)
        {
            for (int idx = 0; idx < numOfBytes; idx++)
            {
                if (++state._asmIdx >= state._asmBuffer.Length)
                {
                    throw new Exception($"_asmBuffer OVERFLOW, current capacity is {state._asmBuffer.Length} bytes");
                }

                //Copy from the _rcvBuffer to assembly buffer:
                state._asmBuffer[state._asmIdx] = state._rcvBuffer[idx];

                if (_forceCancelEvent.WaitOne(0) == true)
                {
                    theLogger.Info("_AssembleMessage:: _forceCancelEvent singnaled....");
                    break;
                }

                int _length = state._asmIdx + 1;
                if (_length == 4)
                {
                    if (_IsMetricMessage(state._asmBuffer) == false)
                    {
                        var sb = new StringBuilder($"GarbledMessage:: Message doesn't start correctly; IncomingBytes: [");
                        for (int jb = 0; jb <= state._asmIdx; jb++)
                        {
                            if (jb > 0)
                                sb.Append(",");
                            sb.AppendFormat("0x{0:X}", state._rcvBuffer[jb]);
                        }
                        sb.Append("]");

                        throw new Exception(sb.ToString());
                    }
                }

                if (_length >= 19)
                {
                    if (state._asmBuffer[state._asmIdx] == 0x04/*EOT - END OF TRANSMISSION*/)
                    {
                        state.msgCounter++;
                        _DispatchMessage(state);
                        state._asmIdx = -1;

                        if (state.msgCounter % 1000 == 0)
                        {
                            theLogger.Info($"Total messages = {state.msgCounter} for client {state.m_remoteEndPoint}...");
                        }
                    }
                }
            }
        }



        static bool _IsMetricMessage(byte[] buffer)
        {
            if (buffer[0] != 0x01) return false;//SOH - START OF HEADING
            if (buffer[1] != 0x40) return false;//@
            if (buffer[2] != 0x50) return false;//P
            if (buffer[3] != 0x40) return false;//@

            return true;
        }



        void _DispatchMessage(ClientState state)
        {
            System.Net.EndPoint client = state.m_client.Client.LocalEndPoint;
            Byte[] tbuffer = state._asmBuffer;
            int length = state._asmIdx + 1;

            state.LastMessageRcvDT = DateTime.Now;
            try
            {
                var selectedMethod = (MetricKeysEnumeration)tbuffer[4];
                var origin = (ODLMesssageSource)tbuffer[5];


                string sparam1 = string.Empty;
                string sparam2 = string.Empty;
                if (selectedMethod == MetricKeysEnumeration.Error || selectedMethod == MetricKeysEnumeration.Warning || selectedMethod == MetricKeysEnumeration.SecurityStatus)
                {
                    int idx1 = 18;
                    int idx2 = 18;
                    while (tbuffer[idx2] != (byte)';')
                        idx2++;
                    if (idx2 > idx1)
                    {
                        sparam1 = System.Text.Encoding.ASCII.GetString(tbuffer, idx1, idx2 - idx1);
                    }

                    idx1 = idx2 + 1;
                    idx2 = idx1;
                    while (tbuffer[idx2] != (byte)';')
                        idx2++;
                    if (idx2 > idx1)
                    {
                        sparam2 = System.Text.Encoding.ASCII.GetString(tbuffer, idx1, idx2 - idx1);
                    }
                }


                if (selectedMethod == MetricKeysEnumeration.Hello)
                {
                    /*
                     * Αυτο είναι το πρωτο μηνυμα που λαμβανουμε για μια νεσ σύνδεση
                     * Μας λεει στην ουσία το ClientRole (admin/broker) και
                     * μας δινει ενα μερικο state τι γινεται....
                     */
                    bool sentLogon = tbuffer[6] == 1 ? true : false;
                    bool receivedLogon = tbuffer[7] == 1 ? true : false;
                    bool sentLogout = tbuffer[8] == 1 ? true : false;
                    bool receivedLogout = tbuffer[9] == 1 ? true : false;
                    bool synchronizing = tbuffer[10] == 1 ? true : false;
                    bool server_Synchronizing = tbuffer[11] == 1 ? true : false;
                    bool testRequestPending = tbuffer[12] == 1 ? true : false;
                    bool forceStopInProcess = tbuffer[13] == 1 ? true : false;
                    bool isTCPConnected = tbuffer[14] == 1 ? true : false;
                    bool isFixClientStarted = tbuffer[15] == 1 ? true : false;


                    if (origin == ODLMesssageSource.Administrator)
                    {
                        state.ClientRole = ODLMesssageSource.Administrator;
                        AdminMetrics.Instance.OnWatchDogConnected();
                        AdminMetrics.Instance.OnFIXClientState(sentLogon, receivedLogon, sentLogout, receivedLogout, synchronizing, server_Synchronizing, testRequestPending, forceStopInProcess);

                        if(isTCPConnected)
                        {
                            AdminMetrics.Instance.OnTCPConnect();
                        }
                        if(isFixClientStarted)
                        {
                            AdminMetrics.Instance.OnFIXClientStarted(true);
                        }
                    }
                    else
                    {
                        state.ClientRole = ODLMesssageSource.Broker;
                        BrokerMetrics.Instance.OnWatchDogConnected();
                        BrokerMetrics.Instance.OnFIXClientState(sentLogon, receivedLogon, sentLogout, receivedLogout, synchronizing, server_Synchronizing, testRequestPending, forceStopInProcess);

                        if (isTCPConnected)
                        {
                            BrokerMetrics.Instance.OnTCPConnect();
                        }
                        if (isFixClientStarted)
                        {
                            BrokerMetrics.Instance.OnFIXClientStarted(true);
                        }
                    }

                    theLogger.Info($"OnWatchDogConnected ({origin})");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPIncomingMessage)
                {
                    int numOfBytes = BitConverter.ToInt32(tbuffer, 6);

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPIncomingMessage(numOfBytes);
                    else
                        BrokerMetrics.Instance.OnTCPIncomingMessage(numOfBytes);

                    //theLogger.Verbose($"TCPIncomingMessage from {origin} (numOfBytes={numOfBytes})");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPOutcomingMessage)
                {
                    int numOfBytes = BitConverter.ToInt32(tbuffer, 6);

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPOutcomingMessage(numOfBytes);
                    else
                        BrokerMetrics.Instance.OnTCPOutcomingMessage(numOfBytes);

                    //theLogger.Verbose($"TCPOutcomingMessage from {origin} (numOfBytes={numOfBytes})");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPWarning)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPWarning();
                    else
                        BrokerMetrics.Instance.OnTCPWarning();

                    theLogger.Verbose($"TCPWarning from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPError)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPError();
                    else
                        BrokerMetrics.Instance.OnTCPError();

                    theLogger.Verbose($"TCPError from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPConnect)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPConnect();
                    else
                        BrokerMetrics.Instance.OnTCPConnect();

                    theLogger.Verbose($"TCPConnect from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.TCPDisconnect)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnTCPDisconnect();
                    else
                        BrokerMetrics.Instance.OnTCPDisconnect();

                    theLogger.Verbose($"TCPDisconnect from {origin}");
                }


                else if (selectedMethod == MetricKeysEnumeration.FIXClientNewInstance)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientNewInstance();
                    else
                        BrokerMetrics.Instance.OnFIXClientNewInstance();

                    theLogger.Verbose($"FIXClientNewInstance from origin={origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientStarted)
                {
                    bool _value = BitConverter.ToInt32(tbuffer, 6) == 1 ? true : false;

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientStarted(_value);
                    else
                        BrokerMetrics.Instance.OnFIXClientStarted(_value);

                    theLogger.Verbose($"FIXClientStarted from {origin} ({_value})");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientInboundSeqNumTooHigh)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientInboundSeqNumTooHigh();
                    else
                        BrokerMetrics.Instance.OnFIXClientInboundSeqNumTooHigh();

                    theLogger.Verbose($"FIXClientInboundSeqNumTooHigh from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientThrowAwayMessage)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientThrowAwayMessage();
                    else
                        BrokerMetrics.Instance.OnFIXClientThrowAwayMessage();

                    theLogger.Verbose($"FIXClientThrowAwayMessage from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientIgnoredMessage)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientIgnoredMessage();
                    else
                        BrokerMetrics.Instance.OnFIXClientIgnoredMessage();

                    theLogger.Verbose($"FIXClientIgnoredMessage from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientSendResendRequest)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientSendResendRequest();
                    else
                        BrokerMetrics.Instance.OnFIXClientSendResendRequest();

                    theLogger.Verbose($"FIXClientSendResendRequest from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientSendRejection)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientSendRejection();
                    else
                        BrokerMetrics.Instance.OnFIXClientSendRejection();

                    theLogger.Verbose($"FIXClientSendRejection from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientState1)
                {
                    bool sentLogon = tbuffer[6] == 1 ? true : false;
                    bool receivedLogon = tbuffer[7] == 1 ? true : false;
                    bool sentLogout = tbuffer[8] == 1 ? true : false;
                    bool receivedLogout = tbuffer[9] == 1 ? true : false;
                    bool synchronizing = tbuffer[10] == 1 ? true : false;
                    bool server_Synchronizing = tbuffer[11] == 1 ? true : false;
                    bool testRequestPending = tbuffer[12] == 1 ? true : false;
                    bool forceStopInProcess = tbuffer[13] == 1 ? true : false;


                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientState(sentLogon, receivedLogon, sentLogout, receivedLogout, synchronizing, server_Synchronizing, testRequestPending, forceStopInProcess);
                    else
                        BrokerMetrics.Instance.OnFIXClientState(sentLogon, receivedLogon, sentLogout, receivedLogout, synchronizing, server_Synchronizing, testRequestPending, forceStopInProcess);


                    theLogger.Verbose($"FIXClientState1 from {origin}, (sentLogon={sentLogon}, receivedLogon={receivedLogon}, sentLogout={sentLogout}, receivedLogout={receivedLogout}, synchronizing={synchronizing}, server_Synchronizing={server_Synchronizing}, testRequestPending={testRequestPending}, forceStopInProcess={forceStopInProcess}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientState2)
                {
                    int nextOutboundSeqNum = BitConverter.ToInt32(tbuffer, 6);
                    int nextInboundSeqNum = BitConverter.ToInt32(tbuffer, 10);

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientState(nextOutboundSeqNum, nextInboundSeqNum);
                    else
                        BrokerMetrics.Instance.OnFIXClientState(nextOutboundSeqNum, nextInboundSeqNum);

                    theLogger.Verbose($"FIXClientState2 from {origin} (nextOutboundSeqNum={nextOutboundSeqNum}, nextInboundSeqNum={nextInboundSeqNum})");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientSessionTimerHeartBeat)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientSessionTimerHeartBeat();
                    else
                        BrokerMetrics.Instance.OnFIXClientSessionTimerHeartBeat();

                    //theLogger.Verbose($"FIXClientSessionTimerHeartBeat from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientFailedLogin)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientFailedLogin();
                    else
                        BrokerMetrics.Instance.OnFIXClientFailedLogin();

                    theLogger.Verbose($"FIXClientFailedLogin from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientWarning)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientWarning();
                    else
                        BrokerMetrics.Instance.OnFIXClientWarning();

                    theLogger.Verbose($"FIXClientWarning from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientError)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientError();
                    else
                        BrokerMetrics.Instance.OnFIXClientError();

                    theLogger.Verbose($"FIXClientError from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.FIXClientReject)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnFIXClientReject();
                    else
                        BrokerMetrics.Instance.OnFIXClientReject();

                    theLogger.Verbose($"FIXClientReject from {origin}");
                }

                else if (selectedMethod == MetricKeysEnumeration.ParsingWarning)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnParsingWarning();
                    else
                        BrokerMetrics.Instance.OnParsingWarning();

                    theLogger.Verbose($"ParsingWarning from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.ParsingError)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnParsingError();
                    else
                        BrokerMetrics.Instance.OnParsingError();

                    theLogger.Verbose($"ParsingError from {origin}");
                }

                else if (selectedMethod == MetricKeysEnumeration.EvaluationIime)
                {
                    ODLMessageTypeEnum mtype = (ODLMessageTypeEnum)tbuffer[14];
                    long elapsedTicks = BitConverter.ToInt64(tbuffer, 6);

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnEvaluationIime(mtype, elapsedTicks);
                    else
                        BrokerMetrics.Instance.OnEvaluationIime(mtype, elapsedTicks);

                    //theLogger.Verbose($"EvaluationIime from {origin} for {mtype}  (elapsedTicks={elapsedTicks})");
                }
                else if (selectedMethod == MetricKeysEnumeration.Receive)
                {
                    ODLMessageTypeEnum mtype = (ODLMessageTypeEnum)tbuffer[14];

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnReceive(mtype);
                    else
                        BrokerMetrics.Instance.OnReceive(mtype);

                    //theLogger.Verbose($"Receive from {origin} (mtype = {mtype})");
                }
                else if (selectedMethod == MetricKeysEnumeration.Send)
                {
                    ODLMessageTypeEnum mtype = (ODLMessageTypeEnum)tbuffer[14];

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnSend(mtype);
                    else
                        BrokerMetrics.Instance.OnSend(mtype);

                    //theLogger.Verbose($"Send from {origin} (mtype = {mtype})");
                }

                else if (selectedMethod == MetricKeysEnumeration.Error)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnError(sparam1, sparam2);
                    else
                        BrokerMetrics.Instance.OnError(sparam1, sparam2);

                    theLogger.Verbose($"Error from {origin} (sparam1={sparam1}, sparam2={sparam2})");
                }
                else if (selectedMethod == MetricKeysEnumeration.Warning)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnWarning(sparam1, sparam2);
                    else
                        BrokerMetrics.Instance.OnWarning(sparam1, sparam2);

                    theLogger.Verbose($"Warning from {origin} (sparam1={sparam1}, sparam2={sparam2})");
                }

                else if (selectedMethod == MetricKeysEnumeration.ControllerHeartBeat)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnControllerHeartBeat();
                    else
                        BrokerMetrics.Instance.OnControllerHeartBeat();

                    //theLogger.Verbose($"ControllerHeartBeat from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.EventsListenerHeartBeat)
                {
                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.OnEventsListenerHeartBeat();
                    else
                        BrokerMetrics.Instance.OnEventsListenerHeartBeat();

                    //theLogger.Verbose($"EventsListenerHeartBeat from {origin}");
                }
                else if (selectedMethod == MetricKeysEnumeration.UnConfirmedPoolMessages)
                {
                    int value = BitConverter.ToInt32(tbuffer, 6);

                    if (origin == ODLMesssageSource.Administrator)
                        AdminMetrics.Instance.UnConfirmedPoolMessages(value);
                    else
                        BrokerMetrics.Instance.UnConfirmedPoolMessages(value);

                    //theLogger.Verbose($"UnConfirmedPoolMessages from {origin} (value = {value})");
                }
                else if (selectedMethod == MetricKeysEnumeration.MarketStatus)
                {
                    if (origin == ODLMesssageSource.Administrator)
                    {
                        /*
                         * Μονο απο τον Administrator δεχομαστε MarketStatus
                         */
                        char marketID = (char)tbuffer[10];
                        char boardID = (char)tbuffer[11];
                        char tradingSessionID = (char)tbuffer[12];
                        char tradSesStatus = (char)tbuffer[13];
                        string exchange = Encoding.ASCII.GetString(tbuffer, 6, 4);

                        //theLogger.Verbose($"MarketStatus from {origin} ({exchange}, marketID={marketID}, boardID={boardID}, tradingSessionID={tradingSessionID}, tradSesStatus={tradSesStatus})");
                    }
                }
                else if (selectedMethod == MetricKeysEnumeration.SecurityStatus)
                {
                    if (origin == ODLMesssageSource.Administrator)
                    {
                        /*
                         * Μονο απο τον Administrator δεχομαστε SecurityStatus
                         */
                        char status = (char)tbuffer[6];
                        char phaseID = (char)tbuffer[7];
                        char marketID = (char)tbuffer[8];

                        //theLogger.Verbose($"SecurityStatus from {origin} ({sparam1}, {sparam2}, status={status}, phaseID={phaseID}, marketID={marketID})");
                        SecurityStatusDB.Instance.PushStatus(sparam1, sparam2, status, phaseID, marketID);
                    }
                }


            }
            catch (Exception ex)
            {
                theLogger.Error($"_DispatchMessage: {ex.Message}");
            }
        }
    }
}
