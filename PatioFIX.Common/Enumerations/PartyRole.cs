namespace PatioFIX.Common.Enumerations
{
    /// <summary>
    /// Supported values for PartyRole (Tag = 452, Type: int)
    /// </summary>
    public static class PartyRole
    {
        public const int ExecutingFirm = 1;
        public const int ClientID = 3;
        public const int ClearingFirm = 4;
        /// <summary>
        /// (MIFID II: Execution within firm)
        /// </summary>
        public const int ExecutingTrader = 12;
        public const int ContraFirm = 17;
        /// <summary>
        /// (MIFID II: Non-executing broker)
        /// </summary>
        public const int CorrespondentBroker = 26;
        public const int EnteringTrader = 36;
        public const int ContraTrader = 37;
        /// <summary>
        /// (MIFID II: Investment decision within firm)
        /// </summary>
        public const int InvestmentDecisionMaker = 122;
    }
}
