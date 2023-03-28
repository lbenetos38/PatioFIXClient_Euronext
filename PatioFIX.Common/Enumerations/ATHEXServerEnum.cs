namespace PatioFIX.Common
{
    /// <summary>
    /// ATHEX Servers
    /// </summary>
    public enum ATHEXServerEnum
    {
        /// <summary>
        /// OASIS Trading System
        /// <para>
        /// By using the “ETS” connection, the ATHEX Gateway 
        /// passes the messages to the OASIS –CTCI Comm Server, 
        /// which finally sends them to the OASIS Server
        /// </para>
        /// </summary>
        ETS = 0,
        /// <summary>
        /// XNET server for routing order information to multiple venues/exchanges
        /// <para>
        /// By using the “ORA” connection, the ATHEX Gateway passes the messages 
        /// to the Xnet server, which finally sends them to the target exchange.
        /// </para>
        /// </summary>
        ORA = 1,
        /// <summary>
        /// DSS server (for Clearing)
        /// <para>
        /// By using the “DSS” connection, the ATHEX Gateway passes the 
        /// messages to the DSS server
        /// </para>
        /// </summary>
        DSS = 2
    }
}
