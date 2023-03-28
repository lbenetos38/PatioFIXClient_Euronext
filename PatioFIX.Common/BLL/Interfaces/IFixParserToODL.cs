using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    interface IFixParserToODL
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        IODLMessage ParseFixMessage(FIXMessage message, Logger logger);
    }
}
