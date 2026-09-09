using System;
using System.Threading;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class CallBackTimer
    {
        readonly Logger theLogger;
        readonly System.Threading.Timer m_heartBeatTimer;
        readonly Int32 m_heartBeatInterval;
        bool m_issuedAuthoritativeStop = false;
        readonly object m_lock = new();
        readonly string m_timerName;


        public Int32 NumberOfHeartBeats { get; internal set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heartBeatInterval"></param>
        /// <param name="callback"></param>
        /// <param name="timerName"></param>
        public CallBackTimer(Int32 heartBeatInterval, TimerCallback callback, string timerName)
        {
            m_heartBeatInterval = heartBeatInterval;
            this.theLogger = new Logger(timerName);
            this.m_timerName = timerName;

            theLogger.Info($".ctor, heartBeatInterval = {heartBeatInterval} ms");

            m_heartBeatTimer = new Timer((state) =>
            {
                this.NumberOfHeartBeats++;
                callback(state);
            }, null, Timeout.Infinite, Timeout.Infinite);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startImmediately"></param>
        public void Start(bool startImmediately = false)
        {
            if (m_issuedAuthoritativeStop == false)
            {
                lock (m_lock)
                {
                    if (m_issuedAuthoritativeStop == false)
                    {
                        //m_logger.Info($"{m_timerName}.Start( startImmediately = {startImmediately} )");
                        m_heartBeatTimer.Change((startImmediately ? 0 : this.m_heartBeatInterval), Timeout.Infinite);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startImmediately"></param>
        public void AuthoritativeStart(bool startImmediately = false)
        {
            lock (m_lock)
            {
                theLogger.Info($"AuthoritativeStart( startImmediately = {startImmediately} )");
                m_issuedAuthoritativeStop = false;
                m_heartBeatTimer.Change((startImmediately ? 0 : this.m_heartBeatInterval), Timeout.Infinite);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            lock (m_lock)
            {
                theLogger.Info($"Stop()");
                m_heartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void AuthoritativeStop()
        {
            lock (m_lock)
            {
                theLogger.Info($"AuthoritativeStop()");
                m_issuedAuthoritativeStop = true;
                m_heartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Reset()
        {
            lock (m_lock)
            {
                theLogger.Info($"Reset()");
                m_heartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_issuedAuthoritativeStop = false;
                NumberOfHeartBeats = 0;
            }
        }
    }
}
