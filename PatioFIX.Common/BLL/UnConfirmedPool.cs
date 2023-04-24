using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UnConfirmedPool
    {
        readonly Logger theLogger = null;
        readonly System.Object _lockObject = new object();
        readonly IList<IOutboundMessage> m_messages = new List<IOutboundMessage>(12);
        Int32 totalAbandonedMessages = 0;
        Int32 liveAbandonedMessages = 0;
        readonly AutoResetEvent m_notifyEvent = new AutoResetEvent(false);
        readonly StringBuilder m_debugInfo = new StringBuilder();
        bool _showIsDisabledMarkAbandonedMessages = false;

        public AutoResetEvent NotifyEvent => m_notifyEvent;
        public int MaxUnconfirmedMessage { get; }
        public int AbandonedInterval { get; }



        public static readonly UnConfirmedPool Instance = new UnConfirmedPool();
        private UnConfirmedPool()
        {
            theLogger = new Logger("UnConfirmedPool");

            this.MaxUnconfirmedMessage = Globals.Dispatcher.MaxUnconfirmedMessage;
            this.AbandonedInterval = Globals.Dispatcher.AbandonedInterval;


            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Constructor called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Constructor called by '{Thread.CurrentThread.Name}'");

            theLogger.Info($"MaxUnconfirmedMessage={MaxUnconfirmedMessage}, AbandonedInterval={AbandonedInterval}");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                m_messages.Clear();
                totalAbandonedMessages = 0;
                liveAbandonedMessages = 0;
                m_notifyEvent.Reset();
            }

            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Reset() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Reset() called by '{Thread.CurrentThread.Name}'");
        }


        /// <summary>
        /// στειλαμε μηνυμα
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Add(IOutboundMessage message)
        {
            lock (_lockObject)
            {
                if (Globals.Dispatcher.LogUnConfirmedPool)
                {
                    theLogger.Verbose(string.Format("Add, Type={0}, OrderID={1}", message.ODLMessageType, message.OrderID));
                }

                //Μηπως το εχουμε ξαναβάλει?
                for (int idx = m_messages.Count - 1; idx >= 0; --idx)
                {
                    var item = m_messages[idx];

                    if (item._UnConfirmedPool_abandoned)
                        continue;

                    if (item.RowID == message.RowID)
                        return true;
                }

                //Εχουμε χωρο?
                if ((m_messages.Count - liveAbandonedMessages) >= this.MaxUnconfirmedMessage)
                {
                    theLogger.Warning($"NO SLOT AVAILABLE, message {message.OrderID} NOT INSERTED!");
                    //MetricsProxy.
                    return false;
                }


                message._UnConfirmedPool_ticks = DateTime.Now.Ticks;

                m_messages.Add(message);



                MetricsProxy.Instance.UnConfirmedPoolMessages(m_messages.Count);

                if (Globals.Dispatcher.LogUnConfirmedPoolMessages)
                {
                    _logMessages();
                }
                return true;
            }
        }


        /// <summary>
        /// Μπορουμε να την καλεσουμε για να αφαιρεσουμε ένα IOutboundMessage μετα απο πετυχημενο Add
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool RemoveInternal(IOutboundMessage message)
        {
            lock (_lockObject)
            {
                if (Globals.Dispatcher.LogUnConfirmedPool)
                {
                    theLogger.Verbose(string.Format("RemoveInternal, Type={0}, OrderID={1}", message.ODLMessageType, message.OrderID));
                }

                for (int idx = m_messages.Count - 1; idx >= 0; --idx)
                {
                    var item = m_messages[idx];

                    if (item.RowID == message.RowID)
                    {
                        m_messages.RemoveAt(idx);

                        MetricsProxy.Instance.UnConfirmedPoolMessages(m_messages.Count);
                        if (Globals.Dispatcher.LogUnConfirmedPoolMessages)
                        {
                            _logMessages();
                        }

                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowID"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public bool Remove(int rowID, ODLMessageTypeEnum messageType)
        {
            bool __lockWasTaken = false;
            try
            {
                Monitor.Enter(_lockObject, ref __lockWasTaken);
                if (__lockWasTaken)
                {
                    if (Globals.Dispatcher.LogUnConfirmedPool)
                    {
                        theLogger.Verbose(string.Format("Remove, Type={0}, RowID={1}", messageType, rowID));
                    }

                    for (int idx = m_messages.Count - 1; idx >= 0; --idx)
                    {
                        var item = m_messages[idx];

                        if (item.RowID == rowID)
                        {
                            m_messages.RemoveAt(idx);

                            MetricsProxy.Instance.UnConfirmedPoolMessages(m_messages.Count);
                            if (Globals.Dispatcher.LogUnConfirmedPoolMessages)
                            {
                                _logMessages();
                            }

                            return true;
                        }
                    }

                    return false;
                }
            }
            finally
            {
                if (__lockWasTaken)
                {
                    System.Threading.Monitor.Exit(_lockObject);
                    m_notifyEvent.Set();
                }
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClOrdID"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public bool Remove(string ClOrdID, ODLMessageTypeEnum messageType)
        {
            Int32 rowID = 0;
            if (Int32.TryParse(ClOrdID, out rowID))
            {
                return Remove(rowID, messageType);
            }
            return false;
        }


        /// <summary>
        /// Υπαρχει χωρος στο uncomfirmed queue να στειλω
        /// </summary>
        /// <returns></returns>
        public bool CanSend()
        {
            if (this.AbandonedInterval > 0)
            {
                lock (_lockObject)
                {
                    int counter = 0;
                    for (int i = 0; i < m_messages.Count; i++)
                    {
                        if (m_messages[i]._UnConfirmedPool_abandoned == false)
                            counter++;
                    }
                    return counter < this.MaxUnconfirmedMessage;
                }
            }
            else
            {
                lock (_lockObject)
                {
                    return m_messages.Count < this.MaxUnconfirmedMessage;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int MarkAbandonedMessages()
        {
            if (this.AbandonedInterval > 0)
            {
                #region MarkAbandonedMessages algorithm
                long _nowTicks = DateTime.Now.Ticks;
                long _threshold = TimeSpan.TicksPerSecond * this.AbandonedInterval;

                lock (_lockObject)
                {
                    int alreadyAbandonedMessages = 0;
                    int numOfNewAbandonedMessages = 0;


                    for (int idx = m_messages.Count - 1; idx >= 0; --idx)
                    {
                        var item = m_messages[idx];

                        if (item._UnConfirmedPool_abandoned)
                        {
                            alreadyAbandonedMessages++;
                            continue;
                        }

                        if ((_nowTicks - item._UnConfirmedPool_ticks) > _threshold)
                        {
                            numOfNewAbandonedMessages++;
                            totalAbandonedMessages++;
                            item._UnConfirmedPool_abandoned = true;
                        }
                    }

                    liveAbandonedMessages = alreadyAbandonedMessages + numOfNewAbandonedMessages;

                    if (numOfNewAbandonedMessages > 0)
                    {
                        theLogger.Info(string.Format("MarkAbandonedMessages, numOfNewAbandonedMessages={0}, liveAbandonedMessages={1}, totalAbandonedMessages={2}", numOfNewAbandonedMessages, liveAbandonedMessages, totalAbandonedMessages));
                    }
                    return numOfNewAbandonedMessages;
                }
                #endregion
            }
            else
            {
                #region MarkAbandonedMessages DISABLED
                if (_showIsDisabledMarkAbandonedMessages == false)
                {
                    theLogger.Info("MarkAbandonedMessages, IS DISABLED, AbandonedInterval == 0 seconds");
                    _showIsDisabledMarkAbandonedMessages = true;
                }
                return 0;
                #endregion
            }
        }


        /// <summary>
        /// 
        /// </summary>
        void _logMessages()
        {
            m_debugInfo.Clear();

            m_debugInfo.Append($"m_messages<{m_messages.Count}>:: ");
            foreach (var item in m_messages)
            {
                if (this.AbandonedInterval > 0)
                {
                    #region MarkAbandonedMessages ENABLED
                    if (item._UnConfirmedPool_abandoned)
                        m_debugInfo.Append($"A{item.RowID}, ");
                    else
                        m_debugInfo.Append($"_{item.RowID}, ");
                    #endregion
                }
                else
                {
                    #region MarkAbandonedMessages DISABLED
                    m_debugInfo.Append($"{item.RowID}, ");
                    #endregion
                }
            }

            theLogger.Verbose(m_debugInfo.ToString());
        }
    }
}
