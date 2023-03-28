using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Security Price ("CD")
    /// </summary>
    class SecurityPricesMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου SecurityPricesMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL
            AccruedInterest = 0;
            #endregion


            SecurityID = message[Tags.SecurityID].AsString;                 //REQUIRED
            SecurityIDSource = message[Tags.SecurityIDSource].AsChar;       //REQUIRED
            SecurityExchange = message[Tags.SecurityExchange].AsString;     //REQUIRED

            //NoSecurityAltID
            if (message.Contains(Tags.SecurityAltID, instance: 0))
            {
                SecurityCode = message[Tags.SecurityAltID].AsString;
            }
            //SecurityAltIDSource 

            StartOfDayPrice = message[CustomTags.SecurityPrice].AsFloat;    //REQUIRED
            FloorPrice = message[Tags.LowPx].AsFloat;                       //REQUIRED
            CeilingPrice = message[Tags.HighPx].AsFloat;                    //REQUIRED
            if (message.Contains(Tags.AccruedInterestAmt, instance: 0))
            {
                AccruedInterest = message[Tags.AccruedInterestAmt].AsFloat;
            }

            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Security_Price;
        public string SecurityExchange { get; private set; }
        public string SecurityID { get; private set; }
        /// <summary>
        /// SecurityIDSource (Tag = 22, Type: String)
        /// </summary>
        internal char SecurityIDSource;
        public string SecurityCode { get; private set; }
        public double StartOfDayPrice { get; private set; }
        public double FloorPrice { get; private set; }
        public double CeilingPrice { get; private set; }
        public double AccruedInterest { get; private set; }
    }
}
