namespace PatioFIX.Common
{
    /// <summary>
    /// MatchStatus (Tag = 573, Type: char)
    /// The status of this trade with respect to matching or comparison.
    /// </summary>
    public enum MatchStatusEnum : int
    {
        Unknown = -1,
        /// <summary>
        /// '0' 	compared, matched or affirmed
        /// </summary>
        Matched = 0,
        /// <summary>
        /// '1' 	uncompared, unmatched, or unaffirmed
        /// </summary>
        UnMatched = 1
    }
}
