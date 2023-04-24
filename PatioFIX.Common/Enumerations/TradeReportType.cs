namespace PatioFIX.Common
{
    /// <summary>
    /// TradeReportType (Tag = 856, Type: int)
    /// Type of Trade Report
    /// </summary>
    public enum TradeReportTypeEnum : int
    {
        Unknown = -1,
        /// <summary>
        /// 
        /// </summary>
        Submit = 0,
        /// <summary>
        /// 
        /// </summary>
        Alleged = 1,
        /// <summary>
        /// 
        /// </summary>
        Accept = 2,
        /// <summary>
        /// 
        /// </summary>
        Decline = 3,
        /// <summary>
        /// 
        /// </summary>
        Expired = 5,
        /// <summary>
        /// 
        /// </summary>
        TradeReportCancel = 6
    }
}
