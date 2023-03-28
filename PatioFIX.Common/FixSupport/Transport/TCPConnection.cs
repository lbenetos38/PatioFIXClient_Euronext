using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;

namespace PatioFIX.Common.FixSupport.Transport
{
    /// <summary>
    /// 
    /// Το TCPConnection ειναι υλοποιεί το πραγματικο φυσικο TCP connection με τον FIX Server
    ///     -Οταν απαιτείται SSL, τοτε χρησιμοποιεί SslStream
    ///     -Οταν δεν απαιτείται SSL tunnel χρησιμοποιούμε socket 
    ///     (και οχι NetworkStream το οποιο είναι εναs wrapper για το socket)
    /// 
    /// </summary>
    public class TCPConnection : IDisposable
    {
        int _disposed = 0;// Whether Dispose has been called.
        const char SOH = '\u0001';
        readonly IClock _clock = new RealTimeClock();
        readonly Logger theLogger;
        readonly FixConfiguration m_settings;
        readonly byte[] _rcvBuffer;
        readonly byte[] _asmBuffer;
        FIXMessage m_inbound;
        readonly ManualResetEvent m_stopEvent;
        readonly Object _sync = new object();
        Thread m_thread;
        Socket m_socket = null;             //το underlying socket της σύνδεσης
        SslStream m_sslStream = null;       //τπ ssl tunnel μας
        int _asmIdx = -1;


        public event Action<FIXMessage> OnNewMessage;
        public event Action<int> OnDisconnect;


        /// <summary>
        /// Ποτε συνδεθηκαμε με τον FIX Server
        /// </summary>
        public DateTime LastConnectDT { get; protected set; } = DateTime.MinValue;
        /// <summary>
        /// Πότε αποσυνδεθήκαμε απο τον FIX Server
        /// </summary>
        public DateTime LastDisconnectDT { get; protected set; } = DateTime.MinValue;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="messageReceiver"></param>
        public TCPConnection(FixConfiguration settings)
        {
            theLogger = new Logger("TCPConnection");
            theLogger.Info(".ctor");

            m_settings = settings;

            _rcvBuffer = new byte[settings.MaxRcvBuffer];
            _asmBuffer = new byte[settings.MaxRcvBuffer * 2];

            m_inbound = new FIXMessage(m_settings.MaxMessageLength, m_settings.MaxMessageFields, m_settings.ValidateCheckSum, m_settings.ValidateBodyLength);
            m_stopEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// 
        /// </summary>
        ~TCPConnection() => Dispose(false);

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }


            if (disposing)
            {
                //From user code...
                theLogger?.Warning("Dispose(disposing = true)");
            }
            else
            {
                theLogger?.Warning("Dispose(disposing = false)");
            }

            _CloseAndInitialize();

            _disposed = 1;
        }


        void _CloseAndInitialize()
        {
            m_stopEvent.Set();

            if (m_sslStream != null || m_socket != null)
            {
                Disconnect();
                Thread.Sleep(100);
            }

            if (m_thread?.IsAlive == true)
            {
                m_thread?.Join(1000);
            }


            m_stopEvent.Reset();
            _asmIdx = -1;
        }

        void ThrowIfDisposed()
        {
            if (_disposed != 0)
            {
                ThrowObjectDisposedException();
            }

            void ThrowObjectDisposedException() => throw new ObjectDisposedException(GetType().FullName);
        }


        #region Εκτελουνται απο στο νημα του Caller....
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            ThrowIfDisposed();
            _CloseAndInitialize();

