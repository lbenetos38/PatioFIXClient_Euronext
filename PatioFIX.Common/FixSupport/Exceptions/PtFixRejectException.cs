using System;
using System.Runtime.Serialization;
using System.Security;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PtFixRejectException : PtFixException
    {
        /// <summary>
        /// Text <58> field
        /// Where possible, message to explain reason for rejection
        /// </summary>
        public string RejectText { get; }

        /// <summary>
        /// RefSeqNum <45> field
        /// Reference message sequence number
        /// </summary>
        public int RefSeqNum { get; }

        /// <summary>
        /// RefTagID <371> field
        /// The tag number of the FIX field being referenced.
        /// </summary>
        public int RefTagID { get; set; }

        /// <summary>
        /// RefMsgType <372> field
        /// The MsgType <35> of the FIX message being referenced. 
        /// </summary>
        public string RefMsgType { get; set; }

        /// <summary>
        ///  SessionRejectReason <373> field
        ///  Code to identify reason for a session-level Reject <3> message. 
        /// </summary>
        public SessionRejectReason RejectReason { get; set; } = SessionRejectReason.NotSet;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="refSeqNum"></param>
        /// <param name="rejectText"></param>
        public PtFixRejectException(int refSeqNum, string rejectText)
            : base(rejectText)
        {
            this.RefSeqNum = refSeqNum;
            this.RejectText = rejectText;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="refSeqNum"></param>
        /// <param name="rejectText"></param>
        /// <param name="innerException"></param>
        public PtFixRejectException(int refSeqNum, string rejectText, Exception innerException)
            : base(rejectText, innerException)
        {
            this.RefSeqNum = refSeqNum;
            this.RejectText = rejectText;
        }



        /// <summary>
        /// Initializes a new instance of the Patio.Core.PtException class with serialized data.
        /// <para>This protected constructor is used for deserialization and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context"> The contextual information about the source or destination.</param>
        [SecuritySafeCritical]
        protected PtFixRejectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }



        /// <summary>
        /// GetObjectData performs a custom serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
