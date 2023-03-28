using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// An abstraction over time.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Returns the current universal time.
        /// </summary>
        DateTime Time { get; }
    }
}
