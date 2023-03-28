using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Exchange Notes ("TO")
    /// </summary>
    class ExchangeNotesMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου ExchangeNotesMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            this.MemberID = string.Empty;
            this.TraderID = string.Empty;
            this.ExchangeId = ' ';
            #endregion

            var linesOfText = message[Tags.LinesOfText].AsInt;          //REQUIRED
            MessageNote = message[Tags.Text].AsString;                     //REQUIRED


            if (message.Contains(CustomTags.NoteType))
            {
                /*
                    “0”: Free text message from the exchange
                    “1”: Warning. This informs the member firm that its rejections have reached half of the limit. Action should be taken to eliminate the cause of excessive rejections from the member.
                    “2”: Throttling parameters change. This informs the member firm that a change in their throttling parameters (TransPerSecond & OutstandingMsgs) has been made.
                 */
                NoteType = message[CustomTags.NoteType].AsChar;
            }
            else
            {
                /*
                 * In this case we pretent that the NoteType is a '0':
                 */
                NoteType = '0';
            }

            if (message.Contains(CustomTags.TransPerSecond))
                TransPerSecond = message[CustomTags.TransPerSecond].AsInt;
            if (message.Contains(CustomTags.OutstandingMsgs))
                OutstandingMsgs = message[CustomTags.OutstandingMsgs].AsInt;
            if (message.Contains(CustomTags.ExchangeID))
                ExchangeId = message[CustomTags.ExchangeID].AsChar;



            if (message.Contains(Tags.TransactTime))
            {
                Timestamp = message[Tags.TransactTime].AsODLTimestamp;
            }
            else if (message.Contains(Tags.OrigSendingTime))
            {
                Timestamp = message[Tags.OrigSendingTime].AsODLTimestamp;
            }
            else
            {
                Timestamp = message[Tags.SendingTime].AsODLTimestamp;
            }

            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Exchange_Notes;
        public string MemberID { get; private set; }
        public string TraderID { get; private set; }
        /// <summary>
        /// “0”: Free text message from the exchange
        /// “1”: Warning. This informs the member firm that its rejections have reached half of the limit. Action should be taken to eliminate the cause of excessive rejections from the member.
        /// “2”: Throttling parameters change. This informs the member firm that a change in their throttling parameters (TransPerSecond & OutstandingMsgs)
        /// </summary>
        public char NoteType { get; private set; }
        public string MessageNote { get; private set; }
        public string Timestamp { get; private set; }
        public int TransPerSecond { get; private set; }
        public int OutstandingMsgs { get; private set; }
        public char ExchangeId { get; private set; }
    }
}
