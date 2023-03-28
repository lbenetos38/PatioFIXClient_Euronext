using PatioFIX.Common.FixSupport;
using System;

namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// Credit Limit Information ("TL")
    /// </summary>
    class CreditLimitInfoMessage : IODLMessage, IFixParserToODL
    {
        /// <summary>
        /// Παρσαρει το FixMessage και συμπληρώνει τις τιμες του συγκεκριμενου CreditLimitInfoMessage Instance
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public IODLMessage ParseFixMessage(FIXMessage message, Logger logger)
        {
            #region default τιμες οπως ειναι απο τον ODL

            #endregion

            var linesOfText = message[Tags.LinesOfText].AsInt;          //REQUIRED
            var text = message[Tags.Text].AsString;                     //REQUIRED

            //repeating group, <Parties> Component Block
            var noPartyIDs = message[Tags.NoPartyIDs].AsInt;            //REQUIRED
            for (int idx = 0; idx < noPartyIDs; idx++)
            {
                var partyIDSource = message[Tags.PartyIDSource, idx].AsString;  //REQUIRED
                var partyID = message[Tags.PartyID, idx].AsString;              //REQUIRED
                var partyRole = message[Tags.PartyRole, idx].AsInt;             //REQUIRED

                /*
                 */
                if (partyRole == /*Executing Firm*/1)
                {
                    MemberID = partyID;
                }
                else if (partyRole == /*Clearing Firm */4)
                {
                    ClearingSubAccountID = partyID;
                }
                else
                {
                    throw new Exception($"UNEXPECTED PartyRole '{partyRole}'");
                }
            }

            ClearingSpace = message[CustomTags.ClearingSpace].AsString;      //REQUIRED
            CreditLimit = message[CustomTags.CreditLimit].AsFloat;           //REQUIRED

            return this;
        }



        /// <summary>
        /// Ο τυπος αυτου του ODL μηνυματος
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Credit_Limit_Information;

        /// <summary>
        /// 
        /// </summary>
        public string MemberID { get; private set; }
        public string ClearingSpace { get; private set; }
        public string ClearingSubAccountID { get; private set; }
        public double CreditLimit { get; private set; }
    }
}
