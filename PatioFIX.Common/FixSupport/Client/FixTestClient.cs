using PatioFIX.Common.Infrastructure;
using System;
using System.Threading;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class FixTestClient : IFixClient
    {
        bool _disposedValue;

        #region local state
        readonly Logger theLogger = new Logger("FixTestClient");
        readonly FixConfiguration m_settings;
        readonly SessionId m_sessionId;
        readonly MyCallbackTimer _timer;
        readonly FixTestFileEnumerator source;
        readonly IClock _clock = new RealTimeClock();
        readonly ManualResetEvent _receiveLogoutEvent = new ManualResetEvent(false);
        readonly object _clientLock = new object();
        bool m_isStarted = false;
        DateTime m_lastFixStopDT = DateTime.MinValue;
        #endregion


        public event Action<FIXMessage, ODLMesssageSource> NewMessageEvent;
        public event Action DisconnectEvent;
        public event Action<int, int> NewThrottlingPolicy;



        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => m_isStarted;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetConnection"></param>
        /// <returns></returns>
        public bool CanSend(string targetConnection)
        {
            return m_isStarted;
        }

        public bool IsStarted => m_isStarted;

        /// <summary>
        /// 
        /// </summary>
        public DateTime LastFixStopDT => m_lastFixStopDT;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="sessionsRootPath"></param>
        public FixTestClient(FixConfiguration settings, string sessionsRootPath)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(sessionsRootPath)) throw new ArgumentNullException(nameof(sessionsRootPath));

            /*
             * Θελουμε η περιοδος του _timer μας (SessionTimerInterval) να μην ειναι
             * μικροτερη απο 2 δευτερολεπτα, για να εχουν ικανο χρονικο διαστημα μεταξύ
             * τους τα διαφορα tasks που εκτελει
             */
            if (settings.SessionTimerInterval < 2000)
            {
                throw new PtFixException($"SessionTimerInterval cannot be smaller than 2000 ms!");
            }
            m_settings = settings;
            m_sessionId = new SessionId(settings.SenderCompID, settings.TargetCompID);


            theLogger.Info($".ctor, session = '{m_sessionId.Id}'");



            _timer = new MyCallbackTimer(m_settings.SessionTimerInterval, OnSessionTimer, "SessionTimer");


            if (Globals.Emulation.EmulateMessages)
            {
                this.source = new(Globals.Emulation.FixMessagesFile, Globals.Emulation.EmulatePausePeriod, settings);
            }

        }


        public void Dispose()
        {
            if (!_disposedValue)
            {
                theLogger?.Info("Dispose()");



                _disposedValue = true;
            }
        }

        void ResetInnerState()
        {
            _receiveLogoutEvent.Reset();
            _timer.Reset();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientStatus"></param>
        /// <exception cref="Exception"></exception>
        public void Start(ClientStatus clientStatus)
        {
            if (_disposedValue)
            {
                throw new Exception("FixClient instance is disposed");
            }

            if (m_isStarted == false)
            {
                lock (_clientLock)  //Start, Stop, OnSessionTimer, _forceStopImlementation
                {
                    if (m_isStarted == false)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
                        else
                            theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'.");

                        ResetInnerState();

                        //Ξεκιναμε τον timer
                        _timer.AuthoritativeStart(true);

                        if (Globals.Emulation.EmulateMessages)
                        {
                            //Ξεκιναμε το νημα που χειριζεται τα incoming
                            ThreadPool.QueueUserWorkItem(new WaitCallback(EmulateMessagesLoop));
                        }

                        m_isStarted = true;
                    }
                }
            }
            theLogger.Verbose("Start() leaving...");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Stop()
        {
            if (_disposedValue)
            {
                throw new Exception("FixClient instance is disposed");
            }

            if (m_isStarted == true)
            {
                lock (_clientLock)  //Start, Stop, OnSessionTimer, _forceStopImlementation
                {
                    if (m_isStarted == true)
                    {
                        if (Thread.CurrentThread.IsThreadPoolThread)
                            theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
                        else
                            theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'.");


                        //Σταματαμε τον timer....
                        _timer.AuthoritativeStop();


                        m_isStarted = false;
                        m_lastFixStopDT = DateTime.Now;
                    }
                }
            }
            theLogger.Verbose("Stop() leaving...");
        }



        /// <summary>
        /// Τρεχει απο ένα νήμα του ThreadPool
        /// </summary>
        /// <param name="state"></param>
        void OnSessionTimer(Object state)
        {
            try
            {
                #region Guard (just in case....)
                if (m_isStarted == false)
                {
                    /*Αυτο κανονικα δεν θα συμβει ποτε. Οι μεδθοδοι Start() & Stop() φροντιζουν να ανοιγοκλεινουν τον timer μας*/
                    theLogger.Error("OnSessionTimer() called, but m_isStarted == false");
                    return;
                }
                #endregion


                if (_timer.NumberOfHeartBeats % 10 == 0)
                {
                    //m_state.ShowInfo();
                }


                //Εαν δεν ειμαστε συνδεδεμενοι, συνδεομαστε

                //Στελνουμε το Logon

                //πήραμε το Logon??

                //Heartbeat

            }
            catch (PtFixNotReceivedLogon ex)
            {
                theLogger.Error(ex.Message);

            }
            catch (Exception ex)
            {
                theLogger.Error(ex.Message);

            }
            finally
            {
                _timer.Start();
            }
        }


        int _counter = 0;
        void EmulateMessagesLoop(Object state)
        {
            theLogger.Info("EmulateMessagesLoop starting...");

            foreach (var message in source.Enumerator())
            {
                if (m_isStarted == false)
                {
                    break;
                }


                if (Globals.Emulation.EmulatePausePeriod > 0)
                {
                    Thread.Sleep(Globals.Emulation.EmulatePausePeriod);
                }

                if (message.Valid == false)
                {
                    theLogger.Info($"OnInvalidMessage: - {message}");
                }
                else
                {
                    //Application Message
                    var _message = new FIXMessage(message);

                    NewMessageEvent?.Invoke(_message, m_settings.ClientRole);
                }

                _counter++;
            }

            theLogger.Warning("EmulateMessagesLoop terminated...");
        }


        public bool SendMessage(string messageType, FIXMessageWriter message)
        {
            return true;
        }

    }
}
