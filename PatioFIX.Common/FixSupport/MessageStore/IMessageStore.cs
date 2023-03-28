using System;
using System.Collections.Generic;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// Used by a Session to store and retrieve messages for resend purposes
    /// </summary>
    public interface IMessageStore : IDisposable
    {
        /// <summary>
        /// Get outbound messages within sequence number range (inclusive). Used for
        /// message resend requests
        /// </summary>
        /// <param name="startSeqNum">the starting message sequence number</param>
        /// <param name="endSeqNum">the ending message sequence number</param>
        /// <returns></returns>
        IList<string> GetOutbound(int startSeqNum, int endSeqNum);


        /// <summary>
        /// Logs an outbound message
        /// </summary>
        /// <param name="msgSeqNum">the sequence number</param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool SaveOutbound(int msgSeqNum, byte[] msgBytes, int size);


        /// <summary>
        /// Logs an inbound message
        /// </summary>
        /// <param name="msgSeqNum">the sequence number</param>
        /// <param name="msgBytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool SaveInbound(int msgSeqNum, byte[] msgBytes, int size);

        /// <summary>
        /// Επομενο SequenceNumber για ΕΞΕΡΧΟΜΕΝΟ μηνυμα
        /// </summary>
        int NextOutboundSeqNum { get; set; }
        /// <summary>
        /// Επομενο (αναμενομενο) SequenceNumber για ΕΙΣΕΡΧΟΜΕΝΟ μηνυμα
        /// </summary>
        int NextInboundSeqNum { get; set; }



        System.DateTime CreationTime { get; }
        System.Int32 CountOfConnections { get; set; }
        System.Int32 CountOfFailedConnections { get; set; }
        System.DateTime LastConnectionTime { get; set; }
        System.DateTime LastDisconnectionTime { get; set; }
        System.DateTime InboundTimestamp { get; set; }
        System.DateTime OutboundTimestamp { get; set; }


        /// <summary>
        /// Reset the message store. Sequence numbers are set back to 1 and stored
        /// messages are erased. The session creation time is also set to the time of
        /// the reset
        /// </summary>
        void Reset();

        /// <summary>
        /// Refreshes session state from a shared state storage (e.g. database,
        /// file, ...). Refresh will not work for message stores without shared state
        /// (e.g. MemoryStore). These stores should log a session error, at a minimum,
        /// or throw an exception.
        /// </summary>
        void Refresh();
    }
}
