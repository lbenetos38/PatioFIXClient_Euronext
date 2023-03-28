namespace PatioFIX.Common
{
    /// <summary>
    /// Identifies current status of order.
    /// </summary>
    public enum OrderStatusCodeEnum : int
    {
        Unknown = -1,
        /// <summary>
        /// 0          New
        /// </summary>
        New = 0,
        /// <summary>
        /// 1          Partially Filled
        /// </summary>
        PartiallyFilled = 1,
        /// <summary>
        /// 2          Filled   
        /// </summary>
        Filled = 2,
        /// <summary>
        /// 3          Done for day
        /// </summary>
        DoneForDay = 3,
        /// <summary>
        /// 4          Canceled
        /// </summary>
        Canceled = 4,
        /// <summary>
        /// 5          Replaced
        /// </summary>
        Replaced = 5,
        /// <summary>
        /// 6          Pending Cancel
        /// PendingCancel, ResultOfOrderCancelRequest
        /// </summary>
        PendingCancel = 6,
        /// <summary>
        /// 7          Stopped
        /// </summary>
        Stopped = 7,
        /// <summary>
        /// 8          Rejected
        /// </summary>
        Rejected = 8,
        /// <summary>
        /// 9          Suspended
        /// </summary>
        Suspended = 9,
        /// <summary>
        /// A          Pending New
        /// </summary>
        PendingNew = 0xA,
        /// <summary>
        /// B          Calculated
        /// </summary>
        Calculated = 0xB,
        /// <summary>
        /// C          Expired
        /// </summary>
        Expired = 0xC,
        /// <summary>
        /// D          Accepted for bidding
        /// </summary>
        AcceptedForBidding = 0xD,
        /// <summary>
        /// E          Pending replace
        /// PendingReplace, ResultOfOrderCancelReplaceRequest
        /// </summary>
        PendingReplace = 0xE,
    }
}
