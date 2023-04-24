namespace PatioFIX.Common
{
    /// <summary>
    /// TradeReportTransType (Tag = 487, Type: int)
    /// Identifies Trade Capture Report (AE) message transaction type 
    /// </summary>
    public enum TradeReportTransTypeEnum
    {
        Unknown = -1,
        /// <summary>
        /// 
        /// </summary>
        New = 0,
        /// <summary>
        /// 
        /// </summary>
        Cancel = 1,
        /// <summary>
        /// 
        /// </summary>
        Replace = 2
    }
}
