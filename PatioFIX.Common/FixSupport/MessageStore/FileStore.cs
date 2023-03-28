using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileStore : IMessageStore
    {
        bool _disposedValue;

        class MsgDef
        {
            public long offset { get; private set; }
            public int size { get; private set; }

            public MsgDef(long offset, int size)
            {
                this.offset = offset;
                this.size = size;
            }
        }

        class OtherState
        {
            public int CountOfConnections;
            public int CountOfFailedConnections;
            public DateTime LastConnectionTime;
            public DateTime LastDisconnectionTime;
            public DateTime InboundTimestamp;
            public DateTime OutboundTimestamp;
        }


        string m_seqNumsFileName;
        string m_outMsgFileName;
        string m_outIndexFileName;
        string m_inMsgFileName;
        string m_inIndexFileName;
        string m_stateFileName;

        System.IO.FileStream m_seqNumsFile;

        System.IO.FileStream m_outMsgFile;
        System.IO.StreamWriter m_outIndexFile;
        Dictionary<int, MsgDef> m_outOffsets = new Dictionary<int, MsgDef>();
        int m_maxOutMessageSize = 0;


        System.IO.FileStream m_inMsgFile;
        System.IO.StreamWriter m_inIndexFile;

        int m_nextOutMsgSeqNum;
        int m_nextInMsgSeqNum;
        DateTime m_creationTime;
        readonly OtherState m_otherState = new OtherState();
        readonly IClock m_clock = new RealTimeClock();
        readonly Logger theLogger = new Logger("FileStore");

        string m_path;
        SessionId m_sessionId;


        public static string Prefix(SessionId sessionId)
        {
            return "session_" + sessionId.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sessionId"></param>
        public FileStore(string path, SessionId sessionId)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));
            if (sessionId is null)
                throw new ArgumentException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(sessionId.Id))
                throw new ArgumentException("sessionId.Id");

            theLogger.Info($".ctor, session = '{sessionId.Id}'");

            m_path = path;
            m_sessionId = sessionId;

            PrepareFileNames();
            Initialize();
        }

        /// <summary>
        /// Δημιουργεί χύμα τα αρχεία στο root του path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sessionId"></param>
        void PrepareFileNamesSimple()
        {
            if (!System.IO.Directory.Exists(m_path))
                System.IO.Directory.CreateDirectory(m_path);


            //Δημιουργουμε τα ονοματα των αρχειων
            string prefix = Prefix(m_sessionId);
            m_seqNumsFileName = System.IO.Path.Combine(m_path, prefix + "_session.seqnums");

            m_outMsgFileName = System.IO.Path.Combine(m_path, prefix + "_outbound.dat");
            m_outIndexFileName = System.IO.Path.Combine(m_path, prefix + "_outbound.index");

            m_inMsgFileName = System.IO.Path.Combine(m_path, prefix + "_inbound.dat");
            m_inIndexFileName = System.IO.Path.Combine(m_path, prefix + "_inbound.index");

            m_stateFileName = System.IO.Path.Combine(m_path, prefix + "_session.state");
        }
        /// <summary>
        /// Δημιουργεί ένα directory και εκει δημιουργεί τα αρχεία
        /// </summary>
        void PrepareFileNames()
        {

            var _dt = DateTime.Now.ToString("yyyyMMdd");
            var path = Path.Combine(m_path, _dt + "_" + m_sessionId.Id);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            theLogger.Info($"PrepareFileNames() -> path = '{path}'");

            //Δημιουργουμε τα ονοματα των αρχειων
            m_seqNumsFileName = System.IO.Path.Combine(path, "session.seqnums");

            m_outMsgFileName = System.IO.Path.Combine(path, "outbound.dat");
            m_outIndexFileName = System.IO.Path.Combine(path, "outbound.index");

            m_inMsgFileName = System.IO.Path.Combine(path, "inbound.dat");
            m_inIndexFileName = System.IO.Path.Combine(path, "inbound.index");

            m_stateFileName = System.IO.Path.Combine(path, "session.state");
        }

        void Initialize()
        {
            theLogger.Info($"Initialize()");
            CloseFiles();

            m_nextOutMsgSeqNum = 1;
            m_nextInMsgSeqNum = 1;
            m_creationTime = m_clock.Time;
            m_otherState.CountOfConnections = 0;
            m_otherState.CountOfFailedConnections = 0;
            m_otherState.InboundTimestamp = DateTime.MinValue;
            m_otherState.OutboundTimestamp = DateTime.MinValue;
            m_otherState.LastConnectionTime = DateTime.MinValue;
            m_otherState.LastDisconnectionTime = DateTime.MinValue;


            ConstructFromFileCache();
            InitializeSessionCreateTime();

            m_seqNumsFile = new System.IO.FileStream(m_seqNumsFileName, System.IO.FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.RandomAccess);

            m_outMsgFile = new System.IO.FileStream(m_outMsgFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.RandomAccess);
            m_outIndexFile = new System.IO.StreamWriter(m_outIndexFileName, true, CharEncoding.DefaultEncoding);

            m_inMsgFile = new System.IO.FileStream(m_inMsgFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.RandomAccess);
            m_inIndexFile = new System.IO.StreamWriter(m_inIndexFileName, true, CharEncoding.DefaultEncoding);
        }

        void CloseFiles()
        {

            theLogger.Info("CloseFiles()");
            if (m_seqNumsFile != null)
            {
                m_seqNumsFile.Flush(true);
                m_seqNumsFile.Dispose();
                m_seqNumsFile = null;
            }
            if (m_outMsgFile != null)
            {
                m_outMsgFile.Flush(true);
                m_outMsgFile.Dispose();
                m_outMsgFile = null;
            }
            if (m_outIndexFile != null)
            {
                m_outIndexFile.Flush();
                m_outIndexFile.Dispose();
                m_outIndexFile = null;
            }

            if (m_inMsgFile != null)
            {
                m_inMsgFile.Flush(true);
                m_inMsgFile.Dispose();
                m_inMsgFile = null;
            }
            if (m_inIndexFile != null)
            {
                m_inIndexFile.Flush();
                m_inIndexFile.Dispose();
                m_inIndexFile = null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (!_disposedValue)
            {
                CloseFiles();
                theLogger?.Info("Dispose()");
                _disposedValue = true;
            }
        }

        public void Refresh()
        {
            Initialize();
        }


        public void Reset()
        {
            PurgeFileCache();

            PrepareFileNames();
            Initialize();
        }




        private void DeleteSingleFile(string filename)
        {
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);
        }

        private void PurgeFileCache()
        {
            CloseFiles();

            DeleteSingleFile(m_seqNumsFileName);
            DeleteSingleFile(m_outMsgFileName);
            DeleteSingleFile(m_outIndexFileName);
            DeleteSingleFile(m_inMsgFileName);
            DeleteSingleFile(m_inIndexFileName);
            DeleteSingleFile(m_stateFileName);
        }


        void ConstructFromFileCache()
        {
            m_outOffsets.Clear();
            if (System.IO.File.Exists(m_outIndexFileName))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(m_outIndexFileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] headerParts = line.Split(',');
                        if (headerParts.Length == 3)
                        {
                            var msgSeqNum = Convert.ToInt32(headerParts[0]);
                            var offset = Convert.ToInt64(headerParts[1]);
                            var size = Convert.ToInt32(headerParts[2]);

                            m_outOffsets[msgSeqNum] = new MsgDef(offset, size);

                            if (m_maxOutMessageSize < size)
                                m_maxOutMessageSize = size;
                        }
                    }
                }
            }

            if (System.IO.File.Exists(m_seqNumsFileName))
            {
                using (System.IO.StreamReader seqNumReader = new System.IO.StreamReader(m_seqNumsFileName))
                {
                    string[] parts = seqNumReader.ReadToEnd().Split(':');
                    if (parts.Length == 2)
                    {
                        m_nextOutMsgSeqNum = Convert.ToInt32(parts[0]);
                        m_nextInMsgSeqNum = Convert.ToInt32(parts[1]);
                    }
                }
            }
        }

        void InitializeSessionCreateTime()
        {
            if (System.IO.File.Exists(m_stateFileName) && new System.IO.FileInfo(m_stateFileName).Length > 0)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(m_stateFileName))
                {
                    string s = reader.ReadToEnd();
                    m_creationTime = UtcDateTimeSerializer.FromString(s);
                }
            }
            else
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(m_stateFileName, false))
                {
                    writer.Write(UtcDateTimeSerializer.ToString(m_creationTime));
                }
            }
        }


        void saveSeqNums()
        {
            m_seqNumsFile.Seek(0, System.IO.SeekOrigin.Begin);

            var data = m_nextOutMsgSeqNum.ToString("D10") + ":" + m_nextInMsgSeqNum.ToString("D10");
            var bytes = CharEncoding.DefaultEncoding.GetBytes(data);

            m_seqNumsFile.Write(bytes, 0, bytes.Length);
            m_seqNumsFile.Flush();
        }
        void saveOtherState()
        {

        }

        #region IMessageStore

        /// <summary>
        /// Get messages within the range of sequence numbers
        /// </summary>
        /// <param name="startSeqNum"></param>
        /// <param name="endSeqNum"></param>
        /// <returns></returns>
        public IList<string> GetOutbound(int startSeqNum, int endSeqNum)
        {
            var messages = new List<string>();
            byte[] msgBytes = new byte[m_maxOutMessageSize + 10];


            for (int i = startSeqNum; i <= endSeqNum; i++)
            {
                if (m_outOffsets.ContainsKey(i))
                {
                    m_outMsgFile.Seek(m_outOffsets[i].offset, System.IO.SeekOrigin.Begin);
                    m_outMsgFile.Read(msgBytes, 0, m_outOffsets[i].size);

                    messages.Add(CharEncoding.DefaultEncoding.GetString(msgBytes, 0, m_outOffsets[i].size));
                }
            }
            return messages;
        }

        /// <summary>
        /// Logs an outbound message
        /// </summary>
        /// <param name="msgSeqNum"></param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SaveOutbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            var offset = m_outMsgFile.Seek(0, System.IO.SeekOrigin.End);

            m_outIndexFile.Write(msgSeqNum.ToString(CultureInfo.InvariantCulture));
            m_outIndexFile.Write(",");
            m_outIndexFile.Write(offset.ToString(CultureInfo.InvariantCulture));
            m_outIndexFile.Write(",");
            m_outIndexFile.WriteLine(size.ToString(CultureInfo.InvariantCulture));

            m_outIndexFile.Flush();

            m_outOffsets[msgSeqNum] = new MsgDef(offset, size);

            m_outMsgFile.Write(msgBytes, 0, size);
            m_outMsgFile.Flush();

            if (m_maxOutMessageSize < size)
                m_maxOutMessageSize = size;

            return true;
        }

        /// <summary>
        /// Logs an inbound message
        /// </summary>
        /// <param name="msgSeqNum"></param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SaveInbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            long offset = m_inMsgFile.Seek(0, System.IO.SeekOrigin.End);

            m_inIndexFile.Write(msgSeqNum.ToString());
            m_inIndexFile.Write(",");
            m_inIndexFile.Write(offset.ToString());
            m_inIndexFile.Write(",");
            m_inIndexFile.WriteLine(size.ToString());

            m_inIndexFile.Flush();

            //m_offsets[msgSeqNum] = new MsgDef(offset, size);

            m_inMsgFile.Write(msgBytes, 0, size);
            m_inMsgFile.Flush();

            //if (m_maxMessageSize < size)
            //    m_maxMessageSize = size;

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        public int NextOutboundSeqNum
        {
            get
            {
                return m_nextOutMsgSeqNum;
            }
            set
            {
                m_nextOutMsgSeqNum = value;
                saveSeqNums();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int NextInboundSeqNum
        {
            get
            {
                return m_nextInMsgSeqNum;
            }
            set
            {
                m_nextInMsgSeqNum = value;
                saveSeqNums();
            }
        }


        public DateTime CreationTime => m_creationTime;
        public int CountOfConnections
        {
            get
            {
                return m_otherState.CountOfConnections;
            }
            set
            {
                m_otherState.CountOfConnections = value;
                saveOtherState();
            }
        }
        public int CountOfFailedConnections
        {
            get
            {
                return m_otherState.CountOfFailedConnections;
            }
            set
            {
                m_otherState.CountOfFailedConnections = value;
                saveOtherState();
            }
        }
        public DateTime LastConnectionTime
        {
            get
            {
                return m_otherState.LastConnectionTime;
            }
            set
            {
                m_otherState.LastConnectionTime = value;
                saveOtherState();
            }
        }
        public DateTime LastDisconnectionTime
        {
            get
            {
                return m_otherState.LastDisconnectionTime;
            }
            set
            {
                m_otherState.LastDisconnectionTime = value;
                saveOtherState();
            }
        }
        public DateTime InboundTimestamp
        {
            get
            {
                return m_otherState.InboundTimestamp;
            }
            set
            {
                m_otherState.InboundTimestamp = value;
                saveOtherState();
            }
        }
        public DateTime OutboundTimestamp
        {
            get
            {
                return m_otherState.OutboundTimestamp;
            }
            set
            {
                m_otherState.OutboundTimestamp = value;
                saveOtherState();
            }
        }
        #endregion

    }
}