            try
            {
                theLogger.Info($"Try to connect to {m_settings.ServerIP}:{m_settings.Port1}...");

                lock (_sync)
                {
                    //Για να αποφυγουμε τυχων race conditions κοιταμε να εχουν περασει 1500 ms απο την τελευταια αποσυνδεση μας
                    var _elapsedMS = _clock.Time.Subtract(this.LastDisconnectDT).TotalMilliseconds;
                    if (_elapsedMS < 1500)
                    {
                        theLogger.Info($"Instance is HOT. Last disconnection occured before {_elapsedMS} ms. Try again...");
                        return false;
                    }

                    /*
                     * Δημιουργουμε το Socket και κανει Connect
                     * Εαν λαβουμε EXCEPTION, τοτε το m_socket και το m_sslStream παραμενουν NULL
                     */
                    m_socket = SocketFactory.CreateClientSocket(m_settings);
                    if (m_settings.SSLEnable == true)
                    {
                        try
                        {
                            /*
                             * Θελουμε SSL tunnel, το φτιαχνουμε εδω...
                             * Εαν λαβουμε EXCEPTION, τοτε το m_socket δεν ειναι NULL ενω το m_sslStream παραμενει NULL
                             */
                            var _stream = new NetworkStream(m_socket, true);
                            m_sslStream = new SSLStreamFactory(m_settings).CreateClientStreamAndAuthenticate(_stream); //may throw exception...
                        }
                        catch
                        {
                            /*
                             * Κλείνουμε το socket αφου το SSL tunnel αποτυχε...
                             * Στην συνεχεια το  m_socket και το m_sslStream ειναι NULL
                             */
                            if (m_socket != null)
                            {
                                m_socket.Close();
                                m_socket = null;
                            }

                            throw;
                        }
                    }


                    //Κραταμε τι ωρα συνδεθηκαμε
                    LastConnectDT = _clock.Time;

                    //Και τωρα δημιουργουμε το νημα που διαβαζει συνεχως τα μηνυματα που ερχονται...
                    m_thread = new Thread(new ThreadStart(this._ReceiveLoop));
                    m_thread.Name = "_ReceiveLoop";
                    m_thread.Start();


                    MetricsProxy.Instance.OnTCPConnect();
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10061)
                {
                    //No connection could be made because the target machine actively refused it. 127.0.0.1:10450
                    theLogger.Warning(ex.Message);
                }
                else if (ex.ErrorCode == 10054)
                {
                    //An existing connection was forcibly closed by the remote host.
                    theLogger.Warning(ex.Message);
                }
                else
                {
                    MetricsProxy.Instance.OnTCPError();
                    theLogger.Error($"SocketException, ErrorCode={ex.ErrorCode}, Message'{ex.Message}'");
                }

                return false;
            }
            catch (AuthenticationException ex)
            {
                if (ex.InnerException is SocketException sockex)
                {
                    if (sockex.ErrorCode == 10061)
                    {
                        //No connection could be made because the target machine actively refused it. 127.0.0.1:10450
                        theLogger.Warning(ex.Message);
                    }
                    else if (sockex.ErrorCode == 10054)
                    {
                        //An existing connection was forcibly closed by the remote host.
                        theLogger.Warning(ex.Message);
                    }
                    else
                    {
                        MetricsProxy.Instance.OnTCPError();
                        theLogger.Error($"SocketException, ErrorCode={sockex.ErrorCode}, Message'{ex.Message}'");
                    }
                }
                else
                {
                    MetricsProxy.Instance.OnTCPError();
                    theLogger.Error(ex);
                }

                return false;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException sockex)
                {
                    if (sockex.ErrorCode == 10061)
                    {
                        //No connection could be made because the target machine actively refused it. 127.0.0.1:10450
                        theLogger.Warning(ex.Message);
                    }
                    else if (sockex.ErrorCode == 10054)
                    {
                        //An existing connection was forcibly closed by the remote host.
                        theLogger.Warning(ex.Message);
                    }
                    else
                    {
                        MetricsProxy.Instance.OnTCPError();
                        theLogger.Error($"SocketException, ErrorCode={sockex.ErrorCode}, Message'{ex.Message}'");
                    }
                }
                else
                {
                    MetricsProxy.Instance.OnTCPError();
                    theLogger.Error(ex);
                }

