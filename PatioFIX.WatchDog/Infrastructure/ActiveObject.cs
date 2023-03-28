using System;
using System.Globalization;
using System.Threading;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    public class ActiveObject
    {
        #region local variables
        protected readonly string m_threadName;
        protected readonly Logger theLogger = null;

        protected readonly Timer m_timer;
        protected readonly int m_timerInterval;
        readonly object m_timerLock = new();
        bool m_issuedAuthoritativeStop = false;
        protected Int32 totalHeartBeats = 0;

        readonly WaitHandle[] m_waitHandles;
        readonly AutoResetEvent m_beatEvent = new(false);
        readonly AutoResetEvent m_startEvent = new(false);
        readonly AutoResetEvent m_stopEvent = new(false);
        readonly AutoResetEvent m_quitEvent = new(false);
        readonly AutoResetEvent m_customEvent = new(false);     //for user use
        readonly AutoResetEvent m_queueEvent = new(false);      //for user use
        #endregion


        /// <summary>
        /// Ελεγχει εαν τα heartbeats των νημάτων επιβραδύνονται τις νυχτερινες ώρες
        /// <para>Νυχτερινες ωρες θεωρούμε το διαστημα απο 21:00 - 08:00 (το πρωι της επομενς ημέρας)</para>
        /// <para>Χρησιμευει για να μετριαστούν οι εγγραφες στα logs της εφαρμογης μας</para>
        /// </summary>
        public bool EnableNightTimeSlow { get; set; }
        /// <summary>
        /// Ελεγχει εαν τα heartbeats των νημάτων επιβραδύνονται το Σαββατο και την Κυριακη
        /// <para>Χρησιμευει για να μετριαστούν οι εγγραφες στα logs της εφαρμογης μας</para>
        /// </summary>
        public bool EnableWeekEndSlow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AutoResetEvent CustomEvent => m_customEvent;
        /// <summary>
        /// 
        /// </summary>
        public AutoResetEvent QueueEvent => m_queueEvent;

        /// <summary>
        /// 
        /// </summary>
        public Int32 NumberOfHeartBeats { get; internal set; } = 0;


        public void Start()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'");

            m_startEvent.Set();
        }
        public void Stop()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'");

            m_stopEvent.Set();
        }
        public void Quit()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Quit() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Quit() called by '{Thread.CurrentThread.Name}'");

            m_quitEvent.Set();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadName"></param>
        /// <param name="timerInterval"></param>
        public ActiveObject(string threadName, int timerInterval = 0)
        {
            if (timerInterval <= 0)
            {
                throw new ArgumentException("Invalid value", nameof(timerInterval));
            }
            if (string.IsNullOrWhiteSpace(threadName))
            {
                throw new ArgumentException("Invalid value", nameof(threadName));
            }

            m_threadName = threadName;
            //Logger
            theLogger = new Logger(threadName);
            //theLogger.Verbose(string.Format(CultureInfo.InvariantCulture, "AO .ctor(), threadName = {0}, timerInterval={1}", threadName, timerInterval));

            //timer
            m_timerInterval = timerInterval;
            m_timer = new Timer((state) =>
            {
                this.NumberOfHeartBeats++;
                m_beatEvent.Set();
            }, null, Timeout.Infinite, Timeout.Infinite);


            m_waitHandles = new WaitHandle[]{
                m_startEvent,
                m_stopEvent,
                m_quitEvent,
                m_beatEvent,
                m_customEvent,
                m_queueEvent
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback(ActiveLoop));
        }


        Int32 GetTimerInterval()
        {
            if (this.EnableNightTimeSlow == false && this.EnableWeekEndSlow == false)
            {
                return this.m_timerInterval;
            }
            else
            {
                var _hBeatInterval = this.m_timerInterval;
                DateTime now = DateTime.Now;

                if (this.EnableNightTimeSlow)
                {
                    int hh = now.Hour;
                    if (hh == 22 || hh == 23 || hh == 0 || hh == 1 || hh == 2 || hh == 3 || hh == 4 || hh == 5 || hh == 6)
                    {
                        _hBeatInterval = 4 * _hBeatInterval;
                    }
                    if (hh == 21 || hh == 7)
                    {
                        _hBeatInterval = 2 * _hBeatInterval;
                    }
                }
                if (this.EnableWeekEndSlow)
                {
                    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        _hBeatInterval = _hBeatInterval * 2;
                    }
                }

                return _hBeatInterval;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startImmediately"></param>
        protected void StartTimer(bool startImmediately = false)
        {
            if (m_issuedAuthoritativeStop == false)
            {
                lock (m_timerLock)
                {
                    if (m_issuedAuthoritativeStop == false)
                    {
                        //theLogger.Verbose($"StartTimer( startImmediately = {startImmediately} )");
                        m_timer.Change((startImmediately ? 0 : GetTimerInterval()), Timeout.Infinite);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startImmediately"></param>
        protected void StartTimerAuthoritative(bool startImmediately = false)
        {
            lock (m_timerLock)
            {
                theLogger.Info($"StartTimerAuthoritative( startImmediately = {startImmediately} )");
                m_issuedAuthoritativeStop = false;
                m_timer.Change((startImmediately ? 0 : GetTimerInterval()), Timeout.Infinite);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void StopTimer()
        {
            lock (m_timerLock)
            {
                //theLogger.Verbose($"StopTimer()");
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void StopTimerAuthoritative()
        {
            lock (m_timerLock)
            {
                theLogger.Info($"StopTimerAuthoritative()");
                m_issuedAuthoritativeStop = true;
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }



        void ActiveLoop(object state)
        {
            try
            {
                OnInitializeThread();

                bool quit = false;
                while (!quit)
                {
                    var index = WaitHandle.WaitAny(m_waitHandles);

                    if (index == 0)
                    {
                        //m_startEvent
                        OnStartEvent();
                    }
                    else if (index == 1)
                    {
                        //m_stopEvent
                        quit = OnStopEvent();
                    }
                    else if (index == 2)
                    {
                        //m_quitEvent
                        quit = OnQuitEvent();
                    }
                    else if (index == 3)
                    {
                        //m_beatEvent
                        OnHeartBeatEvent();
                    }
                    else if (index == 4)
                    {
                        //m_customEvent
                        OnCustomEvent();
                    }
                    else if (index == 5)
                    {
                        //m_customEvent
                        OnQueueEvent();
                    }
                }

                OnExitingThread();
            }
            catch (Exception ex)
            {
                theLogger.Error(string.Format(CultureInfo.InvariantCulture, "AO<{0}>.ActiveLoop(), Exception={1}!", m_threadName, ex.Message));
            }
            finally
            {

            }
        }


        protected virtual void OnInitializeThread() { }

        protected virtual void OnStartEvent() { }
        protected virtual bool OnStopEvent() { return false; }
        protected virtual bool OnQuitEvent() { return true; }
        protected virtual void OnHeartBeatEvent() { }
        protected virtual void OnCustomEvent() { }
        protected virtual void OnQueueEvent() { }
        protected virtual void OnExitingThread() { }

    }
}
