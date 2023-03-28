using PatioFIX.Common.FixSupport;
using System;

namespace PatioFIX.Common
{
    public interface IFixClient : IDisposable
    {
        /// <summary>
        /// Ξεκινα τον FixClient
        /// </summary>
        /// <param name="clientStatus"></param>
        void Start(ClientStatus clientStatus);
        /// <summary>
        /// Σταματα τον FixClient
        /// </summary>
        void Stop();

        /// <summary>
        /// 
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Μας λεει εαν υπαρχει η συνδεση στο φυσικο επιπεδο (TCP Socket)
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Αποθηκευει το ποτε "σταματησε" ο FixClient
        /// </summary>
        public DateTime LastFixStopDT { get; }
        
        /// <summary>
        /// Μας λεει εαν μπορουμε να στείλουμε στον FIX Server πακέτα
        /// </summary>
        /// <param name="targetConnection"></param>
        /// <returns></returns>
        bool CanSend(string targetConnection);


        bool SendMessage(string messageType, FIXMessageWriter message);


        event Action<FIXMessage, ODLMesssageSource> NewMessageEvent;
        event Action DisconnectEvent;
        event Action<int, int> NewThrottlingPolicy;

    }
}
