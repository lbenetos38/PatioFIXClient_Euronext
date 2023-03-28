using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FIXInMessage
    {
        /// <summary>
        /// Μας λεει απο που προηλθε αυτο το μήνυμα (DropCopy(Administrator) ή trader(Broker))
        /// </summary>
        public ODLMesssageSource Source { get; }

        /// <summary>
        /// MsgSeqNum (Tag = 34, Type: SeqNum)
        /// Integer message sequence number.
        /// </summary>
        public int MsgSeqNum { get; }
        /// <summary>
        /// PossResend (Tag = 97, Type: Boolean)
        /// Indicates that message may contain information that has been sent under another sequence number.
        /// </summary>
        public bool PossResend { get; }
        /// <summary>
        /// PossDupFlag (Tag = 43, Type: Boolean)
        /// Indicates possible retransmission of message with this sequence number
        /// </summary>
        public bool PossDupFlag { get; }
        /// <summary>
        /// SecondaryOrderID (Tag = 198, Type: String)
        /// ATHEX CUSTOM: "Unique application message id. Used in recovery mechanism."
        /// </summary>
        public int AppMsgID { get; }
        /// <summary>
        /// ATHEXSessionID (Tag = 5604, Type: String)
        /// ATHEX CUSTOM: "This user defined field specifies the unique identity of the session"
        /// </summary>
        public string ATHEXSessionID { get; }


        /// <summary>
        /// 
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType { get; }

        /// <summary>
        /// 
        /// </summary>
        public ATHEXServerEnum ATHEXServer
        {
            get
            {
                return ATHEXServerEnum.ETS;
            }
        }



        /// <summary>
        /// To FixMessage ετσι οπως το παραλαβαμε
        /// </summary>
        public FIXMessage Message { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inbound"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        public FIXInMessage(FIXMessage inbound, ODLMesssageSource source, ODLMessageTypeEnum type)
        {
            this.Message = inbound;
            this.Source = source;
            this.ODLMessageType = type;

            this.AppMsgID = inbound.SecondaryOrderID;
            this.MsgSeqNum = inbound.MsgSeqNum;
            this.PossResend = inbound.PossResend;
            this.PossDupFlag = inbound.PossDupFlag;
            this.ATHEXSessionID = inbound.ATHEXSessionID;
        }


        public override string ToString()
        {
            return Message?.ToString();
        }
    }
}
