namespace PatioFIX.Common
{
    public static class CustomTags
    {
        public const int MarketID = 5502;
        public const int BoardID = 5506;
        public const int PhaseID = 5511;
        public const int SecurityStatus = 5522;
        public const int ATHEXTradeType = 5529;
        public const int SecurityPrice = 5530;
        public const int ΑΤΗΕΧHaltReason = 5531;
        public const int ATHEXMsgType = 5574;
        public const int ATHEXSessionID = 5604;

        /// <summary>
        /// News (Credit Limit Info)
        /// </summary>
        public const int CreditLimit = 5550;
        public const int ClearingSpace = 5558;

        /// <summary>
        /// News (Message Note)
        /// </summary>
        public const int NoteType = 5577;
        public const int TransPerSecond = 5601;
        public const int OutstandingMsgs = 5602;
        public const int ExchangeID = 5603;


        /// <summary>
        /// A 1 character alphanumeric type indicating the source of the Orde
        /// C               CTCI –API / ATHEX FIX Serve
        /// M               ORAMA
        /// R               MRW (ATHEX supervision application)
        /// [space]         OASIS
        /// </summary>
        public const int OrigSource = 5501;
        public const int OrderRelFlag = 5509;
        public const int OrderRefID = 5510;
        public const int GOIFlag = 5512;
        public const int StopSymbol = 5521;
        public const int StopSymbolType = 5527;
        public const int ΑΤΗΕΧTradeType = 5529;
        public const int RejectReasonCode = 5532;
        public const int CurrentCreditValue = 5545;
        public const int MBListID = 5561;
        public const int CancelReasonCode = 5508;
        public const int OrderOrigination = 1724;

        public const int NoOrderAttributes = 2593;
        public const int OrderAttributeType = 2594;
        public const int OrderAttributeValue = 2595;

        public const int NoTrdRegPublications = 2668;
        public const int TrdRegPublicationType = 2669;
        public const int TrdRegPublicationReason = 2670;

        public const int PartyRoleQualifier = 2376;





    }
}
