using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Αντιπροσωεπευει ενα μηνυμα που το αγνοουμε
    /// </summary>
    class IgnoredMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου IgnoredMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Ignored_Message;

    }
}