                return false;
            }


            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (m_sslStream != null)
                {
                    lock (_sync)
                    {
                        if (m_sslStream != null)
                        {
                            if (m_sslStream.CanWrite && m_sslStream.CanRead)
                                return true;
                        }
                    }
                }
                else if (m_socket != null)
                {
                    lock (_sync)
                    {
                        if (m_socket != null)
                        {
                            return m_socket.Connected;
                        }
                    }
                }
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            theLogger.Info($"Stop()");

            ThrowIfDisposed();

            try
            {

                _CloseAndInitialize();

            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                throw;
            }
        }
        #endregion




        /// <summary>
        /// Εδω συνεχως διαβαζουμε το socket, και οτι μας ερθει το στελνουμε στην assembleMessage
        /// Ενα FIX Message μπορει να ερθει σε κομματια ή μπορει να ερθουν πολλα στην σειρα με το ίδιο m_socket.Receive
        /// H assembleMessage, τα μαζευει και τα χωριζε...
        /// </summary>
        void _ReceiveLoop()
        {
            theLogger.Info("_ReceiveLoop Started");

            try
            {
                while (true)
                {
                    if (m_stopEvent.WaitOne(0))
                    {
                        theLogger.Info("m_stopEvent isSet!");
                        break;
                    }

                    int numOfBytes = 0;
                    if (m_sslStream != null)
                        numOfBytes = m_sslStream.Read(_rcvBuffer, 0, _rcvBuffer.Length);
                    else
                        numOfBytes = m_socket.Receive(_rcvBuffer, _rcvBuffer.Length, SocketFlags.None);

                    if (numOfBytes > 0)
                    {
                        _AssembleMessage(_rcvBuffer, numOfBytes);
                    }
                    else
                    {
                        theLogger.Warning("Received 0 bytes!");
                        if (m_sslStream != null)
                        {
                            break;
                        }
                    }

                    /*
                     * Το παρακατω λειτουργει και για απλο socket, και για
                     * SSL socket
                     */
                    if (SocketFactory.SocketIsConnected(m_socket) == false)
                    {
                        theLogger.Verbose($"SocketIsConnected == FALSE");
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10060)
                {   //KeepAlive failed
                    //A connection attempt failed because the connected party did not properly respond after a period of time, or established
                    //connection failed because connected host has failed to respond
                    MetricsProxy.Instance.OnTCPError();
                    theLogger.Error(string.Format("_ReceiveLoop() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));

                }
                else if (ex.ErrorCode == 10054)
                {
                    //An existing connection was forcibly closed by the remote host
                    MetricsProxy.Instance.OnTCPWarning();
                    theLogger.Warning(string.Format("_ReceiveLoop() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));
                }
                else if (ex.ErrorCode == 10053)
                {
                    //DISCONNECT
                    //An existing connection was forcibly closed by the remote host
                    //ή
                    //An established connection was aborted by the software in your host machine.
                    theLogger.Info(string.Format("_ReceiveLoop() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));
                }
                else if (ex.ErrorCode == 10004)
                {
                    //DISCONNECT
                    //A blocking operation was interrupted by a call to WSACancelBlockingCall.
                    MetricsProxy.Instance.OnTCPWarning();
                    theLogger.Warning(string.Format("_ReceiveLoop() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));
                }
                else
                {
                    MetricsProxy.Instance.OnTCPError();
                    theLogger.Error(string.Format("_ReceiveLoop() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));
                }
            }
            catch (ObjectDisposedException ex)
            {
                /*
                 * Το πιθανοτερο ειναι καποιο αλλο νήμα να μας εκλεισε το socket, γιατι κληθηκε η Stop() μας....
                 */
                MetricsProxy.Instance.OnTCPWarning();
                theLogger.Warning(string.Format("_ReceiveLoop() -> {0}", ex.Message));
            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnTCPError();
                theLogger.Error(string.Format("_ReceiveLoop() -> {0}", ex.Message));
            }

            Disconnect(invokeEvent: true, errorcode: 0);
            theLogger.Info("_ReceiveLoop Terminated");
        }

        /// <summary>
        /// Εδω μας ερχονται τα bytes που λαβαμε απο το socket, και τα αντιγραφουμε στο fmsg μεχρι να βρουμε το τελος του μηνύματος.
        /// Μολις εχουμε στο fmsg ενα ολοκληρο Fix Message τότε καλουμε την dispatchMessage
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="numOfBytes"></param>
        void _AssembleMessage(byte[] buffer, int numOfBytes)
        {
            for (int idx = 0; idx < numOfBytes; idx++)
            {
                if (++_asmIdx >= _asmBuffer.Length)
                {
                    throw new PtFixException($"ASSEMBLY BUFFER OVERFLOW, current capacity is {_asmBuffer.Length} bytes. Must increase TCPConnection.MaxRcvBuffer (={m_settings.MaxRcvBuffer} * 2)");
                }

                _asmBuffer[_asmIdx] = buffer[idx];



                if (_asmIdx == 11 /*εχουμε λαβει 12 bytes, -> μπορουμε να καταλαβουμε εαν εχουμε ενα σωστο FIX HEADER*/)
                {
                    #region _IsFixMessage
                    if (_IsFixMessage(_asmBuffer) == false)
                    {
                        var sb = new StringBuilder($"GarbledMessage:: Message doesn't start correctly; IncomingBytes: [");
                        for (int jb = 0; jb <= _asmIdx; jb++)
                        {
                            if (jb > 0)
                                sb.Append(",");
                            sb.AppendFormat("0x{0:X}", _asmBuffer[jb]);
                        }
                        sb.Append("]");

                        throw new PtFixException(sb.ToString());
                    }
                    #endregion
                }

                if (_asmIdx >= 54 /*εχουμε λαβει ηδη 55 bytes*/  && _asmBuffer[_asmIdx] == SOH /* και το τελευταιο byte ειναι SOH*/)
                {
                    /*
                     * Εαν εχουμε λαβει ηδη 55 bytes (το μικροτερο valid fix message ειναι γυρω στα 62 bytes τουλαχιστον)
                     * και το τελευταιο byte που λαβαμε ειναι ένα SOH (δλδ μολις τερματισαμε ενα FIX FILED)
                     * τοτε ελεγχουμε μηπως εχουμε ένα πλήρες FIX Message στα χερια μας....
                     */
                    if (_IsMessageCompleted(_asmBuffer, length: _asmIdx))
                    {
                        if (m_stopEvent.WaitOne(0))
                        {
                            break;
                        }

                        _DispatchMessage(_asmBuffer, _asmIdx + 1);

                        _asmIdx = -1;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tbuffer"></param>
        /// <param name="numOfBytes"></param>
        void _DispatchMessage(byte[] tbuffer, int numOfBytes)
        {
            m_inbound.Clear();
            m_inbound.Parse(tbuffer, 0, numOfBytes, theLogger);

            if (m_inbound.Valid == false)
            {
                throw new PtFixException($"GarbledMessage:: INVALID_MESSAGE");
            }

            try
            {
                MetricsProxy.Instance.OnTCPIncomingMessage(numOfBytes);
                OnNewMessage?.Invoke(m_inbound);
            }
            catch (Exception ex)
            {
                theLogger.Error($"_DispatchMessage: {ex.Message}");
            }
        }

        /// <summary>
        /// ελεγχουμε τα πρωτα 12 bytes....
        /// κοιταμε το incoming μηνυμα να ξεκιναει με
        ///         ΄8=FIX.4.XSOH9='΄
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        static bool _IsFixMessage(byte[] buffer)
        {
            if (buffer[0] != 0x38) return false;//8
            if (buffer[1] != 0x3d) return false;//=
            if (buffer[2] != 0x46) return false;//F
            if (buffer[3] != 0x49) return false;//I
            if (buffer[4] != 0x58) return false;//X
            if (buffer[5] != 0x2e) return false;//.
            if (buffer[6] != 0x34) return false;//4
            if (buffer[7] != 0x2e) return false;//.
            //if (buffer[8] != 0x34) return false;//4
            if (buffer[9] != SOH) return false;//SOH
            if (buffer[10] != 0x39) return false;//9
            if (buffer[11] != 0x3d) return false;//=

            return true;
        }
        /*
         * Ενα FIXMessage τερματίζει με την εξης αλληλουχία
         *              ^10=XXX^
         *  To τελευταιο tag είναι το tag10 (CheckSum), ακολουθουν 3 bytes και μετα ακολουθει το SOH
         *  Αρα ψαχνουμε στα τελευταια 8 bytes που λαβαμε να βρουμε αυτη την αλληλουχία
         */
        static bool _IsMessageCompleted(byte[] buffer, int length)
        {
            //Console.WriteLine("_IsMessageCompleted: {0}", Encoding.ASCII.GetString(buffer, length - 8, 8));

            //if (buffer[length] != SOH) return false;  //το ελεγξαμε ηδη πριν ερθουμε εδω
            if (buffer[length - 4] != '=') return false;
            if (buffer[length - 5] != '0') return false;
            if (buffer[length - 6] != '1') return false;
            if (buffer[length - 7] != SOH) return false;

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer, int offset, int size)
        {
            ThrowIfDisposed();

            try
            {
                if (m_sslStream != null || m_socket != null)
                {
                    lock (_sync)
                    {
                        if (m_sslStream != null || m_socket != null)
                        {
                            if (m_sslStream != null)
                                m_sslStream.Write(buffer, offset, size);
                            else
                                m_socket.Send(buffer, offset, size, SocketFlags.None);

                            MetricsProxy.Instance.OnTCPOutcomingMessage(size);
                            return true;
                        }
                    }
                }

                theLogger.Error("Send() -> m_socket/m_sslStream is null");
                return false;
            }
            catch (SocketException ex)
            {
                MetricsProxy.Instance.OnTCPError();
                theLogger.Error(string.Format("Send() -> ErrorCode= {0}, Message = {1}", ex.ErrorCode, ex.Message));
            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnTCPError();
                theLogger.Error(string.Format("Send() -> {0}", ex.Message));
            }

            Disconnect(true);
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="invokeEvent"></param>
        /// <param name="errorcode"></param>
        void Disconnect(bool invokeEvent = false, int errorcode = 0)
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Disconnect() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
            else
                theLogger.Verbose($"Disconnect() called by '{Thread.CurrentThread.Name}'.");

            if (m_sslStream != null || m_socket != null)
            {
                lock (_sync)
                {
                    if (m_sslStream != null || m_socket != null)
                    {
                        LastDisconnectDT = _clock.Time;

                        try
                        {
                            if (m_sslStream != null)
                            {
                                /* https://github.com/dotnet/standard/issues/598
                                 * You can still "cleanly" close the TLS/SSL stream by simply doing a 'Dispose' of the 
                                 * SslStream object. Disposing the SslStream will also close the underlying NetworkStream 
                                 * (TCP) as well unless the SslStream object was created to not own the inner stream.
                                 * As long as you are calling Dispose, it should work fine and end up with a TCP FIN 
                                 * to the other side indicating a clean close of the TCP channel
                                 */
                                m_sslStream.Dispose();
                            }
                            else
                            {
                                try
                                {
                                    /*
                                     * When using a connection-oriented Socket, always call the 
                                     * Shutdown method before closing the Socket. This ensures 
                                     * that all data is sent and received on the connected socket 
                                     * before it is closed.
                                     */
                                    m_socket.Shutdown(SocketShutdown.Both);
                                }
                                catch (SocketException ex)
                                {
                                    theLogger.Warning($"At m_socket.Shutdown(), SocketException with ErrorCode={ex.ErrorCode}, Message='{ex.Message},'!");
                                }
                                catch (Exception ex)
                                {
                                    theLogger.Warning($"At m_socket.Shutdown(), Exception with Message='{ex.Message}'!");
                                }
                                finally
                                {
                                    m_socket.Close();
                                }
                            }
                        }
                        catch (SocketException ex)
                        {
                            theLogger.Warning($"In Disconnect(), SocketException with ErrorCode={ex.ErrorCode}, Message='{ex.Message},'!");
                        }
                        catch (Exception ex)
                        {
                            theLogger.Warning($"In Disconnect(), Exception with Message='{ex.Message}'!");
                        }


                        m_socket = null;
                        m_sslStream = null;

                        MetricsProxy.Instance.OnTCPDisconnect();

                        if (invokeEvent) OnDisconnect?.Invoke(errorcode);

                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Disconnected..... by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
                        else
                            theLogger.Verbose($"Disconnected..... by '{Thread.CurrentThread.Name}'.");
                    }
                }
            }
        }
    }
}
