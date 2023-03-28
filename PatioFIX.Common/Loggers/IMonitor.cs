using System;
using System.Diagnostics;

namespace PatioFIX.Common
{
    public interface IMonitor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <param name="ownerName"></param>
        void ShowMessage(TraceLevel level, string msg, string ownerName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="ex"></param>
        /// <param name="ownerName"></param>
        void ShowException(TraceLevel level, Exception ex, string ownerName);
    }
}
