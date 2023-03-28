using System;
using System.Collections.Generic;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// In-memory message store implementation
    /// </summary>
    public class MemoryStore : IMessageStore
    {
        Dictionary<int, byte[]> m_outMessages;
        DateTime m_creationTime;


        /// <summary>
        /// 
        /// </summary>
        public MemoryStore()
        {
            m_outMessages = new Dictionary<int, byte[]>();
            Reset();
        }


        public void Dispose()
        {

        }

        #region MessageStore Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="begSeqNo"></param>
        /// <param name="endSeqNo"></param>
        /// <returns></returns>
        public IList<string> GetOutbound(int begSeqNo, int endSeqNo)
        {
            var messages = new List<string>();
            for (int current = begSeqNo; current <= endSeqNo; current++)
            {
                if (m_outMessages.ContainsKey(current))
                {
                    messages.Add(CharEncoding.DefaultEncoding.GetString(m_outMessages[current]));
                }
            }
            return messages;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgSeqNum"></param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SaveOutbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            m_outMessages[msgSeqNum] = msgBytes;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgSeqNum"></param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SaveInbound(int msgSeqNum, byte[] msgBytes, int size)
        {
            return true;
        }

        public int NextOutboundSeqNum { get; set; }

        public int NextInboundSeqNum { get; set; }



        public DateTime CreationTime => m_creationTime;
        public int CountOfConnections { get; set; }
        public int CountOfFailedConnections { get; set; }
        public DateTime LastConnectionTime { get; set; }
        public DateTime LastDisconnectionTime { get; set; }
        public DateTime InboundTimestamp { get; set; }
        public DateTime OutboundTimestamp { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            NextOutboundSeqNum = 1;
            NextInboundSeqNum = 1;
            m_outMessages.Clear();
            m_creationTime = DateTime.UtcNow;

            InboundTimestamp = DateTime.MinValue;
            OutboundTimestamp = DateTime.MinValue;
            LastConnectionTime = DateTime.MinValue;
            LastDisconnectionTime = DateTime.MinValue;
            CountOfConnections = 0;
            CountOfFailedConnections = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Refresh()
        { }


        #endregion
    }
}
