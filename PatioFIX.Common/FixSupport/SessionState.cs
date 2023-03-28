using PatioFIX.Common.FixSupport.Transport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class SessionState : IDisposable
    {
        int _disposed = 0;// Whether Dispose has been called.

        readonly Logger theLogger = new Logger("SessionState");
        readonly StringBuilder m_info = new StringBuilder();
        readonly FixConfiguration m_settings;
        readonly SessionId m_sessionId;
        readonly TCPConnection m_connector;
        IMessageStore m_messageStore;
        readonly Object m_sync = new object();


        /// <summary>
        /// Heartbeat interval (milliseconds)
        /// </summary>
        public double HeartbeatInterval;
        /// <summary>
        /// TestRequest interval (milliseconds)
        /// </summary>
        public double HeartbeatTimeoutMin;
        /// <summary>
        /// DropConnection interval (milliseconds)
        /// </summary>
        public double HeartbeatTimeoutMax;


        #region MessageStore
        public IList<string> GetOutbound(int startSeqNum, int endSeqNum)
        {
            lock (m_sync)
            {
                return m_messageStore.GetOutbound(startSeqNum, endSeqNum);
            }
        }
        public bool SaveOutbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            if (m_settings.LogOutboundMessages)
            {
                try
                {
                    lock (m_sync)
                    {
                        m_messageStore.SaveOutbound(msgSeqNum, msgBytes, size);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    theLogger.Warning($"SaveInbound -> {ex.Message}");
                }
                return false;
            }
            return true;
        }
        public bool SaveInbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            if (m_settings.LogInboundMessages)
            {
                try
                {
                    lock (m_sync)
                    {
                        m_messageStore.SaveInbound(msgSeqNum, msgBytes, size);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    theLogger.Warning($"SaveInbound -> {ex.Message}");
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Επομενο SequenceNumber για ΕΞΕΡΧΟΜΕΝΟ μηνυμα
        /// </summary>
        public int NextOutboundSeqNum
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.NextOutboundSeqNum;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.NextOutboundSeqNum = value;
                }
            }
        }
        /// <summary>
        /// Επομενο (αναμενομενο) SequenceNumber για ΕΙΣΕΡΧΟΜΕΝΟ μηνυμα
        /// </summary>
        public int NextInboundSeqNum
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.NextInboundSeqNum;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.NextInboundSeqNum = value;
                }
            }
        }

        /// <summary>
        /// Ποσες φορες προσπαθησαμε να συνδεθουμε (m_connector.Connect()) στον FIX Server
        /// </summary>
        public int CountOfConnections
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.CountOfConnections;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.CountOfConnections = value;
                }
            }
        }
        /// <summary>
        /// Ποσες φορες αποτυχε η m_connector.Connect()
        /// </summary>
        public int CountOfFailedConnections
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.CountOfFailedConnections;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.CountOfFailedConnections = value;
                }
            }
        }
        /// <summary>
        /// Ποτε κληθηκε τελευταια φορα η m_connector.Connect()
        /// </summary>
        public DateTime LastConnectionTime
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.LastConnectionTime;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.LastConnectionTime = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastDisconnectionTime
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.LastDisconnectionTime;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.LastDisconnectionTime = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetMessageStore()
        {
            lock (m_sync)
            {
                m_messageStore.Reset();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshMessageStore()
        {
            lock (m_sync)
            {
                m_messageStore.Refresh();
            }
        }

        public DateTime CreationTime => m_messageStore.CreationTime;

        /// <summary>
        /// Τελευταια φορα που λαβαμε message
        /// </summary>
        public DateTime InboundTimestamp
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.InboundTimestamp;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.InboundTimestamp = value;
                }
            }
        }
        /// <summary>
        /// Τελευταια φορα που στειλαμε message
        /// </summary>
        public DateTime OutboundTimestamp
        {
            get
            {
                lock (m_sync)
                {
                    return m_messageStore.OutboundTimestamp;
                }
            }
            set
            {
                lock (m_sync)
                {
                    m_messageStore.OutboundTimestamp = value;
                }
            }
        }
        #endregion


        #region State
        public bool SentLogon { get; set; }
        public bool ReceivedLogon { get; set; }
        public bool SentLogout { get; set; }
        public bool ReceivedLogout { get; set; }
        public bool IsLoggedOn { get { return ReceivedLogon && SentLogon; } }

        /// <summary>
        /// ΛΑμβανουμε απο τον FIX Server αποθηκευμενα μηνυματα
        /// σαν απαντηση σε 'Resend Request (2)' που στειλαμε
        /// </summary>
        public bool Synchronizing;
        public bool Synchronizing_Pending;
        public int Synchronizing_from_inboundSeqNum;
        public int Synchronizing_to_inboundSeqNum;
        public DateTime Synchronizing_StartTime;

        /// <summary>
        /// Στελνουμε στον FIX Server αποθηκευμενα μηνυματα
        /// σαν απαντηση σε 'Resend Request (2)' που λαβαμε
        /// </summary>
        public bool Server_Synchronizing;
        public DateTime Server_Synchronizing_StartTime;

        /// <summary>
        /// Εχουμε στειλει 'Test Request (1)' και περιμενουμε 
        /// απαντηση
        /// </summary>
        public bool TestRequestPending;

        /// <summary>
        /// Κλεινουμε (drop) την συνδεση μας με τον FIX Server
        /// </summary>
        public bool ForceStopInProcess;

        /// <summary>
        /// 
        /// </summary>
        public void ResetState()
        {
            this.SentLogon = false;
            this.ReceivedLogon = false;
            this.SentLogout = false;
            this.ReceivedLogout = false;



            this.Synchronizing = false;
            this.Synchronizing_Pending = false;
            this.Synchronizing_from_inboundSeqNum = 0;
            this.Synchronizing_to_inboundSeqNum = 0;
            this.Synchronizing_StartTime = DateTime.MinValue;

            this.Server_Synchronizing = false;
            this.Server_Synchronizing_StartTime = DateTime.MinValue;

            this.TestRequestPending = false;

            this.ForceStopInProcess = false;

            MetricsProxy.Instance.OnFIXClientState(this);
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        internal SessionState()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_connector"></param>
        /// <param name="settings"></param>
        /// <param name="sessionsRootPath"></param>
        /// <param name="sessionId"></param>
        public SessionState(TCPConnection connector, FixConfiguration settings, string sessionsRootPath, SessionId sessionId)
        {
            theLogger.Info($".ctor, session = '{sessionId.Id}'");

            m_sessionId = sessionId;
            m_settings = settings;
            m_connector = connector;
            m_messageStore = new FileStore(sessionsRootPath, sessionId);

            HeartbeatInterval = (settings.HeartbeatInterval * 1.0) * 1000;
            HeartbeatTimeoutMin = (settings.HeartbeatInterval * 1.3) * 1000;
            HeartbeatTimeoutMax = (settings.HeartbeatInterval * 2.0) * 1000;

            //CreationTime  -> απο το IMessageStore
            //NextInboundSeqNum  -> απο το IMessageStore
            //NextOutboundSeqNum  -> απο το IMessageStore

            //CountOfConnections  -> απο το IMessageStore
            //CountOfFailedConnections  -> απο το IMessageStore

            //InboundTimestamp  -> απο το IMessageStore
            //OutboundTimestamp  -> απο το IMessageStore
            //LastConnectionTime  -> απο το IMessageStore


            ResetState();
        }

        /// <summary>
        /// 
        /// </summary>
        ~SessionState() => Dispose(false);

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
        public void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }


            if (m_messageStore != null)
            {
                m_messageStore.Dispose();
                m_messageStore = null;
            }

            theLogger?.Info("Dispose()");

            _disposed = 1;
        }


        /// <summary>
        /// 
        /// </summary>
        public void ShowInfo()
        {
            m_info.Clear();

            if (this.ForceStopInProcess)
            {
                m_info.Append("FORCESTOP, ");
            }
            if (m_connector.IsConnected)
            {
                m_info.Append("CONN, ");
            }
            else
            {
                m_info.Append("NOT_CONN, ");
            }

            m_info.AppendFormat("LOGON({0}, {1}), ", this.SentLogon, this.ReceivedLogon);

            if (this.Synchronizing)
            {
                m_info.Append("SYNCH, ");
            }
            if (this.Server_Synchronizing)
            {
                m_info.Append("SERVER_SYNCH, ");
            }
            if (this.TestRequestPending)
            {
                m_info.Append("TESTREQUEST, ");
            }

            m_info.AppendFormat("LOGOUT({0}, {1}), ", this.SentLogout, this.ReceivedLogout);
            m_info.Append($"In({this.NextInboundSeqNum}, {this.InboundTimestamp.TimeOfDay}), ");
            m_info.Append($"Out({this.NextOutboundSeqNum}, {this.OutboundTimestamp.TimeOfDay}), ");

            m_info.Append($"CreationTime={CreationTime.TimeOfDay}");

            theLogger.Info(m_info.ToString());
        }

    }
}
